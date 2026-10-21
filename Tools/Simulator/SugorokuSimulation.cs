using System;
using System.Linq;
using Vector.Core;
using Vector.Core.Sugoroku;

namespace Vector.Simulator
{
    /// <summary>
    /// SUGOROKU の検証。最善手（後ろ向き帰納）と「いつも大きい目で進む」打ち方の厳密な RTP、
    /// 最善手で上がれる確率と上がったときの段の分布を出し、実際の SugorokuEngine で最善手どおりに打ったシミュレーションと突き合わせる。
    /// </summary>
    static class SugorokuSimulation
    {
        public static void Run(SugorokuRules rules, int games, ulong seed)
        {
            var solver = SugorokuSolver.Solve(rules);
            Console.WriteLine($"SUGOROKU ({rules.Goal} squares, {rules.Turns} rolls, pays x{string.Join(" / x", rules.PayHundredths.Select(p => (p / 100.0).ToString("0.##")))})");
            Console.WriteLine($"  RTP (optimal, exact)       = {solver.Return:P4}");
            Console.WriteLine($"  RTP (always larger die)    = {Greedy(rules):P4}");
            Console.WriteLine($"  RTP (myopic: coins first)  = {Myopic(rules):P4}");

            var (goal, tiers) = Outcomes(rules, solver);
            Console.WriteLine($"  reach the goal (optimal)   = {goal:P3}");
            for (int tier = 0; tier <= rules.MaxTier; tier++)
                Console.WriteLine($"    tier {tier}  x{rules.PayFor(tier) / 100.0,-6:0.##} {tiers[tier],9:P3}");
            if (games <= 0)
                return;

            var random = new SeededRandom(seed);
            var engine = new SugorokuEngine(rules, random);
            double sum = 0, squares = 0;
            for (int g = 0; g < games; g++)
            {
                engine.BeginBetting();
                var bets = new BetBoard();
                bets.Add(SugorokuRules.Main, rules.MinBet);
                engine.Start(bets);
                while (engine.Phase == SugorokuPhase.Choosing)
                    engine.Choose(solver.Best(engine.TurnsLeft, engine.Position, engine.Tier, engine.Dice.first, engine.Dice.second));
                double ret = engine.Settle().TotalReturn / (double)rules.MinBet;
                sum += ret;
                squares += ret * ret;
            }
            double mean = sum / games;
            double sd = Math.Sqrt(Math.Max(0, squares / games - mean * mean));
            Console.WriteLine($"  simulation (optimal, {games:N0} games): RTP = {mean:P3} ±{1.96 * sd / Math.Sqrt(games):P3}");
        }

        /// <summary>
        /// 目先のマスだけを見る打ち方の厳密な RTP（宝・小判に止まれるならそこへ、凶・落とし穴は避け、あとは大きい目）。
        /// 先を読まない人の目安。
        /// </summary>
        static double Myopic(SugorokuRules rules)
        {
            int Score(int position, int die)
            {
                if (position + die >= rules.Goal)
                    return 5;
                switch (rules.At(position + die))
                {
                    case Square.Treasure: return 4;
                    case Square.Coin: return 3;
                    case Square.Bridge: return 2;
                    case Square.Omen:
                    case Square.Pit: return 0;
                    default: return 1;
                }
            }
            return Evaluate(rules, (position, tier, a, b) =>
            {
                int sa = Score(position, a), sb = Score(position, b);
                return sa != sb ? (sa > sb ? a : b) : Math.Max(a, b);
            });
        }

        static double Greedy(SugorokuRules rules) => Evaluate(rules, (position, tier, a, b) => Math.Max(a, b));

        /// <summary>決まった打ち方（位置・段・2つの目 → 進む目）の厳密な RTP。</summary>
        static double Evaluate(SugorokuRules rules, Func<int, int, int, int, int> policy)
        {
            int goal = rules.Goal;
            var value = new double[rules.Turns + 1, goal + 1, rules.MaxTier + 1];
            for (int tier = 0; tier <= rules.MaxTier; tier++)
            {
                for (int t = 0; t <= rules.Turns; t++)
                    value[t, goal, tier] = rules.PayFor(tier) / 100.0;
            }
            for (int t = 1; t <= rules.Turns; t++)
            {
                for (int position = 0; position < goal; position++)
                {
                    for (int tier = 0; tier <= rules.MaxTier; tier++)
                    {
                        double sum = 0;
                        for (int a = 1; a <= 6; a++)
                        {
                            for (int b = 1; b <= 6; b++)
                            {
                                var (to, next) = rules.Land(position, tier, policy(position, tier, a, b));
                                sum += value[t - 1, to, next];
                            }
                        }
                        value[t, position, tier] = sum / 36.0;
                    }
                }
            }
            return value[rules.Turns, 0, rules.StartTier];
        }

        /// <summary>最善手で打ったときの、上がれる確率と上がったときの段の分布（前向きに確率を流す）。</summary>
        static (double goal, double[] tiers) Outcomes(SugorokuRules rules, SugorokuSolver solver)
        {
            int goal = rules.Goal;
            var mass = new double[goal + 1, rules.MaxTier + 1];
            mass[0, rules.StartTier] = 1;
            var tiers = new double[rules.MaxTier + 1];
            for (int left = rules.Turns; left >= 1; left--)
            {
                var next = new double[goal + 1, rules.MaxTier + 1];
                for (int position = 0; position < goal; position++)
                {
                    for (int tier = 0; tier <= rules.MaxTier; tier++)
                    {
                        double p = mass[position, tier];
                        if (p == 0)
                            continue;
                        for (int a = 1; a <= 6; a++)
                        {
                            for (int b = 1; b <= 6; b++)
                            {
                                int die = solver.Best(left, position, tier, a, b) == 0 ? a : b;
                                var (to, after) = rules.Land(position, tier, die);
                                next[to, after] += p / 36.0;
                            }
                        }
                    }
                }
                for (int tier = 0; tier <= rules.MaxTier; tier++)
                {
                    tiers[tier] += next[goal, tier];
                    next[goal, tier] = 0;
                }
                mass = next;
            }
            return (tiers.Sum(), tiers);
        }

        /// <summary>「盤面:段ごとの配当（100倍、カンマ区切り）:回数」。省いた項目は標準のまま。</summary>
        public static SugorokuRules Parse(string text)
        {
            var parts = text.Split(':');
            var standard = SugorokuRules.Standard;
            string layout = parts.Length > 0 && parts[0].Length > 0 ? parts[0] : SugorokuRules.StandardLayout;
            var pays = parts.Length > 1 && parts[1].Length > 0 ? parts[1].Split(',').Select(long.Parse).ToArray() : standard.PayHundredths.ToArray();
            int turns = parts.Length > 2 ? int.Parse(parts[2]) : standard.Turns;
            return new SugorokuRules(layout, pays, turns);
        }
    }
}
