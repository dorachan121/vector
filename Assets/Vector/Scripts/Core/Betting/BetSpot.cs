namespace Vector.Core
{
    /// <summary>テーブル上のベットエリア。</summary>
    public enum BetSpot : byte
    {
        /// <summary>プレイヤーモードのメインベット。</summary>
        Main,

        /// <summary>ボーナス（5枚ポーカー役）のサイドベット。両モード共通。</summary>
        Bonus,

        /// <summary>観戦モード: プレイヤーの勝ちに賭ける。</summary>
        Player,

        /// <summary>観戦モード: ディーラーの勝ちに賭ける。</summary>
        Dealer,

        /// <summary>観戦モード: 引き分けに賭ける。</summary>
        Tie,
    }
}
