using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>出目表（バカラの罫線にあたる履歴）。シューが替わるとリセットする。</summary>
    public sealed class Scoreboard
    {
        readonly List<RoundWinner> entries = new List<RoundWinner>();

        public IReadOnlyList<RoundWinner> Entries => entries;

        public void Add(RoundWinner winner) => entries.Add(winner);

        public void Clear() => entries.Clear();

        public int Count(RoundWinner winner)
        {
            int n = 0;
            foreach (var e in entries)
            {
                if (e == winner)
                    n++;
            }
            return n;
        }
    }
}
