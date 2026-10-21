using System;
using UnityEngine;
using Vector.Core;

namespace Vector.Game.Casino
{
    /// <summary>チップを置ける場所（フェルトのベットエリアやオッズ表のマス）。番号はゲームごとのベットエリアの番号。</summary>
    public interface IBetTarget
    {
        int Id { get; }

        event Action<int> Clicked;

        /// <summary>チップの山の一番上（チップを飛ばす演出の起点・終点）。</summary>
        Vector3 ChipWorldPosition { get; }

        void SetAmount(long amount);

        void SetInteractable(bool value);

        /// <summary>精算結果を表示する（勝ちは光り、負けは暗くなる）。</summary>
        void ShowOutcome(BetOutcome outcome);

        void ResetOutcome();
    }
}
