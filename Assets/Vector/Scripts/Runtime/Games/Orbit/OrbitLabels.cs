using UnityEngine;
using Vector.Core.Orbit;
using Vector.Game.UI;

namespace Vector.Game.Orbit
{
    /// <summary>ORBIT の画面の文言と色。</summary>
    public static class OrbitLabels
    {
        public static readonly Color InnerColor = Palette.Cyan;
        public static readonly Color MiddleColor = Palette.Amber;
        public static readonly Color OuterColor = Palette.Amethyst;
        public static readonly Color CometColor = Palette.Ruby;

        static readonly string[] quadrants = { "北東", "南東", "南西", "北西" };
        static readonly string[] halves = { "天", "地" };

        public static string Quadrant(int quadrant) => quadrants[quadrant];

        public static string Half(int half) => halves[half];

        /// <summary>トーストなどに出すベットエリアの名前。</summary>
        public static string Spot(int spot)
        {
            if (OrbitSpots.IsInner(spot))
                return $"内惑星 {OrbitSpots.SlotOf(spot) + 1}";
            if (OrbitSpots.IsQuadrant(spot))
                return $"外惑星 {Quadrant(OrbitSpots.QuadrantOf(spot))}";
            if (OrbitSpots.IsHalf(spot))
                return $"外惑星 {Half(OrbitSpots.HalfOf(spot))}";
            switch (spot)
            {
                case OrbitSpots.InnerConjunction: return "内合";
                case OrbitSpots.OuterConjunction: return "外合";
                case OrbitSpots.Alignment: return "惑星直列";
                default: return "彗星";
            }
        }

        /// <summary>配当の表示（例: 5:1）。</summary>
        public static string Odds(OrbitRules rules, int spot) => $"{rules.PaysFor(spot).Win}:1";

        /// <summary>結果の見出し。</summary>
        public static string Headline(OrbitSpin spin)
        {
            if (spin.Comet)
                return "COMET!";
            if (spin.Alignment)
                return "GRAND ALIGNMENT!";
            if (spin.InnerConjunction)
                return "INNER CONJUNCTION";
            if (spin.OuterConjunction)
                return "OUTER CONJUNCTION";
            return "THE PLANETS STOP";
        }

        /// <summary>結果の説明（例: 内惑星 3 · 外惑星 北東・天）。</summary>
        public static string Describe(OrbitSpin spin)
        {
            if (spin.Comet)
                return "彗星が横切り、惑星が隠れた — 彗星への賭けだけが当たり";
            string line = $"内惑星 {spin.Inner + 1}  ·  外惑星 {Quadrant(spin.Quadrant)}・{Half(spin.Half)}";
            if (spin.Alignment)
                return "3つの惑星が一直線に！  " + line;
            if (spin.InnerConjunction)
                return "内惑星と中惑星が並んだ  ·  " + line;
            if (spin.OuterConjunction)
                return "中惑星と外惑星が並んだ  ·  " + line;
            return line;
        }

        /// <summary>履歴の1件（短い表記）。</summary>
        public static string Short(OrbitSpin spin)
        {
            if (spin.Comet)
                return "<color=#FF4D6D>彗星</color>";
            if (spin.Alignment)
                return "<color=#FFD36B>直列</color>";
            string tail = spin.InnerConjunction ? " <color=#36E2FF>内合</color>" : spin.OuterConjunction ? " <color=#FF9F1C>外合</color>" : "";
            return $"{spin.Inner + 1}/{Quadrant(spin.Quadrant)}{tail}";
        }
    }
}
