using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// 1手先の形勢だけを見る素早い方針。プレイアウト（先読みの試行）の中で両者の手を決めるのに使う。
    /// 同評価の候補はマス番号の小さい方を選ぶので結果は決定的。
    /// </summary>
    public static class GreedyPolicy
    {
        const double Epsilon = 1e-9;

        public static Placement ChoosePlacement(int[] values, IReadOnlyList<int> hand, LineFamily playerFamily)
        {
            var best = new Placement(-1, -1);
            double bestScore = double.MinValue;
            for (int h = 0; h < hand.Count; h++)
            {
                if (HasSameValueBefore(hand, h))
                    continue;
                for (int cell = 0; cell < values.Length; cell++)
                {
                    if (values[cell] != 0)
                        continue;
                    values[cell] = hand[h];
                    double score = Heuristics.PlayerAdvantage(values, playerFamily);
                    values[cell] = 0;
                    if (score > bestScore + Epsilon)
                    {
                        bestScore = score;
                        best = new Placement(h, cell);
                    }
                }
            }
            if (best.HandIndex < 0)
                throw new InvalidOperationException("置ける手がありません。");
            return best;
        }

        public static int ChooseDealerCell(int[] values, int cardValue, LineFamily playerFamily)
        {
            int bestCell = -1;
            double bestScore = double.MinValue;
            for (int cell = 0; cell < values.Length; cell++)
            {
                if (values[cell] != 0)
                    continue;
                values[cell] = cardValue;
                double score = Heuristics.DealerAdvantage(values, playerFamily);
                values[cell] = 0;
                if (score > bestScore + Epsilon)
                {
                    bestScore = score;
                    bestCell = cell;
                }
            }
            if (bestCell < 0)
                throw new InvalidOperationException("空きマスがありません。");
            return bestCell;
        }

        /// <summary>同じ点数のカードは評価も同じなので、最初の1枚だけを候補にする。</summary>
        public static bool HasSameValueBefore(IReadOnlyList<int> hand, int index)
        {
            for (int k = 0; k < index; k++)
            {
                if (hand[k] == hand[index])
                    return true;
            }
            return false;
        }
    }
}
