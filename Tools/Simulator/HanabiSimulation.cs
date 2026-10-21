using System;
using Vector.Core;
using Vector.Core.Hanabi;

namespace Vector.Simulator
{
    /// <summary>
    /// HANABI の検証。開く倍率ごとの厳密な確率と RTP を並べ、
    /// 実際の HanabiEngine で「毎回でたらめな倍率で開く」打ち上げを繰り返しても RTP が 97% を超えないことを確かめる。
    /// </summary>
    static class HanabiSimulation
    {
        static readonly int[] Targets = { 101, 150, 200, 300, 500, 1_000, 2_000, 5_000, 10_000, 100_000 };

        public static void Run(HanabiRules rules, int launches, ulong seed)
        {
            Console.WriteLine($"HANABI (crash = floor({HanabiRules.ReturnPercent} x {HanabiRules.Resolution:N0} / (r + 1)) / 100, cap x{rules.MaxHundredths / 100})");
            Console.WriteLine($"  burst before x1.01 = {rules.InstantBurst:P3}");
            foreach (int m in Targets)
                Console.WriteLine($"  cash out at x{m / 100.0,-8:0.00} p = {rules.ProbabilityAtLeast(m),9:P4}  RTP {rules.ReturnAt(m):P4}");
            if (launches <= 0)
                return;

            var random = new SeededRandom(seed);
            var engine = new HanabiEngine(rules, random);
            long stake = rules.MinBet * 100;
            double sum = 0, squares = 0;
            for (int i = 0; i < launches; i++)
            {
                engine.BeginBetting();
                var bets = new BetBoard();
                bets.Add(HanabiRules.Main, stake);
                engine.Launch(bets);
                int target = Targets[random.Next(Targets.Length)];
                if (engine.HasBurstAt(target))
                    engine.Burst();
                else
                    engine.CashOut(target);
                double ret = engine.Settle().TotalReturn / (double)stake;
                sum += ret;
                squares += ret * ret;
            }
            double mean = sum / launches;
            double sd = Math.Sqrt(Math.Max(0, squares / launches - mean * mean));
            Console.WriteLine($"  simulation (random target, {launches:N0} launches): RTP = {mean:P3} ±{1.96 * sd / Math.Sqrt(launches):P3}");
        }
    }
}
