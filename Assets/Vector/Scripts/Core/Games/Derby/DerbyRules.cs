using System;
using System.Collections.Generic;

namespace Vector.Core.Derby
{
    /// <summary>
    /// FOUR BEASTS DERBY のルール（不変）。レースごとに距離とゲート（スタート位置の前進）を抽選し、
    /// 着順の厳密な確率から固定オッズを出す。オッズは「払戻率 × 1 / 確率」を 0.1 倍単位で切り捨てるので、
    /// どの馬券も RTP は払戻率以下になる（README 参照）。
    /// </summary>
    public sealed class DerbyRules
    {
        public static readonly DerbyRules Standard = new DerbyRules(0.97);

        /// <param name="targetReturn">オッズを決める払戻率。シミュレーターで別の値を検証するときに差し替える。</param>
        public DerbyRules(double targetReturn)
        {
            if (targetReturn <= 0 || targetReturn >= 1)
                throw new ArgumentOutOfRangeException(nameof(targetReturn));
            TargetReturn = targetReturn;
        }

        /// <summary>オッズの基準になる払戻率（これ以上は払わない）。</summary>
        public double TargetReturn { get; }

        /// <summary>ゴールまでの距離（歩数）の抽選範囲。</summary>
        public int MinDistance { get; } = 8;
        public int MaxDistance { get; } = 12;

        /// <summary>ゲートの前進（0〜この値の歩数だけ前からスタート）。</summary>
        public int MaxGate { get; } = 3;

        public long WinMinBet { get; } = 10;
        public long WinMaxBet { get; } = 10_000;
        public long ExactaMinBet { get; } = 5;
        public long ExactaMaxBet { get; } = 1_000;

        /// <summary>出せる最小・最大のオッズ（10倍した値。1.1倍 〜 999.9倍）。これより低いオッズの馬券は発売しない。</summary>
        public int MinOddsTenths { get; } = 11;
        public int MaxOddsTenths { get; } = 9999;

        /// <summary>ベット受付の制限時間（秒）。</summary>
        public float BettingSeconds { get; } = 20f;

        readonly Dictionary<int, RaceCard> cards = new Dictionary<int, RaceCard>();

        /// <summary>
        /// 出走表（距離とゲートが同じなら確率もオッズも同じ）。抽選されうる出走表は 1,280 通りしかないので、作ったものを覚えておく。
        /// 複数のスレッドから同時に呼んでよい。
        /// </summary>
        public RaceCard CardFor(int distance, IReadOnlyList<int> gates)
        {
            int key = distance;
            foreach (int g in gates)
                key = key * 16 + g;
            lock (cards)
            {
                if (!cards.TryGetValue(key, out var card))
                {
                    card = new RaceCard(this, distance, gates);
                    cards[key] = card;
                }
                return card;
            }
        }

        /// <summary>確率からオッズ（10倍した値）を出す。発売しない馬券は 0。</summary>
        public int OddsTenthsFor(double probability)
        {
            if (probability <= 0)
                return 0;
            double tenths = Math.Floor(TargetReturn / probability * 10 + 1e-9);
            if (tenths < MinOddsTenths)
                return 0;
            return (int)Math.Min(tenths, MaxOddsTenths);
        }
    }
}
