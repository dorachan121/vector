namespace Vector.Core
{
    /// <summary>チップを置けなかった理由。</summary>
    public enum ChipError : byte
    {
        None,

        /// <summary>ベット受付時間外。</summary>
        NotBetting,

        /// <summary>このモードでは使えないベットエリア。</summary>
        SpotUnavailable,

        /// <summary>手持ちが足りない。</summary>
        InsufficientFunds,

        /// <summary>テーブルマキシマムを超える。</summary>
        AboveMaximum,
    }
}
