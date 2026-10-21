using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// 勝敗判定のルール。プレイヤーは1種類のラインを選び、ディーラーは残り2種類のうち得点の高い方を使う。
    /// 盤面は「点数の配列（空きマスは0）」で受け取る。
    /// </summary>
    public static class Scoring
    {
        /// <summary>ラインの種類の得点 = その種類で最大のライン合計。</summary>
        public static int FamilyScore(int[] values, LineFamily family)
        {
            int best = 0;
            foreach (var line in Lines.Of(family))
                best = Math.Max(best, line.Sum(values));
            return best;
        }

        /// <summary>得点を構成している（最大合計の）ライン。同点なら複数返す。</summary>
        public static List<Line> BestLines(int[] values, LineFamily family)
        {
            int score = FamilyScore(values, family);
            var result = new List<Line>();
            foreach (var line in Lines.Of(family))
            {
                if (line.Sum(values) == score)
                    result.Add(line);
            }
            return result;
        }

        /// <summary>ディーラーが採用するライン種類。同点なら Row→Column→Slash の順で先の方。</summary>
        public static LineFamily DealerFamily(int[] values, LineFamily playerFamily)
        {
            var a = Lines.Other(playerFamily, 0);
            var b = Lines.Other(playerFamily, 1);
            return FamilyScore(values, b) > FamilyScore(values, a) ? b : a;
        }

        /// <summary>ディーラーの得点 = プレイヤーが選ばなかった2種類のうち高い方。</summary>
        public static int DealerScore(int[] values, LineFamily playerFamily)
            => FamilyScore(values, DealerFamily(values, playerFamily));

        /// <summary>プレイヤー勝ちなら+1、ディーラー勝ちなら-1、引き分けなら0。</summary>
        public static int Compare(int[] values, LineFamily playerFamily)
            => Math.Sign(FamilyScore(values, playerFamily) - DealerScore(values, playerFamily));

        public static RoundWinner WinnerOf(int playerScore, int dealerScore)
            => playerScore > dealerScore ? RoundWinner.Player : playerScore < dealerScore ? RoundWinner.Dealer : RoundWinner.Tie;

        public static int EmptyCount(int[] values)
        {
            int n = 0;
            foreach (int v in values)
            {
                if (v == 0)
                    n++;
            }
            return n;
        }
    }
}
