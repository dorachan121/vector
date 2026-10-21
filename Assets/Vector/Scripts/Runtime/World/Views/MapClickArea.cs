using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vector.Game.World
{
    /// <summary>見取り図のクリック（タップ）を、押した位置（この RectTransform の座標）と一緒に通知する。</summary>
    public sealed class MapClickArea : MonoBehaviour, IPointerClickHandler
    {
        public event Action<Vector2> Clicked;

        public void OnPointerClick(PointerEventData e)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out var local))
                Clicked?.Invoke(local);
        }
    }
}
