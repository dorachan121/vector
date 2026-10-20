namespace Vector.Core
{
    /// <summary>ベット検証の結果。</summary>
    public enum BetError : byte
    {
        None,

        /// <summary>このモードでは使えないベットエリア。</summary>
        SpotUnavailable,

        /// <summary>テーブルミニマム未満。</summary>
        BelowMinimum,

        /// <summary>テーブルマキシマム超過。</summary>
        AboveMaximum,

        /// <summary>サイドベットだけで、メインのベットがない。</summary>
        NoPrimaryBet,
    }
}
