using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// 画面の枠まわり（ホーム・ヘッダー・ボタン・ダイアログ）の配色。夜のカジノリゾートを、深い藍紫と金、宝石色のアクセントで表す。
    /// テーブルのフェルトや陣営の色は <see cref="Theme"/> が持つ。
    /// </summary>
    public static class Palette
    {
        public static readonly Color Night = Theme.Hex("0B0620");
        public static readonly Color NightDeep = Theme.Hex("06030F");
        public static readonly Color Violet = Theme.Hex("1E0F45");
        public static readonly Color VioletBright = Theme.Hex("3A1C80");
        public static readonly Color Haze = Theme.Hex("7B3FE4");

        public static readonly Color Gold = Theme.Hex("FFD36B");
        public static readonly Color GoldLight = Theme.Hex("FFF1C2");
        public static readonly Color GoldMid = Theme.Hex("F2B33D");
        public static readonly Color GoldDark = Theme.Hex("A86A12");

        public static readonly Color Ruby = Theme.Hex("FF4D6D");
        public static readonly Color Magenta = Theme.Hex("FF3EA5");
        public static readonly Color Emerald = Theme.Hex("2EE59D");
        public static readonly Color Sapphire = Theme.Hex("3C8CFF");
        public static readonly Color Cyan = Theme.Hex("36E2FF");
        public static readonly Color Amber = Theme.Hex("FF9F1C");
        public static readonly Color Amethyst = Theme.Hex("B05CFF");
        public static readonly Color Slate = Theme.Hex("4B3C7A");

        public static readonly Color Ivory = Theme.Hex("FFF8EC");
        public static readonly Color Muted = Theme.Hex("B9AEDB");
        public static readonly Color Ink = Theme.Hex("1A0F2E");

        /// <summary>カードやダイアログの地（上が明るい縦グラデーションの両端）。</summary>
        public static readonly Color PanelTop = Theme.Hex("2B1763");
    }
}
