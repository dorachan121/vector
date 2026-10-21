using System.Collections.Generic;

namespace Vector.Core.Alchemia
{
    /// <summary>1回のスピンの結果（最初の盤面と連鎖の全段）。</summary>
    public sealed class SpinResult : IRoundOutcome
    {
        public SpinResult(Symbol[] initial, IReadOnlyList<CascadeStep> steps, long payHundredths, bool capped, IReadOnlyList<SpotResult> bets)
        {
            Initial = initial;
            Steps = steps;
            PayHundredths = payHundredths;
            Capped = capped;
            Bets = bets;
        }

        public Symbol[] Initial { get; }
        public IReadOnlyList<CascadeStep> Steps { get; }

        /// <summary>払い戻しの倍率（賭け金を含む。100倍した値。上限を適用した後）。</summary>
        public long PayHundredths { get; }

        /// <summary>勝ちの上限に達して打ち切った。</summary>
        public bool Capped { get; }

        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;

        /// <summary>払い戻しが賭け金の何倍か。</summary>
        public double WinMultiple => PayHundredths / 100.0;

        public bool HasTransmutation
        {
            get
            {
                foreach (var step in Steps)
                {
                    foreach (var m in step.Merges)
                    {
                        if (m.IsTransmutation)
                            return true;
                    }
                }
                return false;
            }
        }

        public int HighestTierCreated
        {
            get
            {
                int best = 0;
                foreach (var step in Steps)
                {
                    foreach (var m in step.Merges)
                        best = m.IsTransmutation ? Symbol.MaxTier + 1 : System.Math.Max(best, m.NewTier);
                }
                return best;
            }
        }
    }
}
