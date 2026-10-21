namespace Vector.Core.Rpg
{
    /// <summary>アイテム1種類の定義（交換に必要なコンプと、使ったときのチップ）。名前と説明は表示側（Runtime）が持つ。</summary>
    public sealed class ItemDef
    {
        public string Id;

        /// <summary>コンプでの交換に必要なポイント（0 なら交換できない）。</summary>
        public long CompPrice;

        /// <summary>使うと受け取れる遊戯用チップ（0 なら使えない）。</summary>
        public long ChipValue;
    }
}
