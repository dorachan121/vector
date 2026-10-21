namespace Vector.Core.Crown
{
    /// <summary>振り直しの推奨。期待値はアンティ1単位あたりの払い戻し（賭け金を含む）。</summary>
    public readonly struct HoldAdvice
    {
        /// <summary>残すダイスの位置（ビット i が立っていれば i 番目を残す）。全部立っていれば止める。</summary>
        public readonly int HoldMask;

        /// <summary>推奨どおりにしたときの期待値。</summary>
        public readonly double ExpectedReturn;

        /// <summary>今の手で止めたときの期待値。</summary>
        public readonly double StandReturn;

        public HoldAdvice(int holdMask, double expectedReturn, double standReturn)
        {
            HoldMask = holdMask;
            ExpectedReturn = expectedReturn;
            StandReturn = standReturn;
        }

        public bool Stand => HoldMask == (1 << DiceRank.DiceCount) - 1;
    }
}
