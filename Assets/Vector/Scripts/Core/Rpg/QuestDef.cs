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

    /// <summary>クエストの定義（条件と報酬）。名前と台詞は表示側（Runtime）が持つ。</summary>
    public sealed class QuestDef
    {
        public string Id;

        /// <summary>依頼する人の id。</summary>
        public string Giver;

        public QuestKind Kind;
        public GameId? Game;
        public string Tag;
        public int Target = 1;

        public int Xp;
        public long Comps;
        public long Chips;
        public string Item;

        /// <summary>依頼を受けられるようになる条件の印（null なら最初から）。</summary>
        public string RequiresFlag;
    }
}
