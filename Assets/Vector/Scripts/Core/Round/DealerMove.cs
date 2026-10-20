namespace Vector.Core
{
    /// <summary>ディーラーの1手。</summary>
    public readonly struct DealerMove
    {
        public readonly Card Card;
        public readonly int Cell;

        public DealerMove(Card card, int cell)
        {
            Card = card;
            Cell = cell;
        }
    }
}
