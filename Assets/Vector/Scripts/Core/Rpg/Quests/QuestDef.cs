namespace Vector.Core.Rpg
{
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
