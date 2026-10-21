using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>CROWN DICE の1ラウンドの最終結果。</summary>
    public sealed class CrownResult : IRoundOutcome
    {
        public CrownResult(int crown, IReadOnlyList<int> playerDice, IReadOnlyList<int> dealerDice, DiceRank playerRank, DiceRank dealerRank,
            bool crownYachtBonus, OpeningHand opening, IReadOnlyList<SpotResult> bets)
        {
            Crown = crown;
            PlayerDice = playerDice;
            DealerDice = dealerDice;
            PlayerRank = playerRank;
            DealerRank = dealerRank;
            CrownYachtBonus = crownYachtBonus;
            Opening = opening;
            Bets = bets;
            Winner = playerRank > dealerRank ? RoundWinner.Player : playerRank < dealerRank ? RoundWinner.Dealer : RoundWinner.Tie;
        }

        public int Crown { get; }
        public IReadOnlyList<int> PlayerDice { get; }
        public IReadOnlyList<int> DealerDice { get; }
        public DiceRank PlayerRank { get; }
        public DiceRank DealerRank { get; }
        public RoundWinner Winner { get; }

        /// <summary>王冠ヨットのアンティボーナスが付いた。</summary>
        public bool CrownYachtBonus { get; }

        public OpeningHand Opening { get; }
        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;
    }
}
