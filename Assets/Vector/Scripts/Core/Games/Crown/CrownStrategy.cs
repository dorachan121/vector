using System;
using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>
    /// プレイヤーの最善の振り方（厳密解）。王冠の目ごとに、すべての手について
    /// 「そこで止めたときの期待値（ディーラーのハウスウェイを考慮）」を求め、残り2回・1回の振り直しを後ろから解く。
    /// 戦略ヒント・時間切れの自動進行・RTP の検証に使う。作成後は読み取り専用でスレッド安全。
    /// </summary>
    public sealed class CrownStrategy
    {
        readonly double[] stand;
        readonly double[] oneLeft;
        readonly double[] twoLeft;

        CrownStrategy(int crown, double[] stand, double[] oneLeft, double[] twoLeft, double expectedReturn)
        {
            Crown = crown;
            this.stand = stand;
            this.oneLeft = oneLeft;
            this.twoLeft = twoLeft;
            ExpectedReturn = expectedReturn;
        }

        public int Crown { get; }

        /// <summary>この王冠の目で、最善に打った場合のアンティ1単位あたりの払い戻しの期待値（＝RTP）。</summary>
        public double ExpectedReturn { get; }

        public static CrownStrategy Solve(CrownRules rules, int crown)
        {
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));
            if (crown < 1 || crown > DiceRank.Faces)
                throw new ArgumentOutOfRangeException(nameof(crown));

            double bonus = rules.CrownYachtBonus.Ratio;
            var byRank = new Dictionary<int, double>();
            var stand = new double[DiceTables.HandCount];
            for (int i = 0; i < stand.Length; i++)
            {
                var rank = DealerPlan.RankOf(i, crown);
                if (!byRank.TryGetValue(rank.Key, out double value))
                {
                    value = DealerPlan.For(rules, rank, crown).PlayerReturn;
                    byRank[rank.Key] = value;
                }
                stand[i] = value + (rank.IsCrownYachtFor(crown) ? bonus : 0);
            }
            var (oneLeft, twoLeft, initial) = RerollSolver.Solve(stand, maximize: true);
            return new CrownStrategy(crown, stand, oneLeft, twoLeft, initial);
        }

        /// <summary>今の手で止めたときの期待値。</summary>
        public double StandValue(IReadOnlyList<int> dice) => stand[DiceTables.HandIndex(DiceTables.CountsOf(dice))];

        /// <summary>holdMask の目を残して振り直したときの期待値。rollsLeft は残りの振り直し回数（1 か 2）。</summary>
        public double HoldValue(IReadOnlyList<int> dice, int holdMask, int rollsLeft)
        {
            if (holdMask == (1 << dice.Count) - 1)
                return StandValue(dice);
            var values = rollsLeft >= 2 ? twoLeft : oneLeft;
            return values[DiceTables.HoldIndexOf(dice, holdMask)];
        }

        /// <summary>最善の残し方。同じ期待値なら止める方、次に残す個数の多い方を選ぶ。</summary>
        public HoldAdvice Advise(IReadOnlyList<int> dice, int rollsLeft)
        {
            if (rollsLeft < 1)
                throw new ArgumentOutOfRangeException(nameof(rollsLeft));
            double standValue = StandValue(dice);
            int bestMask = RerollSolver.ChooseHold(dice, standValue, rollsLeft >= 2 ? twoLeft : oneLeft, maximize: true, out double best);
            return new HoldAdvice(bestMask, best, standValue);
        }
    }
}
