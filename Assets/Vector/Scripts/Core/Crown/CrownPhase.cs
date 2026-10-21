namespace Vector.Core.Crown
{
    public enum CrownPhase : byte
    {
        /// <summary>ベット受付中（ラウンド開始前）。</summary>
        Betting,

        /// <summary>プレイヤーが振り直すか止めるかを選ぶ番。</summary>
        PlayerTurn,

        /// <summary>ディーラーがハウスウェイで振る番。</summary>
        DealerTurn,

        /// <summary>両者の手が確定し、勝敗判定待ち。</summary>
        Showdown,

        /// <summary>精算済み。</summary>
        Complete,
    }
}
