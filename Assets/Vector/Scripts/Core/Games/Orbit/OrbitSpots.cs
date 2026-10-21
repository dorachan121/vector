using System;

namespace Vector.Core.Orbit
{
    /// <summary>
    /// ORBIT のベットエリアの番号。内惑星の位置 0〜5、外惑星の方角 6〜9（北東・南東・南西・北西）、
    /// 外惑星の天地 10〜11（天・地）、内合 12、外合 13、惑星直列 14、彗星 15。
    /// </summary>
    public static class OrbitSpots
    {
        public const int FirstQuadrant = OrbitSpin.InnerSlots;
        public const int FirstHalf = FirstQuadrant + OrbitSpin.Quadrants;
        public const int InnerConjunction = FirstHalf + OrbitSpin.Halves;
        public const int OuterConjunction = InnerConjunction + 1;
        public const int Alignment = OuterConjunction + 1;
        public const int Comet = Alignment + 1;
        public const int Count = Comet + 1;

        public static int Inner(int slot)
        {
            if (slot < 0 || slot >= OrbitSpin.InnerSlots)
                throw new ArgumentOutOfRangeException(nameof(slot));
            return slot;
        }

        public static int Quadrant(int quadrant)
        {
            if (quadrant < 0 || quadrant >= OrbitSpin.Quadrants)
                throw new ArgumentOutOfRangeException(nameof(quadrant));
            return FirstQuadrant + quadrant;
        }

        public static int Half(int half)
        {
            if (half < 0 || half >= OrbitSpin.Halves)
                throw new ArgumentOutOfRangeException(nameof(half));
            return FirstHalf + half;
        }

        public static bool IsValid(int spot) => spot >= 0 && spot < Count;

        public static bool IsInner(int spot) => spot >= 0 && spot < FirstQuadrant;

        public static bool IsQuadrant(int spot) => spot >= FirstQuadrant && spot < FirstHalf;

        public static bool IsHalf(int spot) => spot >= FirstHalf && spot < InnerConjunction;

        /// <summary>内惑星の位置のベットエリアが指す位置。</summary>
        public static int SlotOf(int spot) => spot;

        /// <summary>方角のベットエリアが指す方角。</summary>
        public static int QuadrantOf(int spot) => spot - FirstQuadrant;

        /// <summary>天地のベットエリアが指す側。</summary>
        public static int HalfOf(int spot) => spot - FirstHalf;
    }
}
