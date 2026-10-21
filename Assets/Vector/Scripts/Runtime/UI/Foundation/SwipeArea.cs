using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vector.Game.UI
{
    /// <summary>
    /// 横にはらう操作を知らせる（カルーセルの送り）。指を置いて離すまでに、画面の幅の一定の割合より横に動いたらはらったとみなす。
    /// 子のボタンを押したときはボタンが受け取るので、はらう操作にはならない。
    /// </summary>
    public sealed class SwipeArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>はらったとみなす横の移動（画面の幅に対する割合）。</summary>
        const float Threshold = 0.06f;

        Vector2 down;

        /// <summary>はらった向き（左へはらうと +1 = 次へ、右へはらうと -1 = 前へ）。</summary>
        public event Action<int> Swiped;

        public void OnPointerDown(PointerEventData e) => down = e.position;

        public void OnPointerUp(PointerEventData e)
        {
            var moved = e.position - down;
            if (Mathf.Abs(moved.x) > Screen.width * Threshold && Mathf.Abs(moved.x) > Mathf.Abs(moved.y))
                Swiped?.Invoke(moved.x < 0 ? 1 : -1);
        }
    }
}
