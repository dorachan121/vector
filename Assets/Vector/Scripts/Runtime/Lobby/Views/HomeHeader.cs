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
    /// <summary>
    /// ホーム画面の上部。左にプレイヤーの札（顔・レベル・会員ランク・経験値）、右に所持金とコンプ。画面の上端に付く。
    /// </summary>
    public sealed class HomeHeader
    {
        public const float Height = 176f;

        TextMeshProUGUI level;
        TextMeshProUGUI rank;
        Meter xpMeter;
        CurrencyPill chips;
        CurrencyPill comps;

        public static HomeHeader Create(Transform parent, Action openShop)
        {
            var header = new HomeHeader();
            float w = Ui.Portrait.x;
            var rt = Ui.Edge(parent, "Header", true, w, Height);
            Ui.Image(rt, "Fade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0.9f), Palette.NightDeep.WithAlpha(0f)), Color.white, new Vector2(0, 10), new Vector2(w * 3, Height + 60));
            // 縦に長い端末で画面の枠の上に余った所も、同じ暗さで続ける
            float fadeTop = 10 + (Height + 60) / 2;
            Ui.Image(rt, "FadeAbove", null, Palette.NightDeep.WithAlpha(0.9f), new Vector2(0, fadeTop + ScreenFrame.Bleed / 2), new Vector2(w * 3, ScreenFrame.Bleed));
            float left = -w / 2;

            // プレイヤーの札
            var plate = new Vector2(left + 262, 4);
            Ui.Shadow(rt, "PlateShadow", plate + new Vector2(0, -5), new Vector2(500, 132), 30, 0.55f);
            Ui.Sliced(rt, "Plate", Chrome.Shade, Palette.Violet.WithAlpha(0.92f), plate, new Vector2(500, 132), 30);
            Ui.Sliced(rt, "PlateRim", Chrome.Rim, Palette.GoldMid.WithAlpha(0.85f), plate, new Vector2(500, 132), 30);
            var avatar = Ui.Card(rt, "Avatar", new Vector2(left + 86, 4), new Vector2(118, 118), Palette.VioletBright, 24);
            var window = Ui.Clip(avatar, "Window", Vector2.zero, new Vector2(106, 106));
            window.SetSiblingIndex(2);
            PortraitView.Create(window, "Face", Cast.Player, new Vector2(0, -50), 200);
            header.level = Ui.Headline(rt, "Level", "", Fonts.Display, 44, new Vector2(left + 320, 34), new Vector2(260, 52), TextAlignmentOptions.Left);
            header.rank = Ui.Text(rt, "Rank", "", Fonts.Body, 22, Palette.GoldLight, new Vector2(left + 320, -6), new Vector2(260, 28), TextAlignmentOptions.Left);
            header.xpMeter = Meter.Create(rt, "Xp", new Vector2(left + 320, -38), new Vector2(260, 20), Palette.Emerald);

            // 所持金とコンプ
            float x = w / 2 - 196;
            header.chips = CurrencyPill.Create(rt, new Vector2(x, 38), 360, Chrome.Coin, Color.white, Labels.Money, openShop);
            header.comps = CurrencyPill.Create(rt, new Vector2(x, -32), 360, Chrome.Jewel, Palette.Cyan, v => v.ToString("N0") + " pt");
            return header;
        }

        public void Refresh(RpgState state, long balance, bool animate)
        {
            var vip = Progression.RankFor(state.level);
            level.text = $"Lv.{state.level}";
            rank.text = $"{StoryNames.Rank(vip)} 会員";
            long need = Progression.XpToNext(state.level);
            xpMeter.SetFraction(need > 0 ? state.xp / (float)need : 1, animate);
            chips.SetValue(balance, animate);
            comps.SetValue(state.comps, animate);
        }
    }
}
