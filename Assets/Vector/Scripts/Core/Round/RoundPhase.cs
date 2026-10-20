namespace Vector.Core
{
    public enum RoundPhase : byte
    {
        /// <summary>ベット受付中（ラウンド開始前）。</summary>
        Betting,

        /// <summary>手札が配られ、プレイヤーがラインの種類を選ぶ。</summary>
        ChoosingFamily,

        /// <summary>プレイヤーが手札を1枚置く番。</summary>
        PlayerTurn,

        /// <summary>ディーラーがシューから1枚引いて置く番。</summary>
        DealerTurn,

        /// <summary>盤面が埋まり、勝敗判定待ち。</summary>
        Showdown,

        /// <summary>精算済み。</summary>
        Complete,
    }
}
