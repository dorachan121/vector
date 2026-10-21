namespace Vector.Core
{
    /// <summary>1つのベットの精算結果。</summary>
    public readonly struct BetResult
    {
        public readonly BetSpot Spot;
        public readonly long Stake;
        public readonly BetOutcome Outcome;

        /// <summary>払い戻し総額（賭け金を含む）。負けなら0。</summary>
        public readonly long Return;

        public BetResult(BetSpot spot, long stake, BetOutcome outcome, long totalReturn)
        {
            Spot = spot;
            Stake = stake;
            Outcome = outcome;
            Return = totalReturn;
        }

        public long Net => Return - Stake;
    }
}
