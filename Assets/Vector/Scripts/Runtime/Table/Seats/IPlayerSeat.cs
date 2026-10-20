using System;
using System.Collections;
using Vector.Core;

namespace Vector.Game.Table
{
    /// <summary>プレイヤー席の判断の出どころ（人間の入力か、観戦モードのAIか）。</summary>
    public interface IPlayerSeat
    {
        /// <summary>ラインの種類を決めて done に渡す。</summary>
        IEnumerator ChooseFamily(RoundEngine round, Action<LineFamily> done);

        /// <summary>手札のどのカードをどこに置くかを決めて done に渡す。</summary>
        IEnumerator ChoosePlacement(RoundEngine round, Action<Placement> done);
    }
}
