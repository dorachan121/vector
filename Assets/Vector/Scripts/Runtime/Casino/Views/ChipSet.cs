using System.Collections.Generic;
using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>チップの額面と配色（カジノで一般的な色分け）。</summary>
    public static class ChipSet
    {
        public static readonly IReadOnlyList<long> Denominations = new long[] { 1, 5, 10, 100, 500, 1000, 5000, 10000 };

        public static Color Body(long value)
        {
            switch (value)
            {
                case 1: return Theme.Hex("E9E6DF");
                case 5: return Theme.Hex("C8202D");
                case 10: return Theme.Hex("1D5FC2");
                case 100: return Theme.Hex("1B1B1F");
                case 500: return Theme.Hex("6B2FA0");
                case 1000: return Theme.Hex("E7B416");
                case 5000: return Theme.Hex("D2691E");
                default: return Theme.Hex("0F8C83");
            }
        }

        public static Color Accent(long value)
        {
            switch (value)
            {
                case 1: return Theme.Hex("2F6FD6");
                case 100: return Theme.Gold;
                case 500: return Theme.Hex("F3D35B");
                case 1000: return Theme.Hex("1B1B1F");
                case 10000: return Theme.GoldBright;
                default: return Color.white;
            }
        }

        public static Color LabelColor(long value) => value == 1 || value == 1000 ? Theme.Ink : Color.white;

        /// <summary>金額を大きい額面から順にチップへ分解する（チップの山の見た目用）。</summary>
        public static List<long> Breakdown(long amount, int maxChips)
        {
            var chips = new List<long>();
            for (int i = Denominations.Count - 1; i >= 0 && chips.Count < maxChips; i--)
            {
                long d = Denominations[i];
                while (amount >= d && chips.Count < maxChips)
                {
                    chips.Add(d);
                    amount -= d;
                }
            }
            return chips;
        }
    }
}
