namespace Vector.Core
{
    /// <summary>ベット検証の結果と、問題のあったベットエリア・上下限。</summary>
    public readonly struct BetCheck
    {
        public readonly BetError Error;
        public readonly BetSpot Spot;
        public readonly long Limit;

        public BetCheck(BetError error, BetSpot spot = default, long limit = 0)
        {
            Error = error;
            Spot = spot;
            Limit = limit;
        }

        public bool IsValid => Error == BetError.None;

        public static BetCheck Ok => new BetCheck(BetError.None);
    }
}
