using System;
using System.Collections.Generic;

namespace Vector.Core.Alchemia
{
    /// <summary>
    /// ALCHEMIA のルールと配当表（不変）。配当は Tools/Simulator で数億回のスピンを回して検証した値（README 参照）。
    /// 位2（晶）までの融合は配当なしで、位3以上を作ったときに払う（賭け金未満の「勝ち」を出さないため）。
    /// </summary>
    public sealed class AlchemiaRules : IBetLimits
    {
        public const int Spin = 0;

        public static readonly AlchemiaRules Standard = new AlchemiaRules(new long[] { 0, 0, 0, 100, 600, 3_000 }, 30_000, new[] { 1, 1, 1, 2, 2, 3 });

        readonly long[] creationPays;
        readonly int[] cascadeMultipliers;
        readonly long[] betLevels = { 10, 20, 50, 100, 200, 500, 1_000 };

        /// <param name="creationPays">位ごとの「その位を作ったとき」の配当（賭け金を含む払い戻しの倍率を100倍した値。添字が位）。</param>
        /// <param name="transmutationPay">賢者の石がさらに融合した大錬成の配当（100倍した値）。</param>
        /// <param name="cascadeMultipliers">連鎖の段ごとの倍率（最後の値が以降も続く）。</param>
        public AlchemiaRules(long[] creationPays, long transmutationPay, int[] cascadeMultipliers)
        {
            if (creationPays == null || creationPays.Length != Symbol.MaxTier + 1)
                throw new ArgumentException("位0〜5の配当が必要です。", nameof(creationPays));
            if (cascadeMultipliers == null || cascadeMultipliers.Length == 0)
                throw new ArgumentException("連鎖倍率が必要です。", nameof(cascadeMultipliers));
            this.creationPays = (long[])creationPays.Clone();
            this.cascadeMultipliers = (int[])cascadeMultipliers.Clone();
            TransmutationPay = transmutationPay;
        }

        public int Columns { get; } = 6;
        public int Rows { get; } = 5;
        public int Cells => Columns * Rows;
        public int ElementCount { get; } = 5;

        /// <summary>この数以上つながると融合する。</summary>
        public int MinCluster { get; } = 3;

        /// <summary>1回のスピンの勝ちの上限（賭け金の倍数）。</summary>
        public long MaxWinMultiple { get; } = 5_000;

        /// <summary>連鎖が止まらない場合の打ち切り（実際にはまず起きない）。</summary>
        public int MaxCascades { get; } = 100;

        public long TransmutationPay { get; }

        public IReadOnlyList<long> BetLevels => betLevels;

        public float SpinSeconds { get; } = 15f;

        /// <summary>位 tier を作ったときの基本配当（100倍した値）。</summary>
        public long CreationPay(int tier) => creationPays[tier];

        /// <summary>塊の大きさによる配当の倍率（3個で1倍、1個増えるごとに +0.5倍）を掛ける。</summary>
        public long SizeScaled(long pay, int size) => pay * (size - 1) / 2;

        /// <summary>連鎖の段（0始まり）の倍率。</summary>
        public int MultiplierFor(int step) => cascadeMultipliers[Math.Min(step, cascadeMultipliers.Length - 1)];

        public IReadOnlyList<int> CascadeMultipliers => cascadeMultipliers;

        public bool IsSpotAvailable(int spot) => spot == Spin;
        public long MinFor(int spot) => betLevels[0];
        public long MaxFor(int spot) => betLevels[betLevels.Length - 1];
        public bool IsPrimary(int spot) => spot == Spin;
    }
}
