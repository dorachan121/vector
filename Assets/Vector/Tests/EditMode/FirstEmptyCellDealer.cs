using System;
using System.Collections.Generic;
using Vector.Core;

namespace Vector.Tests
{
    /// <summary>常に番号が最小の空きマスに置くテスト用ディーラー。</summary>
    sealed class FirstEmptyCellDealer : IDealerPolicy
    {
        public int ChooseCell(int[] values, int cardValue, LineFamily playerFamily, IReadOnlyList<int> playerHand)
            => Array.IndexOf(values, 0);
    }
}
