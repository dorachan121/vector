using System;
using System.Collections.Generic;
using System.Linq;
using Vector.Core;

namespace Vector.Simulator
{
    /// <summary>
    /// ボーナスベットの厳密な RTP。シューから引く5枚の組み合わせを「カードの種類の多重集合」として
    /// 全列挙し、各多重集合の出現数（各カードの組み合わせ数の積）で重み付けする。
    /// </summary>
    static class BonusCalculator
    {
        public static void Run(TableRules rules)
        {
            int decks = rules.DeckCount;
            var weights = new Dictionary<PokerHand, double>();
            foreach (PokerHand h in Enum.GetValues(typeof(PokerHand)))
                weights[h] = 0;

            var combinations = new double[decks + 1];
            for (int k = 0; k <= decks; k++)
                combinations[k] = Choose(decks, k);

            var ids = new int[5];
            var cards = new Card[5];
            double total = 0;
            for (ids[0] = 0; ids[0] < Card.DistinctCount; ids[0]++)
            for (ids[1] = ids[0]; ids[1] < Card.DistinctCount; ids[1]++)
            for (ids[2] = ids[1]; ids[2] < Card.DistinctCount; ids[2]++)
            for (ids[3] = ids[2]; ids[3] < Card.DistinctCount; ids[3]++)
            for (ids[4] = ids[3]; ids[4] < Card.DistinctCount; ids[4]++)
            {
                double weight = Weight(ids, combinations, decks);
                if (weight == 0)
                    continue;
                for (int i = 0; i < 5; i++)
                    cards[i] = Card.FromId(ids[i]);
                weights[PokerEvaluator.Evaluate(cards)] += weight;
                total += weight;
            }

            Console.WriteLine($"BONUS ({decks} decks, {total:E4} hands)");
            double rtp = 0;
            foreach (var pair in weights.OrderByDescending(p => p.Key))
            {
                double p = pair.Value / total;
                string pays = rules.BonusPays.TryGetValue(pair.Key, out var odds) ? odds.ToString() : "-";
                if (odds.Per > 0)
                    rtp += p * (1 + odds.Ratio);
                Console.WriteLine($"  {pair.Key,-14} p={p:E4}  1 in {1 / p,10:N1}  pays {pays}");
            }
            Console.WriteLine($"  RTP = {rtp:P3}  (house edge {1 - rtp:P3})");
        }

        static double Weight(int[] ids, double[] combinations, int decks)
        {
            double weight = 1;
            int run = 1;
            for (int i = 1; i <= ids.Length; i++)
            {
                if (i < ids.Length && ids[i] == ids[i - 1])
                {
                    run++;
                    continue;
                }
                if (run > decks)
                    return 0;
                weight *= combinations[run];
                run = 1;
            }
            return weight;
        }

        static double Choose(int n, int k)
        {
            double r = 1;
            for (int i = 1; i <= k; i++)
                r = r * (n - k + i) / i;
            return r;
        }
    }
}
