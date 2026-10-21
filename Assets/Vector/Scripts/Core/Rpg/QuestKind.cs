namespace Vector.Core.Rpg
{
    /// <summary>クエストの条件の種類。</summary>
    public enum QuestKind : byte
    {
        /// <summary>Game のテーブルで Target ラウンド遊ぶ。</summary>
        PlayRounds,

        /// <summary>Game のテーブルで Target ラウンド勝つ（Game が null ならどのゲームでも）。</summary>
        WinRounds,

        /// <summary>Tag の出来事を Target 回起こす。</summary>
        Event,

        /// <summary>レベル Target に到達する。</summary>
        ReachLevel,
    }
}
