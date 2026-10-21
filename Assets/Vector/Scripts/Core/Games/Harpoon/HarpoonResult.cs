using System.Collections.Generic;

namespace Vector.Core.Harpoon
{
    /// <summary>HARPOON の1発の精算結果。</summary>
    public sealed class HarpoonResult : IRoundOutcome
    {
        public HarpoonResult(int quarry, int multiple, IReadOnlyList<SpotResult> bets)
        {
            Quarry = quarry;
            Multiple = multiple;
            Bets = bets;
        }

        /// <summary>狙った獲物。</summary>
        public int Quarry { get; }

        /// <summary>捕まえたときの倍率（逃げられたら 0）。</summary>
        public int Multiple { get; }

        public bool Caught => Multiple > 0;

        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;
    }
}
