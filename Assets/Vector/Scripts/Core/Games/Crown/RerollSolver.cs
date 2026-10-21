using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>
    /// 振り直し2回の後ろ向き帰納（プレイヤーの最善戦略とディーラーのハウスウェイで共通）。
    /// 止めたときの値から、残り1回・2回の振り直しで各「残し方」の期待値を求め、最も良い残し方を選ぶ。
    /// maximize が true なら値の大きい方（プレイヤー）、false なら小さい方（ディーラー）を良いとする。
    /// </summary>
    internal static class RerollSolver
    {
        const double Tolerance = 1e-12;

        /// <summary>stand（手の番号ごとの止めたときの値）から、残し方ごとの期待値（残り1回・2回）と、最初に振る前の期待値。</summary>
        public static (double[] oneLeft, double[] twoLeft, double initial) Solve(double[] stand, bool maximize)
        {
            var oneLeft = DiceTables.Expect(stand);
            var twoLeft = DiceTables.Expect(Best(oneLeft, stand, maximize));
            return (oneLeft, twoLeft, DiceTables.ExpectInitial(Best(twoLeft, stand, maximize)));
        }

        /// <summary>
        /// 今の目で最も良い残し方をマスクで返す（全部残すなら止める）。holdValues は残りの振り直し回数に合う段の値。
        /// 同じ値なら止める方、次に残す個数の多い方を選ぶ。
        /// </summary>
        public static int ChooseHold(IReadOnlyList<int> dice, double standValue, double[] holdValues, bool maximize, out double best)
        {
            int bestMask = (1 << dice.Count) - 1;
            best = standValue;
            foreach (int mask in DiceTables.RerollMasks)
            {
                double v = holdValues[DiceTables.HoldIndexOf(dice, mask)];
                if (maximize ? v > best + Tolerance : v < best - Tolerance)
                {
                    best = v;
                    bestMask = mask;
                }
            }
            return bestMask;
        }

        /// <summary>手の番号ごとに、止める（確定値）か振り直す（次の段の期待値）かの良い方。</summary>
        static double[] Best(double[] holdValues, double[] stand, bool maximize)
        {
            var v = new double[DiceTables.HandCount];
            for (int i = 0; i < v.Length; i++)
            {
                double best = stand[i];
                var holds = DiceTables.HoldsOf(i);
                // holds[0] は全部残す（＝止める）なので飛ばす
                for (int k = 1; k < holds.Count; k++)
                {
                    double h = holdValues[holds[k]];
                    if (maximize ? h > best : h < best)
                        best = h;
                }
                v[i] = best;
            }
            return v;
        }
    }
}
