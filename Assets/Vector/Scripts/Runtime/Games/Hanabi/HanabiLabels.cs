using UnityEngine;
using Vector.Core.Hanabi;
using Vector.Game.UI;

namespace Vector.Game.Hanabi
{
    /// <summary>HANABI の倍率の表示と色。</summary>
    public static class HanabiLabels
    {
        /// <summary>百分の一倍の値を「x1.23」に。</summary>
        public static string Multiple(int hundredths) => "x" + (hundredths / (double)HanabiRules.One).ToString("0.00");

        /// <summary>倍率が大きいほど華やかな色（その場で暴発は灰色）。</summary>
        public static Color ColorOf(int hundredths)
        {
            if (hundredths <= HanabiRules.One)
                return Theme.Muted;
            if (hundredths < 200)
                return Theme.Ivory;
            if (hundredths < 500)
                return Palette.GoldLight;
            if (hundredths < 1_000)
                return Palette.Gold;
            if (hundredths < 10_000)
                return Palette.Cyan;
            return Palette.Magenta;
        }
    }
}
