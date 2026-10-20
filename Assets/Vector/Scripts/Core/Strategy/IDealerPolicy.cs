using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>ディーラーが引いたカードを置くマスを決める方針。</summary>
    public interface IDealerPolicy
    {
        /// <param name="values">盤面の点数（空きマスは0）。</param>
        /// <param name="cardValue">ディーラーが引いたカードの点数。</param>
        /// <param name="playerFamily">プレイヤーが選んだラインの種類。</param>
        /// <param name="playerHand">テーブルに表向きで残っているプレイヤーの手札の点数。</param>
        int ChooseCell(int[] values, int cardValue, LineFamily playerFamily, IReadOnlyList<int> playerHand);
    }
}
