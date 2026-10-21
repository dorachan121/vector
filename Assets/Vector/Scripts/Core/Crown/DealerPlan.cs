using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>
    /// ディーラーの振り方（ハウスウェイ）。ディーラーに裁量はなく、プレイヤーの確定した手を見て、
    /// 「プレイヤーへの払い戻しの期待値」が最も小さくなる目を残す（＝勝つ見込みが最も高い。引き分けは半分の勝ちとして数える）。
    /// 同じ見込みなら、振り直さずに止める方を選ぶ。
    /// </summary>
    public sealed class DealerPlan
    {
        static readonly DiceRank[][] ranks = BuildRanks();

        readonly double[] stand;
        readonly double[] oneLeft;
        readonly double[] twoLeft;

        DealerPlan(double[] stand, double[] oneLeft, double[] twoLeft, double playerReturn)
        {
            this.stand = stand;
            this.oneLeft = oneLeft;
            this.twoLeft = twoLeft;
            PlayerReturn = playerReturn;
        }

        /// <summary>このプレイヤーの手に対する、アンティ1単位あたりの払い戻しの期待値（ボーナスを除く、ディーラーが振る前）。</summary>
        public double PlayerReturn { get; }

        /// <summary>手の番号の強さ（王冠の目ごと）。</summary>
        public static DiceRank RankOf(int hand, int crown) => ranks[crown - 1][hand];

        public static DealerPlan For(CrownRules rules, DiceRank player, int crown)
        {
            var table = ranks[crown - 1];
            double win = 1 + rules.WinPays.Ratio;
            var final = new double[DiceTables.HandCount];
            for (int i = 0; i < final.Length; i++)
                final[i] = table[i] > player ? 0 : table[i] == player ? 1 : win;
            var oneLeft = DiceTables.Expect(final);
            var afterSecond = Best(oneLeft, final);
            var twoLeft = DiceTables.Expect(afterSecond);
            var afterFirst = Best(twoLeft, final);
            return new DealerPlan(final, oneLeft, twoLeft, DiceTables.ExpectInitial(afterFirst));
        }

        /// <summary>
        /// 振り直す目を決める。残す目の位置をマスクで返し、全部残すなら止める。rollsLeft は残りの振り直し回数（1 か 2）。
        /// </summary>
        public int ChooseHold(IReadOnlyList<int> dice, int rollsLeft)
        {
            var values = rollsLeft >= 2 ? twoLeft : oneLeft;
            int hand = DiceTables.HandIndex(DiceTables.CountsOf(dice));
            int all = (1 << dice.Count) - 1;
            int bestMask = all;
            double best = stand[hand];
            foreach (int mask in DiceTables.RerollMasks)
            {
                double v = values[DiceTables.HoldIndexOf(dice, mask)];
                if (v < best - 1e-12)
                {
                    best = v;
                    bestMask = mask;
                }
            }
            return bestMask;
        }

        /// <summary>止める（全部残す）場合は確定値、振り直す場合は次の段の期待値として、最小を取る。</summary>
        internal static double[] Best(double[] holdValues, double[] stand)
        {
            var v = new double[DiceTables.HandCount];
            for (int i = 0; i < v.Length; i++)
            {
                double best = stand[i];
                var holds = DiceTables.HoldsOf(i);
                for (int k = 1; k < holds.Count; k++)
                {
                    if (holdValues[holds[k]] < best)
                        best = holdValues[holds[k]];
                }
                v[i] = best;
            }
            return v;
        }

        static DiceRank[][] BuildRanks()
        {
            var result = new DiceRank[DiceRank.Faces][];
            for (int crown = 1; crown <= DiceRank.Faces; crown++)
            {
                result[crown - 1] = new DiceRank[DiceTables.HandCount];
                for (int i = 0; i < DiceTables.HandCount; i++)
                    result[crown - 1][i] = DiceRank.OfCounts(DiceTables.CountsOfHand(i), crown);
            }
            return result;
        }
    }
}
