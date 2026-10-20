namespace Vector.Core
{
    /// <summary>ラインの種類を選んだ場合の見込み。</summary>
    public readonly struct FamilyEstimate
    {
        public readonly LineFamily Family;
        public readonly double WinRate;
        public readonly double TieRate;

        /// <summary>メインベット1単位あたりの期待値。</summary>
        public readonly double ExpectedValue;

        public FamilyEstimate(LineFamily family, double winRate, double tieRate, double expectedValue)
        {
            Family = family;
            WinRate = winRate;
            TieRate = tieRate;
            ExpectedValue = expectedValue;
        }
    }
}
