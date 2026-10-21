using System;
using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// ストーリーモードの規則を状態に当てはめる窓口（経験値・コンプ・クエスト・景品の交換・フロアの主への挑戦）。
    /// チップの出し入れは渡された口座（所持金）に対して行う。表示や保存はしない（呼び出し側の仕事）。
    /// </summary>
    public sealed class RpgSession
    {
        readonly IChipAccount chips;

        public RpgSession(RpgState state, IChipAccount chips) : this(state, chips, () => 0)
        {
        }

        /// <param name="state">進み具合（保存する状態）。</param>
        /// <param name="chips">所持金（報酬の受け取りと参加費の支払い）。</param>
        /// <param name="today">今日の日付の通し番号（ログインボーナスとデイリーミッションの日付）。</param>
        public RpgSession(RpgState state, IChipAccount chips, Func<int> today)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            this.chips = chips ?? throw new ArgumentNullException(nameof(chips));
            Daily = new DailyProgress(state, chips, today);
        }

        public RpgState State { get; }

        /// <summary>ログインボーナスとデイリーミッション。</summary>
        public DailyProgress Daily { get; }

        public VipRank Rank => Progression.RankFor(State.level);

        // ------------------------------------------------------------------
        // テーブルでの1ラウンド
        // ------------------------------------------------------------------

        /// <summary>精算したラウンドを集計する（経験値とコンプは賭けた額だけで決まる）。</summary>
        public RoundReport OnRound(RoundEvent round)
        {
            var report = new RoundReport();
            var rankBefore = Rank;
            State.wagered += round.Stake;
            report.Comps = Progression.CompsForStake(round.Stake, rankBefore);
            State.comps += report.Comps;
            report.Xp = Math.Max(1, Progression.XpForStake(round.Stake));
            report.LevelsGained = Progression.AddXp(State, report.Xp);

            foreach (var progress in State.quests)
            {
                if (progress.done)
                    continue;
                var quest = QuestBook.Find(progress.id);
                if (Counts(quest, round))
                    progress.count++;
                if (IsComplete(quest, progress))
                {
                    progress.done = true;
                    report.CompletedQuests.Add(quest);
                }
            }
            CheckLevelQuests(report.CompletedQuests);
            report.CompletedDailies.AddRange(Daily.OnRound(round));
            if (Rank != rankBefore)
                report.NewRank = Rank;
            return report;
        }

        static bool Counts(QuestDef quest, RoundEvent round)
        {
            switch (quest.Kind)
            {
                case QuestKind.PlayRounds: return quest.Game == null || quest.Game == round.Game;
                case QuestKind.WinRounds: return round.Won && (quest.Game == null || quest.Game == round.Game);
                case QuestKind.Event: return round.Has(quest.Tag);
                default: return false;
            }
        }

        bool IsComplete(QuestDef quest, QuestProgress progress)
            => quest.Kind == QuestKind.ReachLevel ? State.level >= quest.Target : progress.count >= quest.Target;

        void CheckLevelQuests(List<QuestDef> completed)
        {
            foreach (var progress in State.quests)
            {
                var quest = QuestBook.Find(progress.id);
                if (!progress.done && quest.Kind == QuestKind.ReachLevel && State.level >= quest.Target)
                {
                    progress.done = true;
                    completed.Add(quest);
                }
            }
        }

        // ------------------------------------------------------------------
        // クエスト
        // ------------------------------------------------------------------

        /// <summary>その人から次に受けられるクエスト（なければ null）。</summary>
        public QuestDef OfferFrom(string giver)
        {
            foreach (var quest in QuestBook.From(giver))
            {
                if (State.Quest(quest.Id) != null)
                    continue;
                if (quest.RequiresFlag == null || State.Has(quest.RequiresFlag))
                    return quest;
                return null;
            }
            return null;
        }

        /// <summary>その人から受けて、まだ報酬を受け取っていないクエスト（なければ null）。</summary>
        public QuestProgress ActiveFrom(string giver)
        {
            foreach (var quest in QuestBook.From(giver))
            {
                var progress = State.Quest(quest.Id);
                if (progress != null && !progress.claimed)
                    return progress;
            }
            return null;
        }

        public bool Accept(string id)
        {
            var quest = QuestBook.Find(id);
            if (quest == null || State.Quest(id) != null)
                return false;
            var progress = new QuestProgress { id = id };
            State.quests.Add(progress);
            if (IsComplete(quest, progress))
                progress.done = true;
            return true;
        }

        /// <summary>達成したクエストの報酬を受け取る。</summary>
        public bool Claim(string id, out int levelsGained)
        {
            levelsGained = 0;
            var progress = State.Quest(id);
            if (progress == null || !progress.done || progress.claimed)
                return false;
            var quest = QuestBook.Find(id);
            progress.claimed = true;
            State.Set(QuestBook.ClaimedFlag(id));
            levelsGained = new RewardGrant { Xp = quest.Xp, Comps = quest.Comps, Chips = quest.Chips, Item = quest.Item }.ApplyTo(State, chips).LevelsGained;
            var newly = new List<QuestDef>();
            CheckLevelQuests(newly);
            return true;
        }

        // ------------------------------------------------------------------
        // 景品
        // ------------------------------------------------------------------

        /// <summary>コンプポイントで景品と交換する。</summary>
        public bool Buy(string itemId)
        {
            var item = ItemCatalog.Find(itemId);
            if (item == null || item.CompPrice <= 0 || State.comps < item.CompPrice)
                return false;
            State.comps -= item.CompPrice;
            State.AddItem(itemId);
            return true;
        }

        /// <summary>引換券を使って遊戯用チップを受け取る。受け取った額を返す。</summary>
        public long Redeem(string itemId)
        {
            var item = ItemCatalog.Find(itemId);
            if (item == null || item.ChipValue <= 0 || !State.TryRemoveItem(itemId))
                return 0;
            chips.Credit(item.ChipValue);
            return item.ChipValue;
        }

        // ------------------------------------------------------------------
        // フロアと挑戦
        // ------------------------------------------------------------------

        public bool IsFloorOpen(FloorId id)
        {
            var floor = Floors.Of(id);
            return floor.UnlockedBy == null || State.Has(StoryFlags.Defeated(floor.UnlockedBy));
        }

        public bool HasDefeated(string master) => State.Has(StoryFlags.Defeated(master));

        public FloorStatus StatusOf(FloorId id)
        {
            var master = Floors.Of(id).Master;
            if (HasDefeated(master.Master))
                return FloorStatus.Cleared;
            if (!IsFloorOpen(id))
                return FloorStatus.Locked;
            return State.level >= master.RequiredLevel ? FloorStatus.Ready : FloorStatus.Open;
        }

        /// <summary>挑戦できるか。できない理由を返す（できるなら None）。</summary>
        public ChallengeBlock CheckChallenge(ChallengeSpec spec)
        {
            if (State.level < spec.RequiredLevel)
                return ChallengeBlock.Level;
            if (chips.Balance < spec.EntryFee)
                return ChallengeBlock.Funds;
            return ChallengeBlock.None;
        }

        /// <summary>参加費を払って挑戦を始める。</summary>
        public ChallengeRun StartChallenge(ChallengeSpec spec)
        {
            if (CheckChallenge(spec) != ChallengeBlock.None)
                throw new InvalidOperationException("この挑戦はまだ受けられません。");
            if (!chips.TryDebit(spec.EntryFee))
                throw new InvalidOperationException("参加費が足りません。");
            State.challenge = spec.Master;
            return new ChallengeRun(spec);
        }

        /// <summary>
        /// 参加費を払ったまま決着しなかった挑戦（アプリの終了や障害で中断）の参加費を返す。返した額（なければ 0）。
        /// 進行中の挑戦が手元に無いとき（起動時・障害でテーブルを離れたとき）に呼ぶ。
        /// </summary>
        public long RefundInterruptedChallenge()
        {
            var floor = State.challenge == null ? null : Floors.OfMaster(State.challenge);
            State.challenge = null;
            if (floor == null)
                return 0;
            chips.Credit(floor.Master.EntryFee);
            return floor.Master.EntryFee;
        }

        /// <summary>挑戦の結果を受け取る。はじめて勝ったときだけ賞金・記念品・次のフロアが手に入る（再挑戦の勝ちは経験値のみ）。</summary>
        public ChallengeReward FinishChallenge(ChallengeRun run)
        {
            if (!run.IsOver)
                throw new InvalidOperationException("挑戦はまだ終わっていません。");
            var spec = run.Spec;
            State.challenge = null;
            var reward = new ChallengeReward { Spec = spec, Won = run.State == ChallengeState.Won };
            if (!reward.Won)
                return reward;
            reward.FirstVictory = !State.Has(spec.DefeatedFlag);
            reward.Xp = reward.FirstVictory ? spec.XpReward : spec.XpReward / 5;
            reward.LevelsGained = Progression.AddXp(State, reward.Xp);
            if (reward.FirstVictory)
            {
                State.Set(spec.DefeatedFlag);
                reward.Prize = spec.Prize;
                chips.Credit(spec.Prize);
                if (spec.Trophy != null)
                {
                    State.AddItem(spec.Trophy);
                    reward.Trophy = spec.Trophy;
                }
            }
            CheckLevelQuests(new List<QuestDef>());
            return reward;
        }
    }
}
