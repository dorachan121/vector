using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Vector.Game.Audio;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>押せるプレート（ホームのショートカットなど）。押すと沈み、離すと弾む。中身は呼び出し側が Content に置く。</summary>
    public sealed class TileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
    {
        Action onClick;

        public RectTransform Content { get; private set; }

        public static TileButton Create(Transform parent, string name, Vector2 position, Vector2 size, Color color, Action onClick)
        {
            var rt = Ui.Card(parent, name, position, size, color, 26, true, true);
            var tile = rt.gameObject.AddComponent<TileButton>();
            tile.onClick = onClick;
            tile.Content = rt;
            return tile;
        }

        public void OnPointerDown(PointerEventData e) => Tween.Scale(transform, Vector3.one * 0.96f, 0.08f);

        public void OnPointerUp(PointerEventData e) => Tween.Scale(transform, Vector3.one, 0.25f, Ease.OutBack);

        public void OnPointerExit(PointerEventData e) => Tween.Scale(transform, Vector3.one, 0.2f);

        public void OnPointerClick(PointerEventData e)
        {
            Sfx.Play(SfxKind.Button);
            onClick?.Invoke();
        }
    }
}
