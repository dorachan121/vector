namespace Vector.Core
{
    /// <summary>盤面のカードを誰が置いたか。</summary>
    public enum Seat : byte
    {
        None,

        /// <summary>ラウンド開始時に中央へ配られるベクターカード。</summary>
        House,

        Player,
        Dealer,
    }
}
