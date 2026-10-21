using System.Collections.Generic;

namespace Vector.Core.Delve
{
    /// <summary>DELVE の1回の探索の結果。</summary>
    public sealed class DelveResult : IRoundOutcome
    {
        public DelveResult(int clearedDepth, bool busted, IReadOnlyList<SpotResult> bets)
        {
            ClearedDepth = clearedDepth;
            Busted = busted;
            Bets = bets;
        }

        /// <summary>無事に抜けた深さ（0 なら1つ目で魔物に遭った）。</summary>
        public int ClearedDepth { get; }

        /// <summary>魔物に遭って何も持ち帰れなかった。</summary>
        public bool Busted { get; }

        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;
    }
}
