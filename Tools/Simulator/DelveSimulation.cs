using System;
using Vector.Core;
using Vector.Core.Delve;

namespace Vector.Simulator
{
    /// <summary>
    /// DELVE の検証。深さごとに「そこで引き返す打ち方」の厳密な RTP を並べ、
    /// 実際の DelveEngine で「毎回でたらめな深さで引き返す」探索を繰り返しても RTP が変わらないことを確かめる。
    /// </summary>
    static class DelveSimulation
    {
        public static void Run(DelveRules rules, int rounds, ulong seed)
        {
            Console.WriteLine($"DELVE (target return {rules.TargetReturn:P1})");
            for (int d = 1; d <= rules.Depths; d++)
            {
                var floor = rules.Floors[d - 1];
                Console.WriteLine($"  depth {d,2}: {floor.Doors} doors / {floor.Monsters} monster(s)  survive {rules.SurvivalTo(d),9:P4}  " +
                                  $"pays x{rules.MultiplierHundredths(d) / 100.0,8:F2}  RTP if cashing here {rules.ReturnIfCashingAt(d):P3}");
            }
            if (rounds <= 0)
                return;

            var random = new SeededRandom(seed);
            var engine = new DelveEngine(rules, random);
            var bets = new BetBoard();
            bets.Add(DelveRules.Main, 100);
            double sum = 0, squares = 0;
            for (int r = 0; r < rounds; r++)
            {
                engine.BeginBetting();
                engine.Deal(bets);
                int target = 1 + random.Next(rules.Depths);
                while (engine.Phase == DelvePhase.Choosing)
                {
                    if (engine.ClearedDepth >= target)
                    {
                        engine.CashOut();
                        break;
                    }
                    engine.Open(random.Next(rules.Floors[engine.ClearedDepth].Doors));
                }
                double ret = engine.Settle().TotalReturn / 100.0;
                sum += ret;
                squares += ret * ret;
            }
            double mean = sum / rounds;
            double sd = Math.Sqrt(Math.Max(0, squares / rounds - mean * mean));
            Console.WriteLine($"  simulation (random cash-out depth, {rounds:N0} dives): RTP = {mean:P3} ±{1.96 * sd / Math.Sqrt(rounds):P3}");
        }
    }
}
