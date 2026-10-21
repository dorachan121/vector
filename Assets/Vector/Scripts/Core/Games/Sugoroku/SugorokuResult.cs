using System.Collections.Generic;

namespace Vector.Core.Sugoroku
{
    /// <summary>SUGOROKU の1回の道中の精算結果。</summary>
    public sealed class SugorokuResult : IRoundOutcome
    {
        public SugorokuResult(bool reachedGoal, int tier, IReadOnlyList<SugorokuStep> steps, IReadOnlyList<SpotResult> bets)
        {
            ReachedGoal = reachedGoal;
            Tier = tier;
            Steps = steps;
            Bets = bets;
        }

        /// <summary>回数のうちに上がりへ着いた。</summary>
        public bool ReachedGoal { get; }

        /// <summary>終わったときの倍率の段。</summary>
        public int Tier { get; }

        public IReadOnlyList<SugorokuStep> Steps { get; }
        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;
    }
}
