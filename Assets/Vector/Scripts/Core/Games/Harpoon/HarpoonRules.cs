using System;
using System.Collections.Generic;

namespace Vector.Core.Harpoon
{
    /// <summary>
    /// HARPOON のルールと配当表（不変）。どの獲物を狙っても、捕まえる確率 × 倍率がちょうど RTP（97%）になる。
    /// 確率は整数の比で決めるので、シミュレーションを回さなくても RTP が厳密に分かる。
    /// </summary>
    public sealed class HarpoonRules : IBetLimits
    {
        public static readonly HarpoonRules Standard = new HarpoonRules();

        /// <summary>払い戻し率（%）。どの獲物でも同じ。</summary>
        public const int ReturnPercent = 97;

        static readonly QuarryPrize[][] Prizes =
        {
            new[] { new QuarryPrize(2) },
            new[] { new QuarryPrize(3) },
            new[] { new QuarryPrize(5) },
            new[] { new QuarryPrize(10) },
            new[] { new QuarryPrize(20) },
            new[] { new QuarryPrize(50) },
            new[] { new QuarryPrize(100) },
            new[] { new QuarryPrize(300) },
            // 金のクラゲ（平均 26.75 倍）
            new[] { new QuarryPrize(5, 40), new QuarryPrize(10, 30), new QuarryPrize(25, 15), new QuarryPrize(50, 10), new QuarryPrize(200, 4), new QuarryPrize(500, 1) },
        };

        readonly long[] betLevels = { 5, 10, 20, 50, 100, 200 };

        public IReadOnlyList<long> BetLevels => betLevels;

        /// <summary>獲物を捕まえたときの倍率の候補。</summary>
        public IReadOnlyList<QuarryPrize> PrizesOf(int quarry) => Prizes[Require(quarry)];

        /// <summary>倍率の候補が2つ以上ある（捕まえてから倍率が決まる）。</summary>
        public bool IsMystery(int quarry) => PrizesOf(quarry).Count > 1;

        public int MinMultiple(int quarry)
        {
            int min = int.MaxValue;
            foreach (var p in PrizesOf(quarry))
                min = Math.Min(min, p.Multiple);
            return min;
        }

        public int MaxMultiple(int quarry)
        {
            int max = 0;
            foreach (var p in PrizesOf(quarry))
                max = Math.Max(max, p.Multiple);
            return max;
        }

        /// <summary>捕まえたときの倍率の平均。</summary>
        public double ExpectedMultiple(int quarry)
        {
            var (weights, weighted) = Totals(quarry);
            return weighted / (double)weights;
        }

        /// <summary>捕まえる確率を整数の比で（hits / outOf）。平均倍率を m とすると、確率は RTP / m。</summary>
        public (int hits, int outOf) CatchOdds(int quarry)
        {
            var (weights, weighted) = Totals(quarry);
            return (ReturnPercent * weights, 100 * weighted);
        }

        public double CatchProbability(int quarry)
        {
            var (hits, outOf) = CatchOdds(quarry);
            return hits / (double)outOf;
        }

        /// <summary>払い戻し率（どの獲物でも <see cref="ReturnPercent"/> / 100）。</summary>
        public double ReturnOf(int quarry) => CatchProbability(quarry) * ExpectedMultiple(quarry);

        public bool IsSpotAvailable(int spot) => Quarry.IsValid(spot);
        public long MinFor(int spot) => betLevels[0];
        public long MaxFor(int spot) => betLevels[betLevels.Length - 1];
        public bool IsPrimary(int spot) => Quarry.IsValid(spot);

        static (int weights, int weighted) Totals(int quarry)
        {
            int weights = 0, weighted = 0;
            foreach (var p in Prizes[Require(quarry)])
            {
                weights += p.Weight;
                weighted += p.Multiple * p.Weight;
            }
            return (weights, weighted);
        }

        static int Require(int quarry)
        {
            if (!Quarry.IsValid(quarry))
                throw new ArgumentOutOfRangeException(nameof(quarry), quarry, "獲物の番号が不正です。");
            return quarry;
        }
    }
}
