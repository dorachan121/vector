using System;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// レベル・経験値・会員ランク・コンプポイントの規則。経験値とコンプは<b>賭けた額</b>だけで決まり、勝ち負けには左右されない
    /// （実際のカジノのコンプと同じ考え方で、ゲームの確率には一切影響しない）。
    /// </summary>
    public static class Progression
    {
        public const int MaxLevel = 30;

        /// <summary>この額を賭けるごとに経験値 1。</summary>
        public const long StakePerXp = 10;

        /// <summary>この額を賭けるごとにコンプ 1（会員ランクの倍率を掛ける前）。</summary>
        public const long StakePerComp = 100;

        static readonly int[] rankLevels = { 1, 4, 8, 13, 19, 26 };
        static readonly double[] compRates = { 1.0, 1.2, 1.5, 2.0, 2.5, 3.0 };

        /// <summary>レベル level から次のレベルまでに必要な経験値。</summary>
        public static long XpToNext(int level) => level >= MaxLevel ? 0 : 60 + 30L * level;

        /// <summary>レベル 1 からレベル level になるまでに必要な経験値の合計。</summary>
        public static long TotalXpFor(int level)
        {
            long sum = 0;
            for (int l = 1; l < level; l++)
                sum += XpToNext(l);
            return sum;
        }

        public static VipRank RankFor(int level)
        {
            var rank = VipRank.Bronze;
            for (int i = 0; i < rankLevels.Length; i++)
            {
                if (level >= rankLevels[i])
                    rank = (VipRank)i;
            }
            return rank;
        }

        public static double CompRate(VipRank rank) => compRates[(int)rank];

        /// <summary>経験値を足し、上がったレベルの数を返す。最大レベルでは経験値は貯まらない。</summary>
        public static int AddXp(RpgState state, long amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            int gained = 0;
            state.xp += amount;
            while (state.level < MaxLevel && state.xp >= XpToNext(state.level))
            {
                state.xp -= XpToNext(state.level);
                state.level++;
                gained++;
            }
            if (state.level >= MaxLevel)
                state.xp = 0;
            return gained;
        }

        public static long XpForStake(long stake) => stake / StakePerXp;

        public static long CompsForStake(long stake, VipRank rank) => (long)Math.Floor(stake / (double)StakePerComp * CompRate(rank));
    }
}
