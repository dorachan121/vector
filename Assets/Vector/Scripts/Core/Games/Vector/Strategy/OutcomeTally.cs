namespace Vector.Core
{
    /// <summary>勝敗の集計。</summary>
    public struct OutcomeTally
    {
        public int Wins;
        public int Losses;
        public int Ties;

        public int Total => Wins + Losses + Ties;
        public double WinRate => Total == 0 ? 0 : (double)Wins / Total;
        public double LossRate => Total == 0 ? 0 : (double)Losses / Total;
        public double TieRate => Total == 0 ? 0 : (double)Ties / Total;

        /// <summary>Playout.Run / Scoring.Compare の結果（+1 / 0 / -1）を加える。</summary>
        public void Add(int sign)
        {
            if (sign > 0)
                Wins++;
            else if (sign < 0)
                Losses++;
            else
                Ties++;
        }

        /// <summary>勝ちが payout:1、負けが -1、引き分けが 0 のときの1単位あたり期待値。</summary>
        public double ExpectedValue(double payout) => WinRate * payout - LossRate;
    }
}
