using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ミッション。上の切り替えで「デイリー」（今日の3つのミッション・まとめて受け取る・すべて達成のボーナス）と
    /// 「ログインボーナス」（7日で1巡する暦）を見る。表示するたびに中身を作り直す（日付が変わるとミッションも入れ替わる）。
    /// </summary>
    public sealed class MissionsTab : IHomeTab
    {
        static readonly Vector2 CardSize = new Vector2(1000, 180);

        readonly StoryProgress story;
        readonly HomeNavigator nav;
        readonly Transform popups;
        readonly SegmentedTabs pages;
        RectTransform content;

        MissionsTab(Transform parent, StoryProgress story, HomeNavigator nav, Transform popups)
        {
            this.story = story;
            this.nav = nav;
            this.popups = popups;
            Root = Ui.Stretch(parent, "MissionsTab");
            Body = HomeLayout.Top(Root);
            PageTitle.Create(Body, 728, "MISSION", "ミッション");
            pages = SegmentedTabs.Create(Body, new Vector2(0, 606), 760, "デイリー", "ログインボーナス");
            pages.Selected += _ => Refresh();
        }

        public RectTransform Root { get; }

        /// <summary>中身の土台（上端に寄せる）。</summary>
        RectTransform Body { get; }

        DailyProgress Daily => story.Session.Daily;

        /// <param name="popups">報酬の演出を開く画面。</param>
        public static MissionsTab Create(Transform parent, StoryProgress story, HomeNavigator nav, Transform popups)
        {
            var tab = new MissionsTab(parent, story, nav, popups);
            tab.pages.Select(0);
            return tab;
        }

        public void Refresh()
        {
            if (content != null)
                Object.Destroy(content.gameObject);
            content = Ui.Stretch(Body, "Content");
            pages.SetBadge(0, Daily.Claimable);
            if (pages.Current == 1)
                BuildLogin();
            else
                BuildDaily();
        }

        // ------------------------------------------------------------------
        // デイリー
        // ------------------------------------------------------------------

        void BuildDaily()
        {
            BuildSummary(468);
            var missions = Daily.Missions;
            for (int i = 0; i < missions.Count; i++)
                BuildMission(i, missions[i], 268 - i * 200);
            BuildBonus(-372);
            Ui.Paragraph(content, "Note", "ミッションは毎日 0:00 に入れ替わります。報酬は遊戯用チップとポイントで、ゲームの確率や配当は変わりません。",
                21, Palette.Muted, new Vector2(0, -580), new Vector2(980, 64), TextAlignmentOptions.Top);
        }

        void BuildSummary(float y)
        {
            int done = 0;
            for (int i = 0; i < DailyMissions.Count; i++)
            {
                if (Daily.IsDone(i))
                    done++;
            }
            var size = new Vector2(1000, 150);
            var rt = Ui.Card(content, "Summary", new Vector2(0, y), size, Theme.Hex("0E5E4A"), 26);
            Ui.Text(rt, "Label", "今日の達成", Fonts.Body, 26, Palette.Ivory, new Vector2(-300, 34), new Vector2(340, 34), TextAlignmentOptions.Left);
            var count = Ui.Headline(rt, "Count", $"{done} / {DailyMissions.Count}", Fonts.Display, 60, new Vector2(-300, -20), new Vector2(340, 70), TextAlignmentOptions.Left);
            count.characterSpacing = 2;
            Meter.Create(rt, "Meter", new Vector2(-40, -26), new Vector2(240, 24), Palette.GoldLight).SetFraction(done / (float)DailyMissions.Count, false);
            int claimable = Daily.Claimable;
            var all = Ui.Button(rt, "ClaimAll", "まとめて受け取る", new Vector2(size.x / 2 - 186, 0), new Vector2(320, 96), Palette.Gold, Palette.Ink, ClaimAll, 30, 30);
            all.Label.font = Fonts.Body;
            all.Interactable = claimable > 0;
            if (claimable > 0)
                Pulse.Add(all, 0.04f, 3f);
        }

        void BuildMission(int index, DailyMissionDef mission, float y)
        {
            var rt = Ui.Card(content, "Mission" + index, new Vector2(0, y), CardSize, Palette.PanelTop, 24, false);
            float left = -CardSize.x / 2;
            bool done = Daily.IsDone(index), claimed = Daily.IsClaimed(index);
            Ui.Sliced(rt, "IconBox", Chrome.Gloss, done ? Palette.Emerald : Palette.VioletBright, new Vector2(left + 84, 0), new Vector2(124, 124), 28);
            Ui.Icon(rt, "Icon", IconOf(mission.Kind), Palette.Ivory, new Vector2(left + 84, 0), 70);

            // 左のアイコン（右端 -354）に文字と目盛りが重ならない位置から始める
            const float infoX = -86, infoWidth = 520;
            Ui.Text(rt, "Text", MissionText.Describe(mission), Fonts.Body, 28, Palette.Ivory, new Vector2(infoX, 48), new Vector2(infoWidth, 38), TextAlignmentOptions.Left);
            long progress = Daily.ProgressOf(index);
            Meter.Create(rt, "Meter", new Vector2(infoX - 74, 2), new Vector2(infoWidth - 148, 26), done ? Palette.Emerald : Palette.Gold)
                .SetFraction(progress / (float)mission.Target, false);
            Ui.Text(rt, "Progress", MissionText.Progress(mission, progress), Fonts.Label, 22, Palette.Ivory, new Vector2(infoX + infoWidth / 2 - 70, 2), new Vector2(160, 30), TextAlignmentOptions.Right);
            Ui.Text(rt, "Reward", "報酬  " + MissionText.Reward(mission.Xp, mission.Comps, mission.Chips), Fonts.Body, 22, Palette.GoldLight,
                new Vector2(infoX, -46), new Vector2(infoWidth, 30), TextAlignmentOptions.Left);

            var at = new Vector2(CardSize.x / 2 - 116, 0);
            if (claimed)
                Stamp(rt, at, "受取済");
            else if (done)
            {
                var claim = Ui.Button(rt, "Claim", "受け取る", at, new Vector2(196, 92), Palette.Gold, Palette.Ink, () => ClaimMission(index), 30, 26);
                claim.Label.font = Fonts.Body;
                Pulse.Add(claim, 0.05f, 3f);
            }
            else
            {
                var go = Ui.Button(rt, "Go", "挑戦する", at, new Vector2(196, 92), new Color(1, 1, 1, 0.1f), Palette.Ivory, () => Go(mission), 30, 26);
                go.Label.font = Fonts.Body;
            }
        }

        void BuildBonus(float y)
        {
            var size = new Vector2(1000, 220);
            var rt = Ui.Card(content, "Bonus", new Vector2(0, y), size, Palette.GoldDark, 28);
            var rays = Ui.Image(rt, "Rays", Chrome.Rays(14), Palette.GoldLight.WithAlpha(0.22f), new Vector2(-380, 0), Vector2.one * 320);
            Spin.Add(rays, 10f);
            var gift = Ui.Icon(rt, "Gift", IconKind.Gift, Palette.GoldLight, new Vector2(-380, 0), 130);
            Float.Add(gift.rectTransform, 6f, 1.5f);
            Ui.Headline(rt, "Title", "ALL CLEAR BONUS", Fonts.Display, 52, new Vector2(-20, 44), new Vector2(480, 62), TextAlignmentOptions.Left).characterSpacing = 2;
            var bonus = DailyMissions.AllClearBonus;
            Ui.Text(rt, "Reward", MissionText.Reward(bonus.Xp, bonus.Comps, bonus.Chips), Fonts.Label, 28, Palette.Ivory, new Vector2(-20, -18), new Vector2(480, 34), TextAlignmentOptions.Left);
            Ui.Text(rt, "Hint", "3つのミッションの報酬を受け取ると", Fonts.Body, 21, Palette.GoldLight, new Vector2(-20, -62), new Vector2(480, 28), TextAlignmentOptions.Left);
            var at = new Vector2(size.x / 2 - 116, 0);
            if (Daily.BonusClaimed)
            {
                Stamp(rt, at, "受取済");
                return;
            }
            var claim = Ui.Button(rt, "Claim", "受け取る", at, new Vector2(196, 92), Palette.Gold, Palette.Ink, ClaimBonus, 30, 26);
            claim.Label.font = Fonts.Body;
            claim.Interactable = Daily.AllClaimed;
            if (Daily.AllClaimed)
                Pulse.Add(claim, 0.05f, 3f);
        }

        /// <summary>受け取ったあとの判（赤い輪と文字、少し傾ける）。</summary>
        static void Stamp(RectTransform parent, Vector2 at, string text)
        {
            var stamp = Ui.Rect(parent, "Stamp", at, new Vector2(170, 92));
            Ui.Sliced(stamp, "Ring", Chrome.Rim, Palette.Ruby, Vector2.zero, new Vector2(170, 76), 16);
            Ui.Text(stamp, "Text", text, Fonts.Body, 32, Palette.Ruby, new Vector2(0, 1), new Vector2(160, 44));
            stamp.localEulerAngles = new Vector3(0, 0, 8);
        }

        static IconKind IconOf(DailyMissionKind kind)
        {
            switch (kind)
            {
                case DailyMissionKind.PlayRounds: return IconKind.Play;
                case DailyMissionKind.WinRounds: return IconKind.Trophy;
                case DailyMissionKind.Wager: return IconKind.Coin;
                case DailyMissionKind.PlayGame: return IconKind.Games;
                default: return IconKind.Star;
            }
        }

        // ------------------------------------------------------------------
        // ログインボーナス
        // ------------------------------------------------------------------

        void BuildLogin()
        {
            int claimed = Daily.ClaimedThisCycle;
            LoginCalendar.Create(content, new Vector2(0, 286), claimed, false);
            int next = Daily.NextLoginDay;
            var reward = RewardDisplay.From(LoginBonus.RewardFor(next))[0];
            var rt = Ui.Card(content, "Next", new Vector2(0, -80), new Vector2(1000, 150), Palette.PanelTop, 26, false);
            Ui.Image(rt, "Icon", reward.Icon, reward.Tint, new Vector2(-400, 0), Vector2.one * 100).preserveAspect = true;
            Ui.Text(rt, "Label", Daily.LoginAvailable ? "今日の報酬" : "明日の報酬", Fonts.Body, 26, Palette.Muted, new Vector2(-60, 28), new Vector2(560, 34), TextAlignmentOptions.Left);
            Ui.Text(rt, "Value", $"{next + 1}日目  {reward.Amount}", Fonts.Body, 36, Palette.GoldLight, new Vector2(-60, -22), new Vector2(560, 46), TextAlignmentOptions.Left);
            Ui.Paragraph(content, "Note", "ログインボーナスは1日1回、ホームに来たときに受け取れます。休んでもリセットされず、途中からそのまま続きます。7日目は記念の「幸運のコイン」つき。",
                23, Palette.Muted, new Vector2(0, -280), new Vector2(980, 120), TextAlignmentOptions.Top);
        }

        // ------------------------------------------------------------------
        // 受け取り
        // ------------------------------------------------------------------

        void Go(DailyMissionDef mission)
        {
            if (mission.Kind == DailyMissionKind.PlayGame)
                nav.Play(mission.Game);
            else
                nav.SelectTab(HomeTabId.Play);
        }

        void ClaimMission(int index)
        {
            var mission = Daily.Missions[index];
            var grant = Daily.Claim(index);
            if (grant != null)
                Celebrate("MISSION CLEAR", MissionText.Describe(mission), grant);
        }

        void ClaimBonus()
        {
            var grant = Daily.ClaimBonus();
            if (grant != null)
                Celebrate("ALL CLEAR", "今日のミッションをすべて達成しました！", grant);
        }

        /// <summary>達成したミッションの報酬を全部受け取り、3つそろったら達成ボーナスも受け取る。</summary>
        void ClaimAll()
        {
            RewardGrant total = null;
            for (int i = 0; i < DailyMissions.Count; i++)
            {
                var grant = Daily.Claim(i);
                if (grant != null)
                    total = total == null ? grant : total.Plus(grant);
            }
            var bonus = Daily.ClaimBonus();
            if (bonus != null)
                total = total == null ? bonus : total.Plus(bonus);
            if (total != null)
                Celebrate("MISSION CLEAR", bonus != null ? "今日のミッションをすべて達成しました！" : "達成したミッションの報酬を受け取りました。", total);
        }

        void Celebrate(string title, string message, RewardGrant grant)
        {
            story.Save();
            nav.Changed();
            RewardPopup.Show(popups, title, RewardDisplay.Message(message, grant, story.State.level), RewardDisplay.From(grant));
        }
    }
}
