using System.Collections.Generic;

namespace Vector.Core.Alchemia
{
    /// <summary>
    /// 連鎖の1段（演出用）。融合 → 大錬成で消えるマス → 落下 → 上からの補充 の順に起きる。
    /// </summary>
    public sealed class CascadeStep
    {
        public CascadeStep(int multiplier, IReadOnlyList<Merge> merges, IReadOnlyList<int> dissolved, Symbol[] afterMerge,
            IReadOnlyList<Fall> falls, IReadOnlyList<int> spawned, Symbol[] after)
        {
            Multiplier = multiplier;
            Merges = merges;
            Dissolved = dissolved;
            AfterMerge = afterMerge;
            Falls = falls;
            Spawned = spawned;
            After = after;
        }

        /// <summary>この段の連鎖倍率。</summary>
        public int Multiplier { get; }

        public IReadOnlyList<Merge> Merges { get; }

        /// <summary>大錬成で消えた（塊以外の）マス。</summary>
        public IReadOnlyList<int> Dissolved { get; }

        /// <summary>融合と大錬成の直後（落下の前）の盤面。</summary>
        public Symbol[] AfterMerge { get; }

        public IReadOnlyList<Fall> Falls { get; }

        /// <summary>上から補充されたマス。</summary>
        public IReadOnlyList<int> Spawned { get; }

        /// <summary>この段の最後の盤面。</summary>
        public Symbol[] After { get; }

        public long PayHundredths
        {
            get
            {
                long sum = 0;
                foreach (var m in Merges)
                    sum += m.PayHundredths;
                return sum;
            }
        }
    }
}
