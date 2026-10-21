using System;

namespace Vector.Core.Sugoroku
{
    /// <summary>
    /// SUGOROKU の厳密な最善手（後ろ向き帰納）。状態は「残りの回数・位置・倍率の段」だけなので、全状態の期待値を表にできる。
    /// 戦略ヒント・RTP の算出・シミュレーションの打ち手が同じ表を使う。
    /// </summary>
    public sealed class SugorokuSolver
    {
        readonly SugorokuRules rules;

        /// <summary>[残りの回数, 位置, 段] → 最善を尽くしたときの払い戻しの期待値（賭け金に対する倍率）。</summary>
        readonly double[,,] value;

        SugorokuSolver(SugorokuRules rules)
        {
            this.rules = rules;
            value = new double[rules.Turns + 1, rules.Goal + 1, rules.MaxTier + 1];
        }

        public static SugorokuSolver Solve(SugorokuRules rules)
        {
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));
            var solver = new SugorokuSolver(rules);
            solver.Fill();
            return solver;
        }

        /// <summary>振り出しから最善を尽くしたときの RTP。</summary>
        public double Return => Value(rules.Turns, 0, rules.StartTier);

        /// <summary>残り turnsLeft 回で position・段 tier にいるときの、払い戻しの期待値（賭け金に対する倍率）。</summary>
        public double Value(int turnsLeft, int position, int tier) => value[turnsLeft, position, tier];

        /// <summary>目 die で進んだときの期待値（残りの回数は1つ減る）。</summary>
        public double ValueOf(int turnsLeft, int position, int tier, int die)
        {
            var (to, next) = rules.Land(position, tier, die);
            return value[turnsLeft - 1, to, next];
        }

        /// <summary>出た2つの目のうち、進むべき目の番号（0 か 1）。期待値が同じなら大きい目で進む。</summary>
        public int Best(int turnsLeft, int position, int tier, int first, int second)
        {
            double a = ValueOf(turnsLeft, position, tier, first);
            double b = ValueOf(turnsLeft, position, tier, second);
            if (Math.Abs(a - b) < 1e-12)
                return second > first ? 1 : 0;
            return b > a ? 1 : 0;
        }

        void Fill()
        {
            int goal = rules.Goal;
            for (int tier = 0; tier <= rules.MaxTier; tier++)
            {
                for (int t = 0; t <= rules.Turns; t++)
                    value[t, goal, tier] = rules.PayFor(tier) / 100.0;
            }
            // 残り 0 回でまだ着いていなければ没収（配列の初期値 0 のまま）
            for (int t = 1; t <= rules.Turns; t++)
            {
                for (int position = 0; position < goal; position++)
                {
                    for (int tier = 0; tier <= rules.MaxTier; tier++)
                    {
                        double sum = 0;
                        for (int first = 1; first <= 6; first++)
                        {
                            for (int second = 1; second <= 6; second++)
                                sum += Math.Max(ValueOf(t, position, tier, first), ValueOf(t, position, tier, second));
                        }
                        value[t, position, tier] = sum / 36.0;
                    }
                }
            }
        }
    }
}
