using System.Collections.Generic;

namespace Vector.Core.Orbit
{
    /// <summary>ORBIT の1回のスピンの精算結果。</summary>
    public sealed class OrbitResult : IRoundOutcome
    {
        public OrbitResult(OrbitSpin spin, IReadOnlyList<SpotResult> bets)
        {
            Spin = spin;
            Bets = bets;
        }

        public OrbitSpin Spin { get; }
        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;
    }
}
