using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vector.Game.UI
{
    /// <summary>クリックとホバーを通知するだけの汎用ハンドラ（カード・マス・ベットエリア用）。</summary>
    public sealed class ClickTarget : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action Clicked;
        public event Action<bool> Hovered;

        public bool Interactable { get; set; } = true;

        public void OnPointerClick(PointerEventData e)
        {
            if (Interactable)
                Clicked?.Invoke();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (Interactable)
                Hovered?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (Interactable)
                Hovered?.Invoke(false);
        }
    }
}
