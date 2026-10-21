using System;
using Vector.Core;
using Vector.Core.Orbit;

namespace Vector.Simulator
{
    /// <summary>
    /// ORBIT の検証。賭け先ごとの厳密な確率・配当・RTP を並べ、
    /// 実際の OrbitEngine で「毎回でたらめな賭け先に賭ける」スピンを繰り返しても RTP が変わらないことを確かめる。
    /// </summary>
    static class OrbitSimulation
    {
        public static void Run(OrbitRules rules, int spins, ulong seed)
        {
            Console.WriteLine($"ORBIT (comet 1 in {OrbitRules.CometOneIn})");
            for (int spot = 0; spot < OrbitSpots.Count; spot++)
            {
                Console.WriteLine($"  {Name(spot),-18} p = {rules.ProbabilityOf(spot),9:P4}  pays {rules.PaysFor(spot),6}  " +
                                  $"max ${rules.MaxFor(spot),6:N0}  RTP {rules.ReturnOf(spot):P4}");
            }
            if (spins <= 0)
                return;

            var random = new SeededRandom(seed);
            var engine = new OrbitEngine(rules, random);
            double sum = 0, squares = 0;
            for (int s = 0; s < spins; s++)
            {
                engine.BeginBetting();
                var bets = new BetBoard();
                bets.Add(random.Next(OrbitSpots.Count), rules.MinBet);
                engine.Spin(bets);
                double ret = engine.Settle().TotalReturn / (double)rules.MinBet;
                sum += ret;
                squares += ret * ret;
            }
            double mean = sum / spins;
            double sd = Math.Sqrt(Math.Max(0, squares / spins - mean * mean));
            Console.WriteLine($"  simulation (random spot, {spins:N0} spins): RTP = {mean:P3} ±{1.96 * sd / Math.Sqrt(spins):P3}");
        }

        static string Name(int spot)
        {
            if (OrbitSpots.IsInner(spot))
                return $"inner {OrbitSpots.SlotOf(spot) + 1}";
            if (OrbitSpots.IsQuadrant(spot))
                return $"quadrant {OrbitSpots.QuadrantOf(spot) + 1}";
            if (OrbitSpots.IsHalf(spot))
                return OrbitSpots.HalfOf(spot) == 0 ? "heaven" : "earth";
            switch (spot)
            {
                case OrbitSpots.InnerConjunction: return "inner conjunction";
                case OrbitSpots.OuterConjunction: return "outer conjunction";
                case OrbitSpots.Alignment: return "alignment";
                default: return "comet";
            }
        }
    }
}
