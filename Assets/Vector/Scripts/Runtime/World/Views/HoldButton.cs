using UnityEngine;
using UnityEngine.EventSystems;

namespace Vector.Game.World
{
    /// <summary>押している間だけ Held が true になるボタン（タッチパッドの矢印）。指が外れたら離したとみなす。</summary>
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool Held { get; private set; }

        public void OnPointerDown(PointerEventData e) => Set(true);

        public void OnPointerUp(PointerEventData e) => Set(false);

        public void OnPointerExit(PointerEventData e) => Set(false);

        void OnDisable() => Held = false;

        void Set(bool held)
        {
            Held = held;
            transform.localScale = Vector3.one * (held ? 0.92f : 1f);
        }
    }
}
