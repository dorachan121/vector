using UnityEngine;
using Vector.Core;

namespace Vector.Game.UI
{
    /// <summary>
    /// カジノテーブルの配色。エメラルドのフェルト、ゴールドの装飾、漆黒のレールが基調。
    /// 陣営色はバカラの慣習（プレイヤー=青、バンカー/ディーラー=赤、タイ=緑）に合わせる。
    /// </summary>
    public static class Theme
    {
        public static readonly Color Backdrop = Hex("070A09");
        public static readonly Color FeltCenter = Hex("12704A");
        public static readonly Color FeltEdge = Hex("06311F");
        public static readonly Color RailDark = Hex("140B07");
        public static readonly Color RailLight = Hex("3A2416");

        public static readonly Color Gold = Hex("E3C06E");
        public static readonly Color GoldBright = Hex("FFE7A3");
        public static readonly Color GoldDeep = Hex("9C7426");
        public static readonly Color FeltPrint = new Color(0.89f, 0.75f, 0.43f, 0.55f);

        public static readonly Color Ivory = Hex("F6F0E1");
        public static readonly Color Muted = Hex("A7B5AC");
        public static readonly Color Ink = Hex("16120D");

        public static readonly Color PlayerBlue = Hex("4E9BFF");
        public static readonly Color DealerRed = Hex("FF5A5F");
        public static readonly Color TieGreen = Hex("3DDC97");

        public static readonly Color Panel = new Color(0.03f, 0.05f, 0.04f, 0.82f);
        public static readonly Color PanelLine = new Color(0.89f, 0.75f, 0.43f, 0.35f);

        public static readonly Color CardFace = Hex("FBF8F0");
        public static readonly Color CardRed = Hex("C4262E");
        public static readonly Color CardBlack = Hex("1A1A1E");
        public static readonly Color CardBack = Hex("6E0F1D");

        public static readonly Color Positive = Hex("7CF0A6");
        public static readonly Color Negative = Hex("FF7B7B");

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);

        public static Color ColorFor(RoundWinner winner)
        {
            switch (winner)
            {
                case RoundWinner.Player: return PlayerBlue;
                case RoundWinner.Dealer: return DealerRed;
                default: return TieGreen;
            }
        }
    }
}
