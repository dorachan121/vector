using System.Collections.Generic;

namespace Vector.Core.Hanabi
{
    /// <summary>HANABI の1発の精算結果。</summary>
    public sealed class HanabiResult : IRoundOutcome
    {
        public HanabiResult(int crashHundredths, int cashedHundredths, IReadOnlyList<SpotResult> bets)
        {
            CrashHundredths = crashHundredths;
            CashedHundredths = cashedHundredths;
            Bets = bets;
        }

        /// <summary>暴発点（百分の一倍）。</summary>
        public int CrashHundredths { get; }

        /// <summary>開いた倍率（百分の一倍。開けなかったら 0）。</summary>
        public int CashedHundredths { get; }

        public bool Opened => CashedHundredths > 0;

        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;
    }
}
