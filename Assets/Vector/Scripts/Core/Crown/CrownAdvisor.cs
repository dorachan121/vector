using System;

namespace Vector.Core.Crown
{
    /// <summary>
    /// 王冠の目ごとの最善戦略を必要になったときに解いて覚えておく。1つ解くのに数十ミリ秒かかるので、
    /// 演出側はワーカースレッドから呼ぶ。複数のスレッドから同時に呼んでよい。
    /// </summary>
    public sealed class CrownAdvisor
    {
        readonly CrownRules rules;
        readonly CrownStrategy[] strategies = new CrownStrategy[DiceRank.Faces];
        readonly object gate = new object();

        public CrownAdvisor(CrownRules rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public CrownStrategy For(int crown)
        {
            if (crown < 1 || crown > DiceRank.Faces)
                throw new ArgumentOutOfRangeException(nameof(crown));
            lock (gate)
            {
                return strategies[crown - 1] ??= CrownStrategy.Solve(rules, crown);
            }
        }

        /// <summary>全ての王冠の目を平均した、最善に打った場合の RTP（厳密値）。</summary>
        public double ExpectedReturn()
        {
            double sum = 0;
            for (int crown = 1; crown <= DiceRank.Faces; crown++)
                sum += For(crown).ExpectedReturn;
            return sum / DiceRank.Faces;
        }
    }
}
