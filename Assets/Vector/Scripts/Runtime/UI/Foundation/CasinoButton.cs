using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector.Game.Audio;

namespace Vector.Game.UI
{
    /// <summary>押し心地のあるボタン。ホバーで明るく、押すと沈み、無効時は暗くなる。</summary>
    public sealed class CasinoButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        const float HoverScale = 1.03f;
        const float PressScale = 0.96f;

        Image background;
        TextMeshProUGUI label;
        Color baseColor;
        Action onClick;
        bool interactable = true;
        bool hovered;
        bool pressed;

        public TextMeshProUGUI Label => label;

        public bool Interactable
        {
            get => interactable;
            set
            {
                if (interactable == value)
                    return;
                interactable = value;
                Refresh();
            }
        }

        public void Init(Image background, TextMeshProUGUI label, Color baseColor, Action onClick)
        {
            this.background = background;
            this.label = label;
            this.baseColor = baseColor;
            this.onClick = onClick;
            Refresh();
        }

        public void SetLabel(string text) => label.text = text;

        public void SetColor(Color color)
        {
            baseColor = color;
            Refresh();
        }

        void Refresh()
        {
            var c = baseColor;
            if (!interactable)
                c = Color.Lerp(c, new Color(0.15f, 0.15f, 0.15f, c.a), 0.65f);
            else if (pressed)
                c = Color.Lerp(c, Color.black, 0.2f);
            else if (hovered)
                c = Color.Lerp(c, Color.white, 0.15f);
            background.color = c;
            label.alpha = interactable ? 1f : 0.45f;
            float scale = !interactable ? 1f : pressed ? PressScale : hovered ? HoverScale : 1f;
            transform.localScale = Vector3.one * scale;
        }

        public void OnPointerEnter(PointerEventData e)
        {
            hovered = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData e)
        {
            hovered = false;
            pressed = false;
            Refresh();
        }

        public void OnPointerDown(PointerEventData e)
        {
            pressed = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData e)
        {
            pressed = false;
            Refresh();
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (!interactable)
                return;
            Sfx.Play(SfxKind.Button);
            onClick?.Invoke();
        }
    }
}
