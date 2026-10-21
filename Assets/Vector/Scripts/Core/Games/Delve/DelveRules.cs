using System;
using System.Collections.Generic;

namespace Vector.Core.Delve
{
    /// <summary>
    /// DELVE のルール（不変）。深さ d まで抜けて引き返したときの払い戻し倍率は
    /// 「払戻率 ÷ そこまで生き残る確率」を 0.01 倍単位で切り捨てた値なので、どの深さで引き返しても RTP は払戻率以下になり、
    /// どんな打ち方をしても RTP は変わらない（違うのは当たったときの大きさだけ）。
    /// </summary>
    public sealed class DelveRules : IBetLimits
    {
        public const int Main = 0;

        public static readonly DelveRules Standard = new DelveRules(0.97);

        readonly DelveFloor[] floors =
        {
            new DelveFloor(4, 1), new DelveFloor(4, 1),
            new DelveFloor(3, 1), new DelveFloor(3, 1),
            new DelveFloor(2, 1), new DelveFloor(2, 1),
            new DelveFloor(3, 2), new DelveFloor(3, 2),
            new DelveFloor(4, 3), new DelveFloor(4, 3),
        };

        readonly int[] multiplierHundredths;

        public DelveRules(double targetReturn)
        {
            if (targetReturn <= 0 || targetReturn >= 1)
                throw new ArgumentOutOfRangeException(nameof(targetReturn));
            TargetReturn = targetReturn;
            multiplierHundredths = new int[floors.Length];
            double survival = 1;
            for (int d = 0; d < floors.Length; d++)
            {
                survival *= floors[d].Survival;
                multiplierHundredths[d] = (int)Math.Floor(targetReturn / survival * 100 + 1e-9);
            }
        }

        public double TargetReturn { get; }

        public IReadOnlyList<DelveFloor> Floors => floors;

        public int Depths => floors.Length;

        public long MinBet { get; } = 10;
        public long MaxBet { get; } = 1_000;

        public float BettingSeconds { get; } = 15f;
        public float DecisionSeconds { get; } = 25f;

        /// <summary>深さ depth（1始まり）まで抜けて引き返したときの払い戻し倍率（100倍した値、賭け金を含む）。</summary>
        public int MultiplierHundredths(int depth)
        {
            if (depth < 1 || depth > floors.Length)
                throw new ArgumentOutOfRangeException(nameof(depth));
            return multiplierHundredths[depth - 1];
        }

        /// <summary>深さ depth まで生き残る確率。</summary>
        public double SurvivalTo(int depth)
        {
            double s = 1;
            for (int d = 0; d < depth; d++)
                s *= floors[d].Survival;
            return s;
        }

        /// <summary>深さ depth で引き返すと決めた打ち方の厳密な RTP。</summary>
        public double ReturnIfCashingAt(int depth) => SurvivalTo(depth) * MultiplierHundredths(depth) / 100.0;

        /// <summary>深さ depth で引き返したときの勝ち額の倍率（賭け金を含まない）。</summary>
        public Odds PaysAt(int depth) => new Odds(MultiplierHundredths(depth) - 100, 100);

        public bool IsSpotAvailable(int spot) => spot == Main;
        public long MinFor(int spot) => MinBet;
        public long MaxFor(int spot) => MaxBet;
        public bool IsPrimary(int spot) => spot == Main;
    }
}
