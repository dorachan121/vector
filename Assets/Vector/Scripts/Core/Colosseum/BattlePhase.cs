namespace Vector.Core.Colosseum
{
    public enum BattlePhase : byte
    {
        /// <summary>ベット受付中（ラウンド開始前）。</summary>
        Betting,

        /// <summary>獣の予告が出ていて、プレイヤーが手を選ぶ番。</summary>
        PlayerTurn,

        /// <summary>決着がつき、精算待ち。</summary>
        Showdown,

        /// <summary>精算済み。</summary>
        Complete,
    }
}
