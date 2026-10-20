using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>5枚のカードからポーカーの役を判定する。</summary>
    public static class PokerEvaluator
    {
        public const int HandSize = 5;

        public static PokerHand Evaluate(IReadOnlyList<Card> cards)
        {
            if (cards == null || cards.Count != HandSize)
                throw new ArgumentException("ポーカーの役判定には5枚が必要です。", nameof(cards));

            var rankCounts = new int[(int)Rank.King + 1];
            bool flush = true;
            foreach (var card in cards)
            {
                rankCounts[(int)card.Rank]++;
                flush &= card.Suit == cards[0].Suit;
            }

            int pairs = 0, trips = 0, quads = 0, fives = 0, distinct = 0;
            foreach (int count in rankCounts)
            {
                if (count == 0)
                    continue;
                distinct++;
                if (count == 2) pairs++;
                else if (count == 3) trips++;
                else if (count == 4) quads++;
                else if (count >= 5) fives++;
            }

            if (fives > 0)
                return PokerHand.FiveOfAKind;

            bool royal = distinct == HandSize && IsRoyal(rankCounts);
            bool straight = royal || (distinct == HandSize && IsRun(rankCounts));
            if (straight && flush)
                return royal ? PokerHand.RoyalFlush : PokerHand.StraightFlush;
            if (quads > 0)
                return PokerHand.FourOfAKind;
            if (trips > 0 && pairs > 0)
                return PokerHand.FullHouse;
            if (flush)
                return PokerHand.Flush;
            if (straight)
                return PokerHand.Straight;
            if (trips > 0)
                return PokerHand.ThreeOfAKind;
            if (pairs >= 2)
                return PokerHand.TwoPair;
            return pairs == 1 ? PokerHand.OnePair : PokerHand.Nothing;
        }

        /// <summary>10-J-Q-K-A（エースハイのストレート）。</summary>
        static bool IsRoyal(int[] counts)
            => counts[(int)Rank.Ace] == 1 && counts[(int)Rank.Ten] == 1 && counts[(int)Rank.Jack] == 1
               && counts[(int)Rank.Queen] == 1 && counts[(int)Rank.King] == 1;

        /// <summary>連続する5ランク（A-2-3-4-5 を含む）。</summary>
        static bool IsRun(int[] counts)
        {
            int low = (int)Rank.Ace;
            while (counts[low] == 0)
                low++;
            if (low + HandSize - 1 > (int)Rank.King)
                return false;
            for (int r = low; r < low + HandSize; r++)
            {
                if (counts[r] != 1)
                    return false;
            }
            return true;
        }
    }
}
