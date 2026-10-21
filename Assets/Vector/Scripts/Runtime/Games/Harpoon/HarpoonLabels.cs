using UnityEngine;
using Vector.Core.Harpoon;
using Vector.Game.UI;

namespace Vector.Game.Harpoon
{
    /// <summary>HARPOON の表示用の名前・倍率・確率の文字列。</summary>
    public static class HarpoonLabels
    {
        static readonly string[] Names = { "イワシ", "クマノミ", "フグ", "ウミガメ", "カジキ", "サメ", "クジラ", "リヴァイアサン", "金のクラゲ" };

        public static string Name(int quarry) => Names[quarry];

        /// <summary>倍率（金のクラゲは幅で）。</summary>
        public static string Multiple(HarpoonRules rules, int quarry)
            => rules.IsMystery(quarry) ? $"x{rules.MinMultiple(quarry)}–{rules.MaxMultiple(quarry)}" : $"x{rules.MinMultiple(quarry)}";

        /// <summary>捕まえる確率（小さいものは桁を増やす）。</summary>
        public static string Chance(HarpoonRules rules, int quarry)
        {
            double p = rules.CatchProbability(quarry);
            return p >= 0.1 ? $"{p:P1}" : p >= 0.01 ? $"{p:P2}" : $"{p:P3}";
        }

        /// <summary>獲物ごとの差し色（配当の帯と、捕まえたときの文字）。</summary>
        public static Color ColorOf(int quarry)
        {
            switch (quarry)
            {
                case Quarry.Sardine: return Theme.Hex("9CC4E8");
                case Quarry.Clownfish: return Theme.Hex("FF9A4A");
                case Quarry.Puffer: return Theme.Hex("F2C14E");
                case Quarry.Turtle: return Theme.Hex("7FC97A");
                case Quarry.Swordfish: return Theme.Hex("6FA6E0");
                case Quarry.Shark: return Theme.Hex("B9C6D0");
                case Quarry.Whale: return Theme.Hex("7FB2FF");
                case Quarry.Leviathan: return Theme.Hex("34E0C8");
                default: return Palette.GoldLight;
            }
        }
    }
}
