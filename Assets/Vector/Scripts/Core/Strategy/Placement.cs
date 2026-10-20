namespace Vector.Core
{
    /// <summary>手札のどのカードをどのマスに置くか。</summary>
    public readonly struct Placement
    {
        public readonly int HandIndex;
        public readonly int Cell;

        public Placement(int handIndex, int cell)
        {
            HandIndex = handIndex;
            Cell = cell;
        }

        public override string ToString() => $"hand[{HandIndex}] -> cell {Cell}";
    }
}
