namespace Vector.Core.Rpg
{
    /// <summary>デイリーミッションの条件の種類。</summary>
    public enum DailyMissionKind : byte
    {
        /// <summary>どのテーブルでも Target ラウンド遊ぶ。</summary>
        PlayRounds,

        /// <summary>どのテーブルでも Target ラウンド勝つ。</summary>
        WinRounds,

        /// <summary>合計 Target まで賭ける。</summary>
        Wager,

        /// <summary>Game のテーブルで Target ラウンド遊ぶ。</summary>
        PlayGame,

        /// <summary>Target 種類のゲームを遊ぶ。</summary>
        DistinctGames,
    }
}
