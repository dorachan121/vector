using System;
using Vector.Core;
using Vector.Core.Harpoon;

namespace Vector.Simulator
{
    /// <summary>
    /// HARPOON の検証。獲物ごとの厳密な捕獲率・倍率・RTP を並べ、
    /// 実際の HarpoonEngine で「毎回でたらめな獲物を狙う」発射を繰り返しても RTP が変わらないことを確かめる。
    /// </summary>
    static class HarpoonSimulation
    {
        static readonly string[] Names = { "sardine", "clownfish", "puffer", "turtle", "swordfish", "shark", "whale", "leviathan", "golden jelly" };

        public static void Run(HarpoonRules rules, int shots, ulong seed)
        {
            Console.WriteLine($"HARPOON (return {HarpoonRules.ReturnPercent}% on every quarry)");
            for (int q = 0; q < Quarry.Count; q++)
            {
                var (hits, outOf) = rules.CatchOdds(q);
                string multiple = rules.IsMystery(q) ? $"x{rules.MinMultiple(q)}-{rules.MaxMultiple(q)} (avg {rules.ExpectedMultiple(q):0.##})" : $"x{rules.MinMultiple(q)}";
                Console.WriteLine($"  {Names[q],-13} catch {hits,5}/{outOf,-7} = {rules.CatchProbability(q),8:P3}  {multiple,-24} RTP {rules.ReturnOf(q):P4}");
            }
            if (shots <= 0)
                return;

            var random = new SeededRandom(seed);
            var engine = new HarpoonEngine(rules, random);
            long stake = rules.BetLevels[0];
            double sum = 0, squares = 0;
            for (int s = 0; s < shots; s++)
            {
                var bets = new BetBoard();
                bets.Add(random.Next(Quarry.Count), stake);
                double ret = engine.Fire(bets).TotalReturn / (double)stake;
                sum += ret;
                squares += ret * ret;
            }
            double mean = sum / shots;
            double sd = Math.Sqrt(Math.Max(0, squares / shots - mean * mean));
            Console.WriteLine($"  simulation (random quarry, {shots:N0} shots): RTP = {mean:P3} ±{1.96 * sd / Math.Sqrt(shots):P3}");
        }
    }
}
