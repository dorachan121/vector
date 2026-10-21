using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>番号で表すベットエリア1つの精算結果。</summary>
    public readonly struct SpotResult
    {
        public readonly int Spot;
        public readonly long Stake;
        public readonly BetOutcome Outcome;

        /// <summary>払い戻し総額（賭け金を含む）。負けなら0。</summary>
        public readonly long Return;

        public SpotResult(int spot, long stake, BetOutcome outcome, long totalReturn)
        {
            if (stake <= 0)
                throw new ArgumentOutOfRangeException(nameof(stake));
            if (totalReturn < 0)
                throw new ArgumentOutOfRangeException(nameof(totalReturn));
            Spot = spot;
            Stake = stake;
            Outcome = outcome;
            Return = totalReturn;
        }

        public long Net => Return - Stake;

        public static SpotResult Win(int spot, long stake, Odds odds)
            => new SpotResult(spot, stake, BetOutcome.Win, checked(stake + odds.WinningsFor(stake)));

        public static SpotResult Push(int spot, long stake) => new SpotResult(spot, stake, BetOutcome.Push, stake);

        public static SpotResult Lose(int spot, long stake) => new SpotResult(spot, stake, BetOutcome.Lose, 0);

        public static long TotalStake(IReadOnlyList<SpotResult> bets)
        {
            long sum = 0;
            foreach (var b in bets)
                sum += b.Stake;
            return sum;
        }

        public static long TotalReturn(IReadOnlyList<SpotResult> bets)
        {
            long sum = 0;
            foreach (var b in bets)
                sum += b.Return;
            return sum;
        }
    }
}
