using UnityEngine;
using Vector.Core.Sugoroku;
using Vector.Game.UI;

namespace Vector.Game.Sugoroku
{
    /// <summary>SUGOROKU の表示用の文字（マスの名前・効果・配当の倍率）と色。</summary>
    public static class SugorokuLabels
    {
        public static readonly Color Paper = Theme.Hex("F1E3C2");
        public static readonly Color Ink = Theme.Hex("2B1D12");
        public static readonly Color Vermilion = Theme.Hex("C8352B");

        /// <summary>マスに書く字。</summary>
        public static string Glyph(Square square)
        {
            switch (square)
            {
                case Square.Coin: return "小判";
                case Square.Treasure: return "宝";
                case Square.Omen: return "凶";
                case Square.Bridge: return "橋";
                case Square.Pit: return "穴";
                default: return "";
            }
        }

        /// <summary>マスの効果の短い説明。</summary>
        public static string Effect(SugorokuRules rules, Square square)
        {
            switch (square)
            {
                case Square.Coin: return "段 +1";
                case Square.Treasure: return "段 +2";
                case Square.Omen: return "段 −1";
                case Square.Bridge: return $"{rules.BridgeSteps}マス先へ";
                case Square.Pit: return $"{rules.PitSteps}マス戻る";
                default: return "";
            }
        }

        public static Color ColorOf(Square square)
        {
            switch (square)
            {
                case Square.Coin: return Theme.Hex("C9962E");
                case Square.Treasure: return Theme.Hex("8E3FD0");
                case Square.Omen: return Vermilion;
                case Square.Bridge: return Theme.Hex("2E7D6B");
                case Square.Pit: return Theme.Hex("4A3A2A");
                default: return Ink;
            }
        }

        /// <summary>段の配当（「x0.4」など）。</summary>
        public static string Pay(SugorokuRules rules, int tier) => "x" + (rules.PayFor(tier) / 100.0).ToString("0.##");
    }
}
