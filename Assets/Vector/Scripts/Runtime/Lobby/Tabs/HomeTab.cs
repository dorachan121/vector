using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;
using Vector.Game.World;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ホーム。カジノのロビーに立つコンシェルジュのレナ（触ると次の台詞）、左にお知らせ・ログボ・ミッション・メニュー、
    /// 右上にイベントのバナーと今日のミッションの進み具合。
    /// </summary>
    public sealed class HomeTab : IHomeTab
    {
        /// <summary>左の丸いメニューの位置（上の土台の中心から）。</summary>
        const float SideX = -456;

        readonly RpgSession session;
        readonly HomeNavigator nav;
        readonly HomeMenus menus;
        SpeechBubble bubble;
        RectTransform concierge;
        List<string> lines;
        int line;
        Badge missionBadge;
        Meter dailyMeter;
        TextMeshProUGUI dailyText;

        HomeTab(Transform parent, RpgSession session, HomeNavigator nav, HomeMenus menus)
        {
            this.session = session;
            this.nav = nav;
            this.menus = menus;
            Root = Ui.Stretch(parent, "HomeTab");
            Stage = HomeLayout.Bottom(Root);
            Top = HomeLayout.Top(Root);
        }

        public RectTransform Root { get; }

        /// <summary>上端に寄せる中身（メニューとバナー）。</summary>
        RectTransform Top { get; }

        /// <summary>下端に寄せる中身（レナ）。</summary>
        RectTransform Stage { get; }

        /// <param name="menus">お知らせ・ログボ・メニューを開く処理。</param>
        public static HomeTab Create(Transform parent, RpgSession session, HomeNavigator nav, HomeMenus menus)
        {
            var tab = new HomeTab(parent, session, nav, menus);
            tab.BuildConcierge();
            tab.BuildSideMenu();
            tab.BuildBanners();
            tab.BuildDaily();
            return tab;
        }

        public void Refresh()
        {
            lines = HomeLines.For(session, DateTime.Now);
            line = 0;
            bubble.Say(lines[0]);
            var daily = session.Daily;
            int done = 0;
            for (int i = 0; i < DailyMissions.Count; i++)
            {
                if (daily.IsDone(i))
                    done++;
            }
            dailyMeter.SetFraction(done / (float)DailyMissions.Count, false);
            dailyText.text = $"{done} / {DailyMissions.Count}";
            missionBadge.SetCount(daily.Claimable);
        }

        void BuildConcierge()
        {
            var glow = Ui.Image(Stage, "Aura", Art.Glow, Palette.Magenta.WithAlpha(0.3f), new Vector2(-40, 0), new Vector2(1000, 1300));
            Pulse.Add(glow, 0.04f, 0.7f);
            var lena = PortraitView.Create(Stage, "Concierge", Cast.Of("lena").Look, new Vector2(-40, -350), 1060);
            concierge = (RectTransform)lena.transform;
            Float.Add(concierge, 7f, 0.55f);
            var hit = Ui.Image(Stage, "ConciergeHit", null, new Color(1, 1, 1, 0.001f), new Vector2(-40, -300), new Vector2(680, 860), true);
            hit.gameObject.AddComponent<ClickTarget>().Clicked += Poke;
            var name = Ui.Rect(Stage, "Name", new Vector2(-300, -580), new Vector2(330, 120));
            Ui.Sliced(name, "Plate", Chrome.Gloss, Palette.Ruby, new Vector2(0, 14), new Vector2(300, 66), 33);
            Ui.Sliced(name, "Rim", Chrome.Rim, Palette.GoldLight, new Vector2(0, 14), new Vector2(300, 66), 33);
            Ui.Outline(Ui.Text(name, "Name", "レナ", Fonts.Body, 36, Palette.Ivory, new Vector2(0, 15), new Vector2(280, 46)), Palette.Ink, 0.24f);
            Ui.Outline(Ui.Text(name, "Role", "CONCIERGE", Fonts.Label, 22, Palette.GoldLight, new Vector2(0, -36), new Vector2(280, 28)), Palette.Ink, 0.3f);
            bubble = SpeechBubble.Create(Stage, new Vector2(280, 150), new Vector2(460, 190), new Vector2(-220, -150));
        }

        /// <summary>レナに触れる: 少し弾んで、次の台詞を言う。</summary>
        void Poke()
        {
            Sfx.Play(SfxKind.Button, 0.6f);
            concierge.localScale = new Vector3(1.03f, 0.97f, 1);
            Tween.Scale(concierge, Vector3.one, 0.4f, Ease.OutBack);
            line = (line + 1) % lines.Count;
            bubble.Say(lines[line]);
        }

        void BuildSideMenu()
        {
            SideButton.Create(Top, "Notice", new Vector2(SideX, 690), IconKind.Help, Palette.Sapphire, "お知らせ", menus.Notices);
            SideButton.Create(Top, "Login", new Vector2(SideX, 530), IconKind.Calendar, Palette.Ruby, "ログボ", menus.LoginCalendar);
            missionBadge = SideButton.Create(Top, "Missions", new Vector2(SideX, 370), IconKind.Missions, Theme.Hex("12805F"), "ミッション", () => nav.SelectTab(HomeTabId.Missions));
            SideButton.Create(Top, "Menu", new Vector2(SideX, 210), IconKind.Settings, Palette.VioletBright, "メニュー", menus.Menu);
        }

        void BuildBanners()
        {
            var size = new Vector2(580, 250);
            var carousel = BannerCarousel.Create(Top, new Vector2(196, 640), size);
            carousel.Add(b => Banner(b, size, Palette.Violet, "STORY", "最上階のオーナーに挑め", () => nav.SelectTab(HomeTabId.Story),
                art => PortraitView.Create(art, "Zero", Cast.Of("zero").Look, new Vector2(0, -50), 330)));
            carousel.Add(b => Banner(b, size, Theme.Hex("6B4A12"), "10 GAMES", "ここでしか遊べない10のゲーム", () => nav.SelectTab(HomeTabId.Play),
                art => GameEmblem.Create(art, new Vector2(0, -6), 200, Core.GameId.Crown)));
            carousel.Add(b => Banner(b, size, Theme.Hex("0E5E4A"), "DAILY", "毎日のミッションとログボ", () => nav.SelectTab(HomeTabId.Missions), art =>
            {
                Ui.Icon(art, "Calendar", IconKind.Calendar, Palette.GoldLight, new Vector2(-10, 10), 130);
                Ui.Image(art, "Coin", Chrome.Coin, Color.white, new Vector2(60, -60), Vector2.one * 90);
            }));
        }

        /// <summary>イベントのバナー1枚（色の地・右に絵・左に見出しと一言）。押すとその画面へ。</summary>
        static void Banner(RectTransform b, Vector2 size, Color color, string title, string pitch, Action onClick, Action<RectTransform> art)
        {
            var plate = Ui.Plate(b, "Plate", Vector2.zero, size, color, 26, true, true);
            plate.gameObject.AddComponent<ClickTarget>().Clicked += () =>
            {
                Sfx.Play(SfxKind.Button);
                onClick();
            };
            var rays = Ui.Image(b, "Rays", Chrome.Rays(16), Palette.GoldLight.WithAlpha(0.16f), new Vector2(160, 0), Vector2.one * 420);
            Spin.Add(rays, 6f);
            var artRoot = Ui.Clip(b, "Art", new Vector2(160, 0), new Vector2(240, size.y - 12));
            art(artRoot);
            float left = -size.x / 2 + 32;
            var head = Ui.Headline(b, "Title", title, Fonts.Display, 62, new Vector2(left + 150, 40), new Vector2(300, 74), TextAlignmentOptions.Left);
            head.characterSpacing = 3;
            Ui.Paragraph(b, "Pitch", pitch, 24, Palette.Ivory, new Vector2(left + 150, -40), new Vector2(300, 70));
            Ui.Sliced(b, "Event", Chrome.Gloss, Palette.Ruby, new Vector2(-size.x / 2 + 74, size.y / 2 - 6), new Vector2(120, 38), 19);
            Ui.Text(b, "EventText", "EVENT", Fonts.Label, 20, Palette.Ivory, new Vector2(-size.x / 2 + 74, size.y / 2 - 5), new Vector2(110, 26));
        }

        /// <summary>バナーの下の、今日のミッションの進み具合（押すとミッションへ）。</summary>
        void BuildDaily()
        {
            var at = new Vector2(196, 446);
            var size = new Vector2(580, 96);
            var plate = Ui.Plate(Top, "Daily", at, size, Palette.NightDeep.WithAlpha(0.88f), 48, false, true);
            plate.gameObject.AddComponent<ClickTarget>().Clicked += () =>
            {
                Sfx.Play(SfxKind.Button);
                nav.SelectTab(HomeTabId.Missions);
            };
            var rt = (RectTransform)plate.transform.parent;
            Ui.Sliced(rt, "Rim", Chrome.Rim, Palette.Emerald.WithAlpha(0.8f), Vector2.zero, size, 48);
            Ui.Icon(rt, "Icon", IconKind.Missions, Palette.Emerald, new Vector2(-size.x / 2 + 56, 0), 54);
            Ui.Text(rt, "Label", "今日のミッション", Fonts.Body, 24, Palette.Ivory, new Vector2(-40, 20), new Vector2(340, 32), TextAlignmentOptions.Left);
            dailyMeter = Meter.Create(rt, "Meter", new Vector2(-40, -18), new Vector2(340, 22), Palette.Emerald);
            dailyText = Ui.Text(rt, "Count", "", Fonts.Display, 36, Palette.GoldLight, new Vector2(size.x / 2 - 80, 0), new Vector2(130, 44));
        }
    }
}
