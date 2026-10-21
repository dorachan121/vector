using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Vector.Core;

namespace Vector.Simulator
{
    /// <summary>
    /// 実際の RoundEngine とハウスウェイでラウンドを繰り返し、メインベットとバックベットの RTP を推定する。
    /// </summary>
    static class MainGameSimulation
    {
        sealed class Totals
        {
            public readonly Dictionary<LineFamily, OutcomeTally> ByFamily = new Dictionary<LineFamily, OutcomeTally>();
            public OutcomeTally All;
            public double MainReturn;
            public double MainReturnSquares;
            public int Rounds;

            public void Merge(Totals other)
            {
                foreach (var pair in other.ByFamily)
                {
                    ByFamily.TryGetValue(pair.Key, out var t);
                    t.Wins += pair.Value.Wins;
                    t.Losses += pair.Value.Losses;
                    t.Ties += pair.Value.Ties;
                    ByFamily[pair.Key] = t;
                }
                All.Wins += other.All.Wins;
                All.Losses += other.All.Losses;
                All.Ties += other.All.Ties;
                MainReturn += other.MainReturn;
                MainReturnSquares += other.MainReturnSquares;
                Rounds += other.Rounds;
            }
        }

        public static void Run(TableRules rules, int rounds, PlayerStrategy strategy, ulong seed)
        {
            var watch = Stopwatch.StartNew();
            int workers = Math.Max(1, Environment.ProcessorCount);
            var totals = new Totals();
            var gate = new object();
            Parallel.For(0, workers, worker =>
            {
                int share = rounds / workers + (worker < rounds % workers ? 1 : 0);
                var local = Simulate(rules, share, strategy, seed + (ulong)worker * 7919UL);
                lock (gate)
                    totals.Merge(local);
            });
            Report(rules, strategy, totals, watch.Elapsed);
        }

        static Totals Simulate(TableRules rules, int rounds, PlayerStrategy strategy, ulong seed)
        {
            var dealRandom = new SeededRandom(seed);
            var engine = new RoundEngine(rules, new Shoe(rules.DeckCount, dealRandom), new HouseWay(new SeededRandom(seed ^ 0xD1CEUL), rules.HouseWayPlayouts));
            var advisor = new PlayerAdvisor(new SeededRandom(seed ^ 0xAD7153UL), rules);
            var casualAdvisor = new PlayerAdvisor(new SeededRandom(seed ^ 0xCA5UL), rules, familyPlayouts: 24, movePlayouts: 0);
            var bets = new BetBoard();
            bets.Add((int)BetSpot.Main, rules.MinBet);
            var totals = new Totals();

            for (int r = 0; r < rounds; r++)
            {
                engine.BeginBetting();
                engine.Deal(GameMode.Player, bets);
                engine.ChooseFamily(ChooseFamily(engine, strategy, advisor, casualAdvisor, dealRandom));
                while (engine.Phase != RoundPhase.Showdown)
                {
                    if (engine.Phase == RoundPhase.DealerTurn)
                    {
                        engine.PlayDealerCard();
                        continue;
                    }
                    var p = ChoosePlacement(engine, strategy, advisor, dealRandom);
                    engine.PlacePlayerCard(p.HandIndex, p.Cell);
                }
                var result = engine.Settle();
                Record(totals, result, rules.MinBet);
            }
            return totals;
        }

        static LineFamily ChooseFamily(RoundEngine engine, PlayerStrategy strategy, PlayerAdvisor advisor, PlayerAdvisor casual, IRandomSource random)
        {
            switch (strategy)
            {
                case PlayerStrategy.Random:
                    return Lines.Families[random.Next(Lines.Families.Count)];
                case PlayerStrategy.Greedy:
                    return PlayerAdvisor.Best(casual.EstimateFamilies(engine.Board.ToValues(), engine.HandValues()));
                default:
                    return PlayerAdvisor.Best(advisor.EstimateFamilies(engine.Board.ToValues(), engine.HandValues()));
            }
        }

        static Placement ChoosePlacement(RoundEngine engine, PlayerStrategy strategy, PlayerAdvisor advisor, IRandomSource random)
        {
            var values = engine.Board.ToValues();
            var hand = engine.HandValues();
            switch (strategy)
            {
                case PlayerStrategy.Random:
                {
                    var empty = new List<int>();
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] == 0)
                            empty.Add(i);
                    }
                    return new Placement(random.Next(hand.Count), empty[random.Next(empty.Count)]);
                }
                case PlayerStrategy.Greedy:
                    return GreedyPolicy.ChoosePlacement(values, hand, engine.PlayerFamily);
                default:
                    return advisor.SuggestPlacement(values, hand, engine.PlayerFamily);
            }
        }

        static void Record(Totals totals, RoundResult result, long stake)
        {
            int sign = result.Winner == RoundWinner.Player ? 1 : result.Winner == RoundWinner.Dealer ? -1 : 0;
            totals.ByFamily.TryGetValue(result.PlayerFamily, out var t);
            t.Add(sign);
            totals.ByFamily[result.PlayerFamily] = t;
            totals.All.Add(sign);
            double ret = (double)result.TotalReturn / stake;
            totals.MainReturn += ret;
            totals.MainReturnSquares += ret * ret;
            totals.Rounds++;
        }

        static void Report(TableRules rules, PlayerStrategy strategy, Totals t, TimeSpan elapsed)
        {
            double n = t.Rounds;
            double mean = t.MainReturn / n;
            double sd = Math.Sqrt(Math.Max(0, t.MainReturnSquares / n - mean * mean));
            Console.WriteLine($"MAIN  strategy={strategy} rounds={t.Rounds} main pays {rules.MainPays}  ({elapsed.TotalSeconds:F1}s)");
            Console.WriteLine($"  win {t.All.WinRate:P2}  tie {t.All.TieRate:P2}  loss {t.All.LossRate:P2}");
            Console.WriteLine($"  RTP {mean:P2} ± {1.96 * sd / Math.Sqrt(n):P2} (95% CI)");
            foreach (var family in Lines.Families)
            {
                if (!t.ByFamily.TryGetValue(family, out var f) || f.Total == 0)
                    continue;
                Console.WriteLine($"  {family,-7} chosen {(double)f.Total / n,6:P1}  win {f.WinRate:P2}  tie {f.TieRate:P2}  EV {f.ExpectedValue(rules.MainPays.Ratio):+0.000;-0.000}");
            }

            double p = t.All.WinRate, d = t.All.LossRate, tie = t.All.TieRate;
            Console.WriteLine("BACK BETS (AI player = this strategy)");
            Console.WriteLine($"  PLAYER pays {rules.BackPlayerPays}  RTP {p * (1 + rules.BackPlayerPays.Ratio) + tie:P2}");
            Console.WriteLine($"  DEALER pays {rules.BackDealerPays}  RTP {d * (1 + rules.BackDealerPays.Ratio) + tie:P2}");
            Console.WriteLine($"  TIE    pays {rules.BackTiePays}  RTP {tie * (1 + rules.BackTiePays.Ratio):P2}");
        }
    }
}
