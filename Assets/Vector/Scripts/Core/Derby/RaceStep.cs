namespace Vector.Core.Derby
{
    /// <summary>レースの1手（引いたカードと、その結果）。</summary>
    public readonly struct RaceStep
    {
        public readonly Card Card;

        /// <summary>カードのスートの走者。</summary>
        public readonly int Runner;

        /// <summary>走者が進んだ（ゴール済みの走者のカードなら false）。</summary>
        public readonly bool Moved;

        /// <summary>この手の後の走者の位置。</summary>
        public readonly int Position;

        /// <summary>この手でゴールしたときの着順（1 か 2）。ゴールしていなければ 0。</summary>
        public readonly int FinishedPlace;

        public RaceStep(Card card, int runner, bool moved, int position, int finishedPlace)
        {
            Card = card;
            Runner = runner;
            Moved = moved;
            Position = position;
            FinishedPlace = finishedPlace;
        }
    }
}
