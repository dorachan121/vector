namespace Vector.Core.Sugoroku
{
    /// <summary>1回の移動の記録（画面はこれをなぞって駒を動かす）。</summary>
    public readonly struct SugorokuStep
    {
        /// <summary>選んだ目。</summary>
        public readonly int Die;
        public readonly int From;

        /// <summary>目の数だけ進んで止まったマス（上がりを越えたら上がりのマス）。</summary>
        public readonly int Landed;

        /// <summary>止まったマスの種類（上がりなら Plain）。</summary>
        public readonly Square Square;

        /// <summary>マスの効果のあとの位置。</summary>
        public readonly int To;
        public readonly int TierBefore;
        public readonly int TierAfter;

        public SugorokuStep(int die, int from, int landed, Square square, int to, int tierBefore, int tierAfter)
        {
            Die = die;
            From = from;
            Landed = landed;
            Square = square;
            To = to;
            TierBefore = tierBefore;
            TierAfter = tierAfter;
        }
    }
}
