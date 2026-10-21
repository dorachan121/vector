using System;

namespace Vector.Core.Orbit
{
    /// <summary>
    /// ORBIT の1回のスピンの結果。太陽のまわりを3つの惑星が回り、内惑星は 6、中惑星は 12、外惑星は 24 の位置のどこかに止まる。
    /// 位置は真上（北）から時計回りに数え、角度は外惑星の1目盛り（15°）を単位にそろえて比べる。
    /// どの位置も北・東・南・西の線から半目盛り（7.5°）ずれているので、方角と天地の境目に惑星が止まることはない。
    /// <para>彗星が横切ったスピン（37 回に 1 回）は惑星が隠れ、彗星への賭けだけが当たる（ルーレットの 0 と同じ役目）。</para>
    /// </summary>
    public readonly struct OrbitSpin
    {
        public const int InnerSlots = 6;
        public const int MiddleSlots = 12;
        public const int OuterSlots = 24;

        /// <summary>方角（北東・南東・南西・北西）の数。外惑星の位置 6 つずつ。</summary>
        public const int Quadrants = 4;

        /// <summary>天（上半分）と地（下半分）。外惑星の位置 12 ずつ。</summary>
        public const int Halves = 2;

        public readonly bool Comet;
        public readonly int Inner;
        public readonly int Middle;
        public readonly int Outer;

        OrbitSpin(bool comet, int inner, int middle, int outer)
        {
            Comet = comet;
            Inner = inner;
            Middle = middle;
            Outer = outer;
        }

        /// <summary>彗星が横切ったスピン。</summary>
        public static OrbitSpin CometPass => new OrbitSpin(true, 0, 0, 0);

        /// <summary>惑星が止まったスピン。</summary>
        public static OrbitSpin Planets(int inner, int middle, int outer)
        {
            if (inner < 0 || inner >= InnerSlots)
                throw new ArgumentOutOfRangeException(nameof(inner));
            if (middle < 0 || middle >= MiddleSlots)
                throw new ArgumentOutOfRangeException(nameof(middle));
            if (outer < 0 || outer >= OuterSlots)
                throw new ArgumentOutOfRangeException(nameof(outer));
            return new OrbitSpin(false, inner, middle, outer);
        }

        /// <summary>内惑星の角度（外惑星の目盛り単位、0〜23）。</summary>
        public int InnerRay => Inner * (OuterSlots / InnerSlots);

        /// <summary>中惑星の角度（外惑星の目盛り単位、0〜23）。</summary>
        public int MiddleRay => Middle * (OuterSlots / MiddleSlots);

        /// <summary>内合: 内惑星と中惑星が太陽から見て一直線に並んだ。</summary>
        public bool InnerConjunction => !Comet && InnerRay == MiddleRay;

        /// <summary>外合: 中惑星と外惑星が一直線に並んだ。</summary>
        public bool OuterConjunction => !Comet && MiddleRay == Outer;

        /// <summary>惑星直列: 3つとも一直線に並んだ。</summary>
        public bool Alignment => InnerConjunction && OuterConjunction;

        /// <summary>外惑星の方角（0 北東・1 南東・2 南西・3 北西）。</summary>
        public int Quadrant => Outer / (OuterSlots / Quadrants);

        /// <summary>外惑星が天（0: 上半分）にあるか地（1: 下半分）にあるか。</summary>
        public int Half => (Outer + OuterSlots / 4) % OuterSlots < OuterSlots / Halves ? 0 : 1;

        /// <summary>そのベットエリアが当たったか。</summary>
        public bool Wins(int spot)
        {
            if (!OrbitSpots.IsValid(spot))
                throw new ArgumentOutOfRangeException(nameof(spot));
            if (Comet)
                return spot == OrbitSpots.Comet;
            if (OrbitSpots.IsInner(spot))
                return OrbitSpots.SlotOf(spot) == Inner;
            if (OrbitSpots.IsQuadrant(spot))
                return OrbitSpots.QuadrantOf(spot) == Quadrant;
            if (OrbitSpots.IsHalf(spot))
                return OrbitSpots.HalfOf(spot) == Half;
            switch (spot)
            {
                case OrbitSpots.InnerConjunction: return InnerConjunction;
                case OrbitSpots.OuterConjunction: return OuterConjunction;
                case OrbitSpots.Alignment: return Alignment;
                default: return false;
            }
        }

        public override string ToString() => Comet ? "Comet" : $"{Inner}/{Middle}/{Outer}";
    }
}
