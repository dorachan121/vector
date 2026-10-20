using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// テーブル上のベット配置。チップを置いた順番を覚えておき、1手ずつ取り消せる。
    /// </summary>
    public sealed class BetLayout
    {
        static readonly BetSpot[] allSpots = (BetSpot[])Enum.GetValues(typeof(BetSpot));

        readonly Dictionary<BetSpot, long> amounts = new Dictionary<BetSpot, long>();
        readonly List<KeyValuePair<BetSpot, long>> history = new List<KeyValuePair<BetSpot, long>>();

        public long this[BetSpot spot] => amounts.TryGetValue(spot, out var v) ? v : 0;

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

        public void Add(BetSpot spot, long amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            amounts[spot] = checked(this[spot] + amount);
            history.Add(new KeyValuePair<BetSpot, long>(spot, amount));
        }

        /// <summary>直前に置いたチップを取り消す。</summary>
        public bool TryUndo(out BetSpot spot, out long amount)
        {
            if (history.Count == 0)
            {
                spot = default;
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

        /// <summary>金額が置かれているベットエリアを定義順に列挙する。</summary>
        public IEnumerable<KeyValuePair<BetSpot, long>> NonZero()
        {
            foreach (var spot in allSpots)
            {
                long v = this[spot];
                if (v > 0)
                    yield return new KeyValuePair<BetSpot, long>(spot, v);
            }
        }

        public BetLayout Clone()
        {
            var copy = new BetLayout();
            foreach (var entry in history)
                copy.Add(entry.Key, entry.Value);
            return copy;
        }
    }
}
