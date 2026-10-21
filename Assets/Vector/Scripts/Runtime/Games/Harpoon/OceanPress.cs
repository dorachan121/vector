using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// 海の画面が押された位置（この RectTransform の座標）を知らせる。泳いでいる獲物は押してから離すまでに動くので、
    /// クリックではなく押した瞬間で受け取る。
    /// </summary>
    public sealed class OceanPress : MonoBehaviour, IPointerDownHandler
    {
        public event Action<Vector2> Pressed;

        public void OnPointerDown(PointerEventData e)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out var local))
                Pressed?.Invoke(local);
        }
    }
}
