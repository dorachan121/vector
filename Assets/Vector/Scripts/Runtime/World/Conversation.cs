using System;
using System.Collections;
using System.Collections.Generic;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>
    /// 登場人物との会話の流れ（はじめての挨拶 → 達成したクエストの報酬 → 主の挑戦 → 進行中のクエスト → 新しい依頼 → 世間話）と、
    /// 主の挑戦の決着。規則の判断は RpgSession に任せ、ここは台詞の順番と選択肢だけを扱う。
    /// </summary>
    public sealed class Conversation
    {
        static readonly string[] QuestOptions = { "引き受ける", "あとで" };

        readonly StoryProgress story;
        readonly DialogueBox box;
        readonly Toast toast;
        readonly Action<ChallengeSpec> startChallenge;
        readonly Random random = new Random();

        public Conversation(StoryProgress story, DialogueBox box, Toast toast, Action<ChallengeSpec> startChallenge)
        {
            this.story = story;
            this.box = box;
            this.toast = toast;
            this.startChallenge = startChallenge;
        }

        RpgSession Session => story.Session;
        RpgState State => story.State;

        /// <summary>頭の上の印。"?" は報酬を受け取れる、"!" は話すことがある（はじめて会う・依頼・挑戦）、null は何もない。</summary>
        public string MarkFor(string id)
        {
            var active = Session.ActiveFrom(id);
            if (active != null && active.done)
                return "?";
            if (!State.Has(StoryFlags.Met(id)))
                return "!";
            var spec = Floors.OfMaster(id)?.Master;
            if (spec != null)
                return !Session.HasDefeated(id) && State.level >= spec.RequiredLevel ? "!" : null;
            return active == null && Session.OfferFrom(id) != null ? "!" : null;
        }

        public IEnumerator Talk(string id)
        {
            var member = Cast.Of(id);
            bool firstMeeting = !State.Has(StoryFlags.Met(id));
            if (firstMeeting)
            {
                foreach (var line in member.Intro)
                    yield return Say(id, line);
                State.Set(StoryFlags.Met(id));
                story.Save();
            }

            var active = Session.ActiveFrom(id);
            if (active != null && active.done)
            {
                yield return ClaimQuest(id, QuestBook.Find(active.id));
                yield break;
            }
            var spec = Floors.OfMaster(id)?.Master;
            if (spec != null)
            {
                yield return OfferChallenge(id, spec);
                yield break;
            }
            if (active != null)
            {
                yield return Say(id, string.Format(LinesFor(id, active.id).Progress, Remaining(QuestBook.Find(active.id), active)));
                yield break;
            }
            var offer = Session.OfferFrom(id);
            if (offer != null)
                yield return OfferQuest(id, offer);
            else if (!firstMeeting)
                yield return SmallTalk(id);
        }

        /// <summary>テーブルで決着した挑戦の、主の台詞と報酬を見せる（結果はテーブル側で確定済み）。</summary>
        public IEnumerator Conclude(ChallengeReward reward)
        {
            string master = reward.Spec.Master;
            var lines = Cast.Of(master).Master;
            if (!reward.Won)
            {
                yield return Say(master, lines.Lose);
                yield break;
            }
            Sfx.Play(SfxKind.BigWin);
            if (!reward.FirstVictory)
            {
                toast.Show($"再挑戦に勝利！  XP +{reward.Xp}", Theme.GoldBright);
                yield return Say(master, lines.AfterWin);
                yield break;
            }
            toast.Show($"勝利！  賞金 {Labels.Money(reward.Prize)}  ·  XP +{reward.Xp}", Theme.GoldBright);
            yield return Say(master, lines.Win);
            if (reward.Trophy != null)
                yield return Narrate($"記念品「{StoryNames.Item(reward.Trophy)}」を手に入れた。");
            foreach (var floor in Floors.All)
            {
                if (floor.UnlockedBy == master)
                    yield return Narrate($"{StoryNames.Floor(floor.Id)}（{StoryNames.FloorJapanese(floor.Id)}）に行けるようになった！\nエレベーターから向かおう。");
            }
            if (Floors.OfMaster(master).Id == FloorId.Penthouse && !State.Has(StoryFlags.Ending))
            {
                foreach (var line in Cast.Ending)
                    yield return Narrate(line);
                State.Set(StoryFlags.Ending);
                story.Save();
            }
        }

        IEnumerator ClaimQuest(string id, QuestDef quest)
        {
            yield return Say(id, LinesFor(id, quest.Id).Done);
            if (!Session.Claim(quest.Id, out int levelsGained))
                yield break;
            story.Save();
            Sfx.Play(SfxKind.ChipsCollect);
            string levelUp = levelsGained > 0 ? $"  ·  LEVEL UP! Lv {State.level}" : "";
            toast.Show($"報酬: {RewardText(quest)}{levelUp}", Theme.GoldBright);
        }

        IEnumerator OfferQuest(string id, QuestDef quest)
        {
            var lines = LinesFor(id, quest.Id);
            int choice = -1;
            yield return box.Choose(StoryNames.Person(id), StoryNames.Title(id), Cast.Of(id).Look, lines.Offer, QuestOptions, c => choice = c);
            if (choice != 0 || !Session.Accept(quest.Id))
                yield break;
            story.Save();
            toast.Show($"クエスト開始: {StoryNames.Quest(quest.Id)}", Theme.GoldBright);
            yield return Say(id, lines.Accepted);
            // 受けた時点で条件を満たしている（レベルのクエストなど）なら、その場で報酬を渡す
            var progress = State.Quest(quest.Id);
            if (progress.done)
                yield return ClaimQuest(id, quest);
        }

        IEnumerator OfferChallenge(string id, ChallengeSpec spec)
        {
            var member = Cast.Of(id);
            var lines = member.Master;
            if (Session.CheckChallenge(spec) == ChallengeBlock.Level)
            {
                yield return Say(id, string.Format(lines.NotReady, spec.RequiredLevel));
                yield break;
            }
            if (Session.HasDefeated(id))
                yield return Say(id, lines.AfterWin);
            string terms = string.Format(lines.Terms, Labels.Money(spec.EntryFee), Labels.Money(spec.Stack), spec.Rounds, Labels.Money(spec.Target));
            var options = new[] { $"挑戦する（参加費 {Labels.Money(spec.EntryFee)}）", "やめておく" };
            int choice = -1;
            yield return box.Choose(StoryNames.Person(id), StoryNames.Title(id), member.Look, terms, options, c => choice = c);
            if (choice != 0)
            {
                yield return SmallTalk(id);
                yield break;
            }
            if (Session.CheckChallenge(spec) == ChallengeBlock.Funds)
            {
                yield return Narrate("所持金が参加費に足りません。キャッシャーでチップを補充するか、引換券を使いましょう。");
                yield break;
            }
            yield return Say(id, lines.Accepted);
            startChallenge(spec);
        }

        IEnumerator Say(string id, string text) => box.Say(StoryNames.Person(id), StoryNames.Title(id), Cast.Of(id).Look, text);

        IEnumerator SmallTalk(string id)
        {
            var idle = Cast.Of(id).Idle;
            if (idle.Length > 0)
                yield return Say(id, idle[random.Next(idle.Length)]);
        }

        IEnumerator Narrate(string text) => box.Say("", "", null, text);

        static QuestLines LinesFor(string id, string quest)
        {
            if (!Cast.Of(id).Quests.TryGetValue(quest, out var lines))
                throw new InvalidOperationException($"{id} にクエスト {quest} の台詞がありません。");
            return lines;
        }

        /// <summary>途中の台詞の {0}（レベルのクエストは今のレベル、ほかは残りの回数）。</summary>
        int Remaining(QuestDef quest, QuestProgress progress)
            => quest.Kind == QuestKind.ReachLevel ? State.level : Math.Max(0, quest.Target - progress.count);

        static string RewardText(QuestDef quest)
        {
            var parts = new List<string>();
            if (quest.Xp > 0)
                parts.Add($"XP +{quest.Xp}");
            if (quest.Comps > 0)
                parts.Add($"COMP +{quest.Comps}");
            if (quest.Chips > 0)
                parts.Add(Labels.Money(quest.Chips));
            if (quest.Item != null)
                parts.Add(StoryNames.Item(quest.Item));
            return string.Join("  ·  ", parts);
        }
    }
}
