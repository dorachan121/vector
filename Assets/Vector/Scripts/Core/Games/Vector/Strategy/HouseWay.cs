using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// ディーラーの配置ルール（ハウスウェイ）。ディーラーに裁量はなく、常にこの手順で置く:
    /// 空きマスそれぞれに引いたカードを置いた場合を、表向きのプレイヤー手札も含めて最後まで試算し、
    /// ディーラーの勝ちが最も多いマスを選ぶ。
    /// </summary>
    public sealed class HouseWay : IDealerPolicy
    {
        /// <summary>同点のとき1手先評価の候補を優先させるための加点（試行1回分未満）。</summary>
        const double TieBreakBonus = 0.5;

        readonly IRandomSource random;
        readonly int playoutsPerCell;

        public HouseWay(IRandomSource random, int playoutsPerCell)
        {
            if (playoutsPerCell < 0)
                throw new ArgumentOutOfRangeException(nameof(playoutsPerCell));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            this.playoutsPerCell = playoutsPerCell;
        }

        public int ChooseCell(int[] values, int cardValue, LineFamily playerFamily, IReadOnlyList<int> playerHand)
        {
            int greedyCell = GreedyPolicy.ChooseDealerCell(values, cardValue, playerFamily);
            if (playoutsPerCell == 0 || Scoring.EmptyCount(values) <= 1)
                return greedyCell;

            int bestCell = greedyCell;
            double bestScore = double.MinValue;
            var v = (int[])values.Clone();
            for (int cell = 0; cell < v.Length; cell++)
            {
                if (v[cell] != 0)
                    continue;
                v[cell] = cardValue;
                double score = cell == greedyCell ? TieBreakBonus : 0;
                for (int n = 0; n < playoutsPerCell; n++)
                    score -= Playout.Run(v, playerHand, playerFamily, true, random);
                v[cell] = 0;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCell = cell;
                }
            }
            return bestCell;
        }
    }
}
