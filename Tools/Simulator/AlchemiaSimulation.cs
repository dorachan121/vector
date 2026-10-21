using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Vector.Core;
using Vector.Core.Alchemia;

namespace Vector.Simulator
{
    /// <summary>
    /// ALCHEMIA の検証。実際の AlchemiaEngine でスピンを繰り返し、RTP・当たりの頻度・ばらつき・大当たりの頻度を推定する。
    /// </summary>
    static class AlchemiaSimulation
    {
        static readonly double[] Buckets = { 0, 1, 2, 5, 10, 50, 200, 1000, 5000 };

        sealed class Totals
        {
            public double Sum, Squares, Max;
            public long Spins, Hits, Capped, Transmutations, Cascades;
            public readonly long[] Tiers = new long[Symbol.MaxTier + 2];
            public readonly long[] Histogram = new long[Buckets.Length];

            public void Merge(Totals o)
            {
                Sum += o.Sum;
                Squares += o.Squares;
                Max = Math.Max(Max, o.Max);
                Spins += o.Spins;
                Hits += o.Hits;
                Capped += o.Capped;
                Transmutations += o.Transmutations;
                Cascades += o.Cascades;
                for (int i = 0; i < Tiers.Length; i++)
                    Tiers[i] += o.Tiers[i];
                for (int i = 0; i < Histogram.Length; i++)
                    Histogram[i] += o.Histogram[i];
            }
        }

        public static void Run(AlchemiaRules rules, long spins, ulong seed)
        {
            var watch = Stopwatch.StartNew();
            int workers = Math.Max(1, Environment.ProcessorCount);
            var totals = new Totals();
            var gate = new object();
            Parallel.For(0, workers, worker =>
            {
                long share = spins / workers + (worker < spins % workers ? 1 : 0);
                var engine = new AlchemiaEngine(rules, new SeededRandom(seed + (ulong)worker * 7919UL));
                var bets = new BetBoard();
                bets.Add(AlchemiaRules.Spin, 100);
                var local = new Totals();
                for (long s = 0; s < share; s++)
                {
                    var result = engine.Spin(bets);
                    // 実際の払い戻し（賭け金 100 に対する倍率）で数える
                    double win = result.TotalReturn / 100.0;
                    local.Sum += win;
                    local.Squares += win * win;
                    local.Max = Math.Max(local.Max, win);
                    local.Spins++;
                    if (win > 0)
                        local.Hits++;
                    if (result.Capped)
                        local.Capped++;
                    local.Cascades += result.Steps.Count;
                    foreach (var step in result.Steps)
                    {
                        foreach (var m in step.Merges)
                        {
                            if (m.IsTransmutation)
                                local.Transmutations++;
                            else
                                local.Tiers[m.NewTier]++;
                        }
                    }
                    int b = 0;
                    while (b + 1 < Buckets.Length && win >= Buckets[b + 1])
                        b++;
                    local.Histogram[win > 0 ? b : 0]++;
                }
                lock (gate)
                    totals.Merge(local);
            });

            double n = totals.Spins;
            double mean = totals.Sum / n;
            double sd = Math.Sqrt(Math.Max(0, totals.Squares / n - mean * mean));
            Console.WriteLine($"ALCHEMIA ({rules.Columns}x{rules.Rows}, {rules.ElementCount} elements, cluster {rules.MinCluster}+, cap {rules.MaxWinMultiple:N0}x)");
            Console.WriteLine($"  {totals.Spins:N0} spins: RTP = {mean:P3} ±{1.96 * sd / Math.Sqrt(n):P3}  hit 1 in {n / totals.Hits:F2} ({totals.Hits / n:P2})  " +
                              $"sd {sd:F2}x  max {totals.Max:N2}x  cascades/spin {totals.Cascades / n:F2}  [{watch.ElapsedMilliseconds} ms]");
            for (int t = 2; t <= Symbol.MaxTier; t++)
                Console.WriteLine($"  tier {t} created: 1 in {n / Math.Max(1, totals.Tiers[t]),12:N1} spins  pays {rules.CreationPay(t) / 100.0:N1}x");
            Console.WriteLine($"  GRAND TRANSMUTATION: 1 in {n / Math.Max(1, totals.Transmutations):N0} spins  pays {rules.TransmutationPay / 100.0:N0}x   capped: {totals.Capped}");
            for (int b = 1; b < Buckets.Length; b++)
            {
                string label = b + 1 < Buckets.Length ? $"[{Buckets[b]}x, {Buckets[b + 1]}x)" : $"{Buckets[b]}x+";
                Console.WriteLine($"    win {label,-16} {totals.Histogram[b] / n:P4}");
            }
        }

        public static AlchemiaRules Parse(string text)
        {
            // 例: 120,800,5000:50000:1,1,1,2,2,3（位3・4・5 の配当と大錬成の配当は100倍した値、最後は連鎖倍率）
            var parts = text.Split(':');
            var tiers = parts[0].Split(',');
            if (parts.Length != 3 || tiers.Length != 3)
                throw new FormatException($"pays must be like 120,800,5000:50000:1,1,1,2,2,3 (got {text})");
            var multipliers = Array.ConvertAll(parts[2].Split(','), int.Parse);
            return new AlchemiaRules(new long[] { 0, 0, 0, long.Parse(tiers[0]), long.Parse(tiers[1]), long.Parse(tiers[2]) }, long.Parse(parts[1]), multipliers);
        }
    }
}
