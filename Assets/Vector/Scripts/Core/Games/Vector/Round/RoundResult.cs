using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>ラウンドの最終結果。</summary>
    public sealed class RoundResult : IRoundOutcome
    {
        public RoundResult(LineFamily playerFamily, LineFamily dealerFamily, int playerScore, int dealerScore,
            IReadOnlyList<Line> playerLines, IReadOnlyList<Line> dealerLines, PokerHand bonusHand, IReadOnlyList<SpotResult> bets)
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
        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);

        public long TotalReturn => SpotResult.TotalReturn(Bets);

        public long Net => TotalReturn - TotalStake;
    }
}
