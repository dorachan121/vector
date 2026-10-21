using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Vector.Core;
using Vector.Core.Colosseum;

namespace Vector.Simulator
{
    /// <summary>
    /// COLOSSEUM の検証。最善戦略の勝率（＝RTP）を厳密に求め、実際の BattleEngine で対戦を繰り返して食い違わないことを確かめる。
    /// </summary>
    static class ColosseumSimulation
    {
        public static void Run(BattleRules rules, int rounds, PlayerStrategy strategy, ulong seed)
        {
            var watch = Stopwatch.StartNew();
            var solver = BattleSolver.Solve(rules.Setup);
            double exact = solver.WinProbability * (1 + rules.WinPays.Ratio);
            Console.WriteLine($"COLOSSEUM ({rules.Setup}, critical K x2, win pays {rules.WinPays})");
            Console.WriteLine($"  optimal win rate = {solver.WinProbability:P4}  RTP (exact) = {exact:P4}  (house edge {1 - exact:P4})  [{watch.ElapsedMilliseconds} ms]");
            if (rounds <= 0)
                return;

            watch.Restart();
            int workers = Math.Max(1, Environment.ProcessorCount);
            var gate = new object();
            long wins = 0, losses = 0, draws = 0, turns = 0, guards = 0, actions = 0, done = 0;
            double sum = 0;
            Parallel.For(0, workers, worker =>
            {
                int share = rounds / workers + (worker < rounds % workers ? 1 : 0);
                var random = new SeededRandom(seed + (ulong)worker * 7919UL);
                var engine = new BattleEngine(rules, random);
                var bets = new BetBoard();
                bets.Add(BattleRules.Main, rules.MinBet);
                long w = 0, l = 0, d = 0, t = 0, g = 0, a = 0;
                double s = 0;
                for (int r = 0; r < share; r++)
                {
                    engine.BeginBetting();
                    engine.Deal(bets);
                    while (engine.Phase == BattlePhase.PlayerTurn)
                    {
                        var action = Choose(strategy, solver, engine, random);
                        if (action == BattleAction.Guard)
                            g++;
                        a++;
                        engine.Act(action);
                    }
                    var result = engine.Settle();
                    s += result.TotalReturn / (double)rules.MinBet;
                    t += result.Turns;
                    if (result.Winner == RoundWinner.Player) w++;
                    else if (result.Winner == RoundWinner.Dealer) l++;
                    else d++;
                }
                lock (gate)
                {
                    wins += w;
                    losses += l;
                    draws += d;
                    turns += t;
                    guards += g;
                    actions += a;
                    sum += s;
                    done += share;
                }
            });
            double p = wins / (double)done;
            double rtp = sum / done;
            double se = Math.Sqrt(p * (1 - p) / done) * (1 + rules.WinPays.Ratio);
            Console.WriteLine($"  simulation ({strategy}, {done:N0} battles): RTP = {rtp:P3} ±{1.96 * se:P3}  win {p:P2}  draw {draws}  " +
                              $"turns {turns / (double)done:F2}  guard {guards / (double)actions:P1}  [{watch.ElapsedMilliseconds} ms]");
        }

        static BattleAction Choose(PlayerStrategy strategy, BattleSolver solver, BattleEngine engine, IRandomSource random)
        {
            switch (strategy)
            {
                case PlayerStrategy.Advisor:
                    return solver.Best(engine.PlayerHp, engine.BeastHp, engine.Intent.Value);
                case PlayerStrategy.Greedy:
                    // カジュアルな打ち方: 予告が大きい（9以上）ときだけ受ける
                    return engine.IntentDamage >= 9 ? BattleAction.Guard : BattleAction.Strike;
                default:
                    return random.Next(2) == 0 ? BattleAction.Strike : BattleAction.Guard;
            }
        }
    }
}
