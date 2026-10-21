using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Vector.Core;
using Vector.Core.Crown;

namespace Vector.Simulator
{
    /// <summary>
    /// CROWN DICE の検証。メインは最善戦略の厳密な RTP、オープニングベットは最初の5個の全列挙で求め、
    /// 実際の CrownEngine でラウンドを繰り返して厳密値と食い違わないことも確かめる。
    /// </summary>
    static class CrownSimulation
    {
        public static void Run(CrownRules rules, int rounds, PlayerStrategy strategy, ulong seed)
        {
            var watch = Stopwatch.StartNew();
            var advisor = new CrownAdvisor(rules);
            Console.WriteLine("CROWN DICE (player rolls first; dealer follows the house way)");
            for (int crown = 1; crown <= DiceRank.Faces; crown++)
                Console.WriteLine($"  crown {crown}: optimal RTP = {advisor.For(crown).ExpectedReturn:P4}");
            double exact = advisor.ExpectedReturn();
            Console.WriteLine($"  ANTE optimal RTP (exact) = {exact:P4}  (house edge {1 - exact:P4})  [{watch.ElapsedMilliseconds} ms]");
            OpeningExact(rules);

            if (rounds <= 0)
                return;
            watch.Restart();
            int workers = Math.Max(1, Environment.ProcessorCount);
            var gate = new object();
            double sum = 0, squares = 0;
            int wins = 0, ties = 0, losses = 0, bonuses = 0, done = 0;
            var kinds = new Dictionary<DiceHandKind, int>();
            Parallel.For(0, workers, worker =>
            {
                int share = rounds / workers + (worker < rounds % workers ? 1 : 0);
                var random = new SeededRandom(seed + (ulong)worker * 7919UL);
                var engine = new CrownEngine(rules, random);
                var bets = new BetBoard();
                bets.Add(CrownRules.Ante, rules.MinBet);
                double s = 0, sq = 0;
                int w = 0, t = 0, l = 0, b = 0;
                var k = new Dictionary<DiceHandKind, int>();
                for (int r = 0; r < share; r++)
                {
                    engine.BeginBetting();
                    engine.Deal(bets);
                    var plan = advisor.For(engine.Crown);
                    while (engine.Phase == CrownPhase.PlayerTurn)
                    {
                        int hold = ChooseHold(strategy, plan, engine, random);
                        if (hold == (1 << DiceRank.DiceCount) - 1)
                            engine.Stand();
                        else
                            engine.Reroll(hold);
                    }
                    while (engine.Phase == CrownPhase.DealerTurn)
                        engine.PlayDealer();
                    var result = engine.Settle();
                    double ret = result.TotalReturn / (double)rules.MinBet;
                    s += ret;
                    sq += ret * ret;
                    if (result.Winner == RoundWinner.Player) w++;
                    else if (result.Winner == RoundWinner.Tie) t++;
                    else l++;
                    if (result.CrownYachtBonus) b++;
                    k.TryGetValue(result.PlayerRank.Kind, out int c);
                    k[result.PlayerRank.Kind] = c + 1;
                }
                lock (gate)
                {
                    sum += s;
                    squares += sq;
                    wins += w;
                    ties += t;
                    losses += l;
                    bonuses += b;
                    done += share;
                    foreach (var pair in k)
                    {
                        kinds.TryGetValue(pair.Key, out int c);
                        kinds[pair.Key] = c + pair.Value;
                    }
                }
            });
            double mean = sum / done;
            double sd = Math.Sqrt(Math.Max(0, squares / done - mean * mean));
            Console.WriteLine($"  simulation ({strategy}, {done:N0} rounds): RTP = {mean:P3} ±{1.96 * sd / Math.Sqrt(done):P3}  " +
                              $"win {wins / (double)done:P2} / tie {ties / (double)done:P2} / lose {losses / (double)done:P2}  " +
                              $"crown yacht 1 in {done / Math.Max(1.0, bonuses):N0}  [{watch.ElapsedMilliseconds} ms]");
            foreach (var pair in kinds.OrderByDescending(p => p.Key))
                Console.WriteLine($"    final {pair.Key,-13} {pair.Value / (double)done:P2}");
        }

        static int ChooseHold(PlayerStrategy strategy, CrownStrategy plan, CrownEngine engine, IRandomSource random)
        {
            switch (strategy)
            {
                case PlayerStrategy.Advisor:
                    return plan.Advise(engine.PlayerDice, engine.PlayerRerollsLeft).HoldMask;
                case PlayerStrategy.Greedy:
                    return KeepLargestGroup(engine.PlayerDice, engine.Crown);
                default:
                    return random.Next(1 << DiceRank.DiceCount);
            }
        }

        /// <summary>カジュアルな打ち方: 一番多い目（同数なら強い目）だけを残す。ストレートとフルハウス以上は止める。</summary>
        static int KeepLargestGroup(IReadOnlyList<int> dice, int crown)
        {
            var rank = DiceRank.Of(dice, crown);
            if (rank.Kind >= DiceHandKind.Straight)
                return (1 << dice.Count) - 1;
            var counts = DiceTables.CountsOf(dice);
            int best = 0;
            for (int f = 1; f <= DiceRank.Faces; f++)
            {
                if (best == 0 || counts[f - 1] > counts[best - 1] ||
                    (counts[f - 1] == counts[best - 1] && DiceRank.Strength(f, crown) > DiceRank.Strength(best, crown)))
                    best = f;
            }
            int mask = 0;
            for (int i = 0; i < dice.Count; i++)
            {
                if (dice[i] == best)
                    mask |= 1 << i;
            }
            return mask;
        }

        /// <summary>オープニングベットの厳密な RTP（王冠の目 6 通り × 最初の5個 7,776 通り）。</summary>
        static void OpeningExact(CrownRules rules)
        {
            var counts = new Dictionary<OpeningHand, long>();
            long total = 0;
            var dice = new int[DiceRank.DiceCount];
            for (int crown = 1; crown <= DiceRank.Faces; crown++)
            {
                for (int code = 0; code < 7776; code++)
                {
                    int x = code;
                    for (int i = 0; i < dice.Length; i++)
                    {
                        dice[i] = x % 6 + 1;
                        x /= 6;
                    }
                    var hand = OpeningHands.Of(DiceRank.Of(dice, crown), crown);
                    counts.TryGetValue(hand, out long c);
                    counts[hand] = c + 1;
                    total++;
                }
            }
            double rtp = 0;
            Console.WriteLine("  OPENING (first five dice):");
            foreach (var pair in counts.OrderByDescending(p => p.Key))
            {
                double p = pair.Value / (double)total;
                string pays = rules.OpeningPays.TryGetValue(pair.Key, out var odds) ? odds.ToString() : "-";
                if (odds.Per > 0)
                    rtp += p * (1 + odds.Ratio);
                Console.WriteLine($"    {pair.Key,-12} p={p:E4}  1 in {1 / p,8:N1}  pays {pays}");
            }
            Console.WriteLine($"    RTP = {rtp:P3}  (house edge {1 - rtp:P3})");
        }
    }
}
