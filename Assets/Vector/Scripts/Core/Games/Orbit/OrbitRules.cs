using System;

namespace Vector.Core.Orbit
{
    /// <summary>
    /// ORBIT のルール（不変）。惑星への賭けは、彗星が来なかったときの確率どおりの「真のオッズ」で払う。
    /// ハウスの取り分は 37 回に 1 回横切る彗星（そのスピンは彗星への賭けだけが当たる）なので、
    /// どの賭け方をしても RTP は 36/37 ≒ 97.30%（シングルゼロのルーレットと同じ）で変わらない。
    /// </summary>
    public sealed class OrbitRules : IBetLimits
    {
        public static readonly OrbitRules Standard = new OrbitRules();

        /// <summary>彗星が横切る確率の分母（37 回に 1 回）。</summary>
        public const int CometOneIn = 37;

        public long MinBet { get; } = 5;

        /// <summary>ベット受付の制限時間（秒）。</summary>
        public float BettingSeconds { get; } = 20f;

        public bool IsSpotAvailable(int spot) => OrbitSpots.IsValid(spot);

        public long MinFor(int spot) => MinBet;

        /// <summary>配当が大きい賭けほど上限を低くする（どの賭けも最大の払い戻しが 1.5 万ドル前後になるように）。</summary>
        public long MaxFor(int spot)
        {
            if (OrbitSpots.IsInner(spot))
                return 2_000;
            if (OrbitSpots.IsQuadrant(spot))
                return 5_000;
            if (OrbitSpots.IsHalf(spot))
                return 10_000;
            switch (spot)
            {
                case OrbitSpots.InnerConjunction: return 1_000;
                case OrbitSpots.OuterConjunction: return 500;
                case OrbitSpots.Alignment: return 50;
                case OrbitSpots.Comet: return 300;
                default: throw new ArgumentOutOfRangeException(nameof(spot));
            }
        }

        public bool IsPrimary(int spot) => OrbitSpots.IsValid(spot);

        /// <summary>当たったときの配当（勝ち額 : 賭け額）。彗星が来なかったときの確率どおりの値。</summary>
        public Odds PaysFor(int spot)
        {
            if (OrbitSpots.IsInner(spot))
                return new Odds(OrbitSpin.InnerSlots - 1);
            if (OrbitSpots.IsQuadrant(spot))
                return new Odds(OrbitSpin.Quadrants - 1);
            if (OrbitSpots.IsHalf(spot))
                return new Odds(OrbitSpin.Halves - 1);
            switch (spot)
            {
                case OrbitSpots.InnerConjunction: return new Odds(OrbitSpin.MiddleSlots - 1);
                case OrbitSpots.OuterConjunction: return new Odds(OrbitSpin.OuterSlots - 1);
                case OrbitSpots.Alignment: return new Odds(OrbitSpin.MiddleSlots * OrbitSpin.OuterSlots - 1);
                case OrbitSpots.Comet: return new Odds(CometOneIn - 2);
                default: throw new ArgumentOutOfRangeException(nameof(spot));
            }
        }

        /// <summary>そのベットエリアに当たる確率（全スピンを数え上げた厳密値）。</summary>
        public double ProbabilityOf(int spot)
        {
            if (!OrbitSpots.IsValid(spot))
                throw new ArgumentOutOfRangeException(nameof(spot));
            double comet = 1.0 / CometOneIn;
            if (spot == OrbitSpots.Comet)
                return comet;
            int hits = 0;
            for (int i = 0; i < OrbitSpin.InnerSlots; i++)
            {
                for (int j = 0; j < OrbitSpin.MiddleSlots; j++)
                {
                    for (int k = 0; k < OrbitSpin.OuterSlots; k++)
                    {
                        if (OrbitSpin.Planets(i, j, k).Wins(spot))
                            hits++;
                    }
                }
            }
            return (1 - comet) * hits / (OrbitSpin.InnerSlots * OrbitSpin.MiddleSlots * OrbitSpin.OuterSlots);
        }

        /// <summary>そのベットエリアだけに賭け続けたときの厳密な RTP（賭け金 1 あたりの払い戻しの期待値）。</summary>
        public double ReturnOf(int spot) => ProbabilityOf(spot) * (PaysFor(spot).Ratio + 1);
    }
}
