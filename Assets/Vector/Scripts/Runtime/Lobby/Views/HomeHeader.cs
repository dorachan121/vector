using System;
using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;
using Vector.Game.World;

namespace Vector.Game.Lobby
{
    /// <summary>ホーム画面の上部（アバター・レベル・会員ランク・経験値・ヘルプと設定・所持金・コンプ）。画面の上端に付く。</summary>
    public sealed class HomeHeader
    {
        public const float Height = 236f;

        TextMeshProUGUI level;
        TextMeshProUGUI rank;
        TextMeshProUGUI xp;
        Meter xpMeter;
        CurrencyPill chips;
        CurrencyPill comps;

        public static HomeHeader Create(Transform parent, Action openShop, Action openHelp, Action openSettings)
        {
            var header = new HomeHeader();
            float w = Ui.Portrait.x;
            var rt = Ui.Edge(parent, "Header", true, w, Height);
            Ui.Image(rt, "Fade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0.97f), Palette.Night.WithAlpha(0f)), Color.white, new Vector2(0, -16), new Vector2(w * 3, Height + 32));
            float left = -w / 2, right = w / 2;
            const float top = 52;

            // アバター（主人公の立ち絵の顔）とレベル
            var avatar = Ui.Plate(rt, "Avatar", new Vector2(left + 94, top), new Vector2(124, 124), Palette.VioletBright, 26);
            var window = Ui.Clip(avatar.transform.parent, "Window", Vector2.zero, new Vector2(112, 112));
            window.SetSiblingIndex(avatar.transform.GetSiblingIndex() + 1);
            PortraitView.Create(window, "Face", Cast.Player, new Vector2(0, -50), 210);
            var levelBadge = Ui.Rect(rt, "LevelBadge", new Vector2(left + 148, top - 46), new Vector2(62, 62));
            Ui.Shadow(levelBadge, "Shadow", new Vector2(0, -3), new Vector2(62, 62), 31, 0.5f);
            Ui.Sliced(levelBadge, "Face", Chrome.Gloss, Palette.Gold, Vector2.zero, new Vector2(62, 62), 31);
            Ui.Sliced(levelBadge, "Rim", Chrome.Rim, Palette.GoldLight, Vector2.zero, new Vector2(62, 62), 31);
            Ui.Text(levelBadge, "Lv", "Lv", Fonts.Label, 14, Palette.Ink, new Vector2(0, 15), new Vector2(40, 16));
            header.level = Ui.Text(levelBadge, "Level", "", Fonts.Display, 30, Palette.Ink, new Vector2(0, -6), new Vector2(60, 36));

            const float infoX = -148, infoWidth = 400;
            header.rank = Ui.Text(rt, "Rank", "", Fonts.Label, 32, Palette.Gold, new Vector2(infoX, top + 26), new Vector2(infoWidth, 40), TextAlignmentOptions.Left);
            Ui.Outline(header.rank, Palette.Ink, 0.18f);
            header.xpMeter = Meter.Create(rt, "Xp", new Vector2(infoX, top - 12), new Vector2(infoWidth, 24), Palette.Emerald);
            header.xp = Ui.Text(rt, "XpText", "", Fonts.Label, 18, Palette.Muted, new Vector2(infoX, top - 42), new Vector2(infoWidth, 24), TextAlignmentOptions.Left);
            Ui.IconButton(rt, "Help", IconKind.Help, new Vector2(right - 168, top), 76, new Color(1, 1, 1, 0.1f), openHelp);
            Ui.IconButton(rt, "Settings", IconKind.Settings, new Vector2(right - 74, top), 76, new Color(1, 1, 1, 0.1f), openSettings);

            // 所持金とコンプ
            const float row = -66;
            header.chips = CurrencyPill.Create(rt, new Vector2(-150, row), 500, Chrome.Coin, Color.white, Labels.Money, openShop);
            header.comps = CurrencyPill.Create(rt, new Vector2(282, row), 300, Chrome.Jewel, Palette.Cyan, v => v.ToString("N0"));
            Ui.Text(rt, "PlayMoney", "PLAY MONEY", Fonts.Label, 15, Palette.Muted.WithAlpha(0.8f), new Vector2(-150, row - 42), new Vector2(240, 20));
            Ui.Text(rt, "CompLabel", "COMP POINTS", Fonts.Label, 15, Palette.Muted.WithAlpha(0.8f), new Vector2(282, row - 42), new Vector2(240, 20));
            return header;
        }

        public void Refresh(RpgState state, long balance, bool animate)
        {
            var vip = Progression.RankFor(state.level);
            level.text = state.level.ToString();
            rank.text = $"{StoryNames.Rank(vip)} MEMBER";
            long need = Progression.XpToNext(state.level);
            xpMeter.SetFraction(need > 0 ? state.xp / (float)need : 1, animate);
            xp.text = need > 0 ? $"NEXT Lv  {need - state.xp} XP" : "MAX LEVEL";
            chips.SetValue(balance, animate);
            comps.SetValue(state.comps, animate);
        }
    }
}
