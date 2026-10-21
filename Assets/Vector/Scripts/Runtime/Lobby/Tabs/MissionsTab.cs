using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ミッションのタブ。ログインボーナスの暦（7日で1巡）、今日の3つのミッション（進み具合と報酬の受け取り）、3つ受け取ったあとの達成ボーナス。
    /// 表示するたびに中身を作り直す（日付が変わるとミッションも入れ替わる）。
    /// </summary>
    public sealed class MissionsTab : IHomeTab
    {
        static readonly Vector2 CardSize = new Vector2(1000, 178);
        const float CellSpacing = 140;

        readonly StoryProgress story;
        readonly HomeNavigator nav;
        readonly Transform popups;
        RectTransform content;

        MissionsTab(Transform parent, StoryProgress story, HomeNavigator nav, Transform popups)
        {
            this.story = story;
            this.nav = nav;
            this.popups = popups;
            Root = Ui.Stretch(parent, "MissionsTab");
            Body = HomeLayout.Top(Root);
        }

        public RectTransform Root { get; }

        /// <summary>中身の土台（上端に寄せる）。</summary>
        RectTransform Body { get; }

        DailyProgress Daily => story.Session.Daily;

        /// <param name="popups">報酬の演出を開く画面。</param>
        public static MissionsTab Create(Transform parent, StoryProgress story, HomeNavigator nav, Transform popups)
        {
            var tab = new MissionsTab(parent, story, nav, popups);
            Ui.Ribbon(tab.Body, "LoginTitle", "LOGIN BONUS", new Vector2(0, 706), 560, Palette.Ruby, 36);
            Ui.Text(tab.Body, "LoginNote", "ログインボーナスは、休んでも途中からそのまま続きます。", Fonts.Body, 20, Palette.Muted, new Vector2(0, 396), new Vector2(1000, 28));
            Ui.Ribbon(tab.Body, "MissionTitle", "DAILY MISSIONS", new Vector2(0, 312), 560, Palette.Violet, 36);
            Ui.Paragraph(tab.Body, "Note", "ミッションは毎日 0:00 に入れ替わります。報酬は遊戯用チップとポイントで、ゲームの確率や配当は変わりません。",
                19, Palette.Muted, new Vector2(0, -668), new Vector2(980, 60), TextAlignmentOptions.Top);
            return tab;
        }

        public void Refresh()
        {
            if (content != null)
                Object.Destroy(content.gameObject);
            content = Ui.Stretch(Body, "Content");
            BuildCalendar();
            var missions = Daily.Missions;
            for (int i = 0; i < missions.Count; i++)
                BuildMission(i, missions[i], 150 - i * 192);
            BuildBonus(-470);
        }

        void BuildCalendar()
        {
            int claimed = Daily.ClaimedThisCycle;
            for (int day = 0; day < LoginBonus.CycleLength; day++)
            {
                var position = new Vector2((day - (LoginBonus.CycleLength - 1) / 2f) * CellSpacing, 528);
                bool last = day == LoginBonus.CycleLength - 1;
                bool done = day < claimed;
                bool today = day == claimed - 1 && !Daily.LoginAvailable;
                if (today)
                {
                    var glow = Ui.Image(content, "TodayGlow", Art.Glow, Palette.GoldLight.WithAlpha(0.6f), position, new Vector2(220, 280));
                    Pulse.Add(glow, 0.06f, 2f);
                }
                var rt = Ui.Card(content, "Day" + (day + 1), position, new Vector2(132, 200), last ? Palette.GoldDark : Palette.Violet, 20, today || last);
                Ui.Text(rt, "Label", $"DAY {day + 1}", Fonts.Label, 22, last ? Palette.GoldLight : Palette.Ivory, new Vector2(0, 72), new Vector2(120, 28));
                var reward = LoginBonus.RewardFor(day);
                var shown = RewardDisplay.From(reward)[0];
                var icon = Ui.Image(rt, "Icon", shown.Icon, shown.Tint, new Vector2(0, 8), Vector2.one * (last ? 84 : 72));
                icon.preserveAspect = true;
                if (last)
                    Ui.Icon(rt, "Gift", IconKind.Gift, Palette.Magenta, new Vector2(36, 38), 40);
                var amount = Ui.Text(rt, "Amount", reward.Chips > 0 ? Labels.Money(reward.Chips) : $"{reward.Comps} pt", Fonts.Display, 28, Palette.Ivory, new Vector2(0, -62), new Vector2(124, 36));
                Ui.Outline(amount, Palette.Ink, 0.2f);
                if (done)
                {
                    Ui.Sliced(rt, "Dim", Chrome.Shade, Palette.NightDeep.WithAlpha(0.55f), Vector2.zero, new Vector2(132, 200), 20);
                    Ui.Image(rt, "Stamp", Art.Circle, Palette.Emerald, new Vector2(0, 4), Vector2.one * 70);
                    Ui.Icon(rt, "Check", IconKind.Check, Palette.Ivory, new Vector2(0, 4), 44);
                }
            }
        }

        void BuildMission(int index, DailyMissionDef mission, float y)
        {
            var rt = Ui.Card(content, "Mission" + index, new Vector2(0, y), CardSize, Palette.PanelTop, 24, false);
            float left = -CardSize.x / 2;
            bool done = Daily.IsDone(index), claimed = Daily.IsClaimed(index);
            Ui.Sliced(rt, "IconBox", Chrome.Gloss, done ? Palette.Emerald : Palette.VioletBright, new Vector2(left + 80, 0), new Vector2(116, 116), 26);
            Ui.Icon(rt, "Icon", IconOf(mission.Kind), Palette.Ivory, new Vector2(left + 80, 0), 66);

            const float infoX = -110, infoWidth = 540;
            Ui.Text(rt, "Text", MissionText.Describe(mission), Fonts.Body, 27, Palette.Ivory, new Vector2(infoX, 46), new Vector2(infoWidth, 36), TextAlignmentOptions.Left);
            long progress = Daily.ProgressOf(index);
            Meter.Create(rt, "Meter", new Vector2(infoX - 70, 0), new Vector2(infoWidth - 140, 26), done ? Palette.Emerald : Palette.Gold)
                .SetFraction(progress / (float)mission.Target, false);
            Ui.Text(rt, "Progress", MissionText.Progress(mission, progress), Fonts.Label, 22, Palette.Ivory, new Vector2(infoX + infoWidth / 2 - 66, 0), new Vector2(150, 30), TextAlignmentOptions.Right);
            Ui.Text(rt, "Reward", "REWARD  " + MissionText.Reward(mission.Xp, mission.Comps, mission.Chips), Fonts.Label, 21, Palette.GoldLight,
                new Vector2(infoX, -46), new Vector2(infoWidth, 28), TextAlignmentOptions.Left);

            var at = new Vector2(CardSize.x / 2 - 120, 0);
            if (claimed)
            {
                Ui.Icon(rt, "Done", IconKind.Check, Palette.Emerald, at + new Vector2(0, 16), 54);
                Ui.Text(rt, "DoneText", "CLEAR", Fonts.Label, 24, Palette.Emerald, at + new Vector2(0, -30), new Vector2(180, 30));
            }
            else if (done)
            {
                var claim = Ui.Button(rt, "Claim", "CLAIM", at, new Vector2(196, 86), Palette.Gold, Palette.Ink, () => ClaimMission(index), 34, 24);
                Pulse.Add(claim, 0.05f, 3f);
            }
            else
            {
                Ui.Button(rt, "Go", "GO", at, new Vector2(196, 86), new Color(1, 1, 1, 0.1f), Palette.Ivory, () => Go(mission), 34, 24);
            }
        }

        void BuildBonus(float y)
        {
            var size = new Vector2(1000, 204);
            var rt = Ui.Card(content, "Bonus", new Vector2(0, y), size, Palette.GoldDark, 26);
            var rays = Ui.Image(rt, "Rays", Chrome.Rays(14), Palette.GoldLight.WithAlpha(0.2f), new Vector2(-380, 0), Vector2.one * 300);
            Spin.Add(rays, 10f);
            var gift = Ui.Icon(rt, "Gift", IconKind.Gift, Palette.GoldLight, new Vector2(-380, 0), 120);
            Float.Add(gift.rectTransform, 6f, 1.5f);
            var head = Ui.Headline(rt, "Title", "ALL CLEAR BONUS", Fonts.Display, 50, new Vector2(-30, 38), new Vector2(480, 60), TextAlignmentOptions.Left);
            head.characterSpacing = 2;
            var bonus = DailyMissions.AllClearBonus;
            Ui.Text(rt, "Reward", MissionText.Reward(bonus.Xp, bonus.Comps, bonus.Chips), Fonts.Label, 26, Palette.Ivory, new Vector2(-30, -26), new Vector2(480, 32), TextAlignmentOptions.Left);
            Ui.Text(rt, "Hint", "3つのミッションの報酬を受け取ると", Fonts.Body, 19, Palette.GoldLight, new Vector2(-30, -66), new Vector2(480, 26), TextAlignmentOptions.Left);
            var at = new Vector2(size.x / 2 - 120, 0);
            if (Daily.BonusClaimed)
                Ui.Text(rt, "Received", "RECEIVED", Fonts.Label, 28, Palette.Emerald, at, new Vector2(200, 36));
            else
            {
                var claim = Ui.Button(rt, "Claim", "CLAIM", at, new Vector2(196, 86), Palette.Gold, Palette.Ink, ClaimBonus, 34, 24);
                claim.Interactable = Daily.AllClaimed;
                if (Daily.AllClaimed)
                    Pulse.Add(claim, 0.05f, 3f);
            }
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

        void Go(DailyMissionDef mission)
        {
            if (mission.Kind == DailyMissionKind.PlayGame)
                nav.Play(mission.Game);
            else
                nav.SelectTab(HomeTabId.Games);
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

        void Celebrate(string title, string message, RewardGrant grant)
        {
            story.Save();
            nav.Changed();
            RewardPopup.Show(popups, title, RewardDisplay.Message(message, grant, story.State.level), RewardDisplay.From(grant));
        }
    }
}
