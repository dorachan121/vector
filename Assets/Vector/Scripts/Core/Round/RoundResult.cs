using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>ラウンドの最終結果。</summary>
    public sealed class RoundResult
    {
        public RoundResult(LineFamily playerFamily, LineFamily dealerFamily, int playerScore, int dealerScore,
            IReadOnlyList<Line> playerLines, IReadOnlyList<Line> dealerLines, PokerHand bonusHand, IReadOnlyList<BetResult> bets)
        {
            PlayerFamily = playerFamily;
            DealerFamily = dealerFamily;
            PlayerScore = playerScore;
            DealerScore = dealerScore;
            PlayerLines = playerLines;
            DealerLines = dealerLines;
            BonusHand = bonusHand;
            Bets = bets;
            Winner = Scoring.WinnerOf(playerScore, dealerScore);
        }

        public LineFamily PlayerFamily { get; }
        public LineFamily DealerFamily { get; }
        public int PlayerScore { get; }
        public int DealerScore { get; }
        public RoundWinner Winner { get; }
        public IReadOnlyList<Line> PlayerLines { get; }
        public IReadOnlyList<Line> DealerLines { get; }
        public PokerHand BonusHand { get; }
        public IReadOnlyList<BetResult> Bets { get; }

        public long TotalStake
        {
            get
            {
                long sum = 0;
                foreach (var b in Bets)
                    sum += b.Stake;
                return sum;
            }
        }

        public long TotalReturn
        {
            get
            {
                long sum = 0;
                foreach (var b in Bets)
                    sum += b.Return;
                return sum;
            }
        }

        public long Net => TotalReturn - TotalStake;
    }
}
