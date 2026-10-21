using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;
using Vector.Game.World;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ホームのタブ。おすすめのバナー、コンシェルジュのレナ（触ると次の台詞）、ゲーム・ストーリー・ミッションへの近道。
    /// </summary>
    public sealed class HomeTab : IHomeTab
    {
        readonly RpgSession session;
        readonly HomeNavigator nav;
        SpeechBubble bubble;
        List<string> lines;
        int line;
        TextMeshProUGUI storyNote;
        TextMeshProUGUI missionNote;
        Badge missionBadge;

        HomeTab(Transform parent, RpgSession session, HomeNavigator nav)
        {
            this.session = session;
            this.nav = nav;
            Root = Ui.Stretch(parent, "HomeTab");
            Top = HomeLayout.Top(Root);
            Stage = HomeLayout.Bottom(Root);
        }

        public RectTransform Root { get; }

        /// <summary>上端に寄せる中身（バナー）。</summary>
        RectTransform Top { get; }

        /// <summary>下端に寄せる中身（レナと近道）。</summary>
        RectTransform Stage { get; }

        public static HomeTab Create(Transform parent, RpgSession session, HomeNavigator nav)
        {
            var tab = new HomeTab(parent, session, nav);
            tab.BuildConcierge();
            tab.BuildBanners();
            tab.BuildShortcuts();
            Ui.Sliced(tab.Stage, "PlayMoneyBack", Chrome.Shade, Palette.NightDeep.WithAlpha(0.8f), new Vector2(0, -736), new Vector2(760, 36), 18);
            Ui.Text(tab.Stage, "PlayMoney", "このゲームのチップは遊戯用です。現金・景品とは交換できません。", Fonts.Body, 18, Palette.Muted, new Vector2(0, -736), new Vector2(1000, 26));
            return tab;
        }

        public void Refresh()
        {
            lines = HomeLines.For(session, DateTime.Now);
            line = 0;
            bubble.Say(lines[0]);
            storyNote.text = NextFloorNote();
            var daily = session.Daily;
            int done = 0;
            for (int i = 0; i < DailyMissions.Count; i++)
            {
                if (daily.IsDone(i))
                    done++;
            }
            missionNote.text = $"{done} / {DailyMissions.Count} CLEAR";
            missionBadge.SetCount(daily.Claimable);
        }

        void BuildConcierge()
        {
            var glow = Ui.Image(Stage, "Glow", Art.Glow, Palette.Magenta.WithAlpha(0.35f), new Vector2(-150, -120), new Vector2(900, 1100));
            Pulse.Add(glow, 0.04f, 0.8f);
            var lena = PortraitView.Create(Stage, "Concierge", Cast.Of("lena").Look, new Vector2(-150, -250), 1000);
            Float.Add((RectTransform)lena.transform, 6f, 0.6f);
            var hit = Ui.Image(Stage, "ConciergeHit", null, new Color(1, 1, 1, 0.001f), new Vector2(-150, -150), new Vector2(560, 760), true);
            hit.gameObject.AddComponent<ClickTarget>().Clicked += NextLine;
            var name = Ui.Ribbon(Stage, "Name", "CONCIERGE  LENA", new Vector2(-150, -640), 440, Palette.Ruby, 28);
            name.characterSpacing = 4;
            bubble = SpeechBubble.Create(Stage, new Vector2(250, 300), new Vector2(520, 190), new Vector2(-330, -150));
        }

        void NextLine()
        {
            line = (line + 1) % lines.Count;
            bubble.Say(lines[line]);
        }

        void BuildBanners()
        {
            var size = new Vector2(1000, 330);
            var carousel = BannerCarousel.Create(Top, new Vector2(0, 560), size);
            carousel.Add(b => Banner(b, size, Palette.Violet, "STORY MODE", "最上階のオーナーへ挑む、カジノ RPG。\nフロアの主に勝って上を目指そう。", "ENTER", nav.EnterStory,
                art => PortraitView.Create(art, "Zero", Cast.Of("zero").Look, new Vector2(0, -40), 400)));
            carousel.Add(b => Banner(b, size, Theme.Hex("6B4A12"), "6 ORIGINAL GAMES", "ここでしか遊べない6つのゲーム。\n配当と確率はすべて公開しています。", "PLAY",
                () => nav.SelectTab(HomeTabId.Games), GamesArt));
            carousel.Add(b => Banner(b, size, Theme.Hex("0E5E4A"), "DAILY MISSIONS", "毎日入れ替わる3つのミッションと、\n7日で一巡するログインボーナス。", "CHECK",
                () => nav.SelectTab(HomeTabId.Missions), art =>
                {
                    Ui.Icon(art, "Calendar", IconKind.Calendar, Palette.GoldLight, new Vector2(-40, 20), 150);
                    Ui.Image(art, "Coin", Chrome.Coin, Color.white, new Vector2(60, -60), Vector2.one * 110);
                    Ui.Image(art, "Spark", Chrome.Sparkle, Palette.GoldLight, new Vector2(80, 70), Vector2.one * 80);
                }));
        }

        static void GamesArt(RectTransform art)
        {
            var games = GameCatalog.All;
            for (int i = 0; i < games.Count; i++)
                GameEmblem.Create(art, new Vector2((i % 3 - 1) * 110, (i < 3 ? 1 : -1) * 66), 112, games[i].Id);
        }

        /// <summary>バナー1枚（色の地・右に絵・左に見出しと説明とボタン）。</summary>
        static void Banner(RectTransform b, Vector2 size, Color color, string title, string pitch, string button, Action onClick, Action<RectTransform> art)
        {
            Ui.Plate(b, "Plate", Vector2.zero, size, color, 30);
            var rays = Ui.Image(b, "Rays", Chrome.Rays(16), Palette.GoldLight.WithAlpha(0.14f), new Vector2(300, 0), Vector2.one * 520);
            Spin.Add(rays, 6f);
            Ui.Image(b, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.3f), new Vector2(300, 0), new Vector2(420, 360));
            var artRoot = Ui.Clip(b, "Art", new Vector2(300, 0), new Vector2(380, size.y - 14));
            art(artRoot);
            float left = -size.x / 2 + 44;
            var head = Ui.Headline(b, "Title", title, Fonts.Display, 58, new Vector2(left + 280, 96), new Vector2(560, 70), TextAlignmentOptions.Left);
            head.characterSpacing = 3;
            Ui.Paragraph(b, "Pitch", pitch, 23, Palette.Ivory, new Vector2(left + 280, 6), new Vector2(560, 84));
            Ui.Button(b, "Go", button, new Vector2(left + 130, -100), new Vector2(260, 76), Palette.Gold, Palette.Ink, onClick, 32, 24);
        }

        void BuildShortcuts()
        {
            const float x = 362;
            Shortcut("Play", new Vector2(x, 40), Palette.GoldDark, IconKind.Play, "PLAY", () => nav.SelectTab(HomeTabId.Games)).text = "6 GAMES";
            storyNote = Shortcut("Story", new Vector2(x, -186), Palette.VioletBright, IconKind.Story, "STORY", () => nav.SelectTab(HomeTabId.Story));
            missionNote = Shortcut("Missions", new Vector2(x, -412), Theme.Hex("12805F"), IconKind.Missions, "MISSIONS", () => nav.SelectTab(HomeTabId.Missions));
            missionBadge = Badge.Create(Stage, new Vector2(x + 140, -412 + 96), 44);
        }

        /// <summary>右の列の近道のタイル。返すのはタイルの下の小さな説明。</summary>
        TextMeshProUGUI Shortcut(string name, Vector2 position, Color color, IconKind icon, string title, Action onClick)
        {
            var tile = TileButton.Create(Stage, name, position, new Vector2(300, 204), color, onClick);
            Ui.Image(tile.Content, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.25f), new Vector2(0, 30), new Vector2(240, 160));
            Ui.Icon(tile.Content, "Icon", icon, Palette.GoldLight, new Vector2(0, 38), 84);
            var label = Ui.Text(tile.Content, "Title", title, Fonts.Display, 40, Palette.Ivory, new Vector2(0, -32), new Vector2(280, 48));
            Ui.Outline(label, Palette.Ink, 0.2f);
            label.characterSpacing = 4;
            return Ui.Text(tile.Content, "Note", "", Fonts.Label, 20, Palette.GoldLight, new Vector2(0, -72), new Vector2(280, 26));
        }

        string NextFloorNote()
        {
            foreach (var floor in FloorOrder.BottomUp)
            {
                if (session.StatusOf(floor) != FloorStatus.Cleared)
                    return "NEXT  " + StoryNames.FloorLevel(floor);
            }
            return "ALL CLEAR";
        }
    }
}
