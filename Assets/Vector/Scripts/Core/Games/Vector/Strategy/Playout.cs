using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// 局面を両者の GreedyPolicy で最後まで打ち切る試行。ディーラーの引くカードは無限デッキ近似（A〜K一様）。
    /// </summary>
    public static class Playout
    {
        const int RankCount = 13;

        /// <summary>プレイヤー勝ちなら+1、負けなら-1、引き分けなら0。</summary>
        public static int Run(int[] values, IReadOnlyList<int> hand, LineFamily playerFamily, bool playerToMove, IRandomSource random)
        {
            var v = (int[])values.Clone();
            var h = new List<int>(hand);
            bool playerTurn = playerToMove;
            while (Scoring.EmptyCount(v) > 0)
            {
                if (playerTurn && h.Count > 0)
                {
                    var p = GreedyPolicy.ChoosePlacement(v, h, playerFamily);
                    v[p.Cell] = h[p.HandIndex];
                    h.RemoveAt(p.HandIndex);
                }
                else
                {
                    int card = random.Next(RankCount) + 1;
                    v[GreedyPolicy.ChooseDealerCell(v, card, playerFamily)] = card;
                }
                playerTurn = !playerTurn;
            }
            return Scoring.Compare(v, playerFamily);
        }
    }
}
