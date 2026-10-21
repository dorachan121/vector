using System;

namespace Vector.Core
{
    /// <summary>ラウンドの決着から各ベットの払い戻しを計算する。</summary>
    public static class Settlement
    {
        public static SpotResult Settle(TableRules rules, BetSpot spot, long stake, RoundWinner winner, PokerHand bonusHand)
        {
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));
            if (stake <= 0)
                throw new ArgumentOutOfRangeException(nameof(stake));

            switch (spot)
            {
                case BetSpot.Main:
                    return Resolve(spot, stake, winner == RoundWinner.Player, winner == RoundWinner.Tie, rules.MainPays);
                case BetSpot.Player:
                    return Resolve(spot, stake, winner == RoundWinner.Player, winner == RoundWinner.Tie, rules.BackPlayerPays);
                case BetSpot.Dealer:
                    return Resolve(spot, stake, winner == RoundWinner.Dealer, winner == RoundWinner.Tie, rules.BackDealerPays);
                case BetSpot.Tie:
                    return Resolve(spot, stake, winner == RoundWinner.Tie, false, rules.BackTiePays);
                case BetSpot.Bonus:
                    bool won = rules.BonusPays.TryGetValue(bonusHand, out var odds);
                    return Resolve(spot, stake, won, false, odds);
                default:
                    throw new ArgumentOutOfRangeException(nameof(spot));
            }
        }

        static SpotResult Resolve(BetSpot spot, long stake, bool won, bool push, Odds odds)
        {
            if (won)
                return SpotResult.Win((int)spot, stake, odds);
            return push ? SpotResult.Push((int)spot, stake) : SpotResult.Lose((int)spot, stake);
        }
    }
}
