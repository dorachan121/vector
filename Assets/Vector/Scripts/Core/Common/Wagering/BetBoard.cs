using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// 番号で表すベットエリアへのチップの配置。置いた順番を覚えておき、1手ずつ取り消せる。
    /// ベットエリアの番号の意味はゲームごとに決める（VECTOR は <see cref="BetSpot"/> の値）。
    /// </summary>
    public sealed class BetBoard
    {
        readonly SortedDictionary<int, long> amounts = new SortedDictionary<int, long>();
        readonly List<KeyValuePair<int, long>> history = new List<KeyValuePair<int, long>>();

        public long this[int spot] => amounts.TryGetValue(spot, out var v) ? v : 0;

        public long Total
        {
            get
            {
                long total = 0;
                foreach (var v in amounts.Values)
                    total += v;
                return total;
            }
        }

        public bool IsEmpty => amounts.Count == 0;

        public bool CanUndo => history.Count > 0;

        public int Count => amounts.Count;

        public void Add(int spot, long amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            amounts[spot] = checked(this[spot] + amount);
            history.Add(new KeyValuePair<int, long>(spot, amount));
        }

        /// <summary>直前に置いたチップを取り消す。</summary>
        public bool TryUndo(out int spot, out long amount)
        {
            if (history.Count == 0)
            {
                spot = -1;
                amount = 0;
                return false;
            }
            var last = history[history.Count - 1];
            history.RemoveAt(history.Count - 1);
            spot = last.Key;
            amount = last.Value;
            long rest = this[spot] - amount;
            if (rest > 0)
                amounts[spot] = rest;
            else
                amounts.Remove(spot);
            return true;
        }

        public void Clear()
        {
            amounts.Clear();
            history.Clear();
        }

        /// <summary>金額が置かれているベットエリアを番号順に列挙する。</summary>
        public IEnumerable<KeyValuePair<int, long>> NonZero() => amounts;

        public BetBoard Clone()
        {
            var copy = new BetBoard();
            foreach (var entry in history)
                copy.Add(entry.Key, entry.Value);
            return copy;
        }
    }
}
