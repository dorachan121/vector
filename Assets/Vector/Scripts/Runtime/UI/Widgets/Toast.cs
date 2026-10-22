using TMPro;
using UnityEngine;

namespace Vector.Game.UI.Widgets
{
    /// <summary>画面上部に短く出て消える通知。新しい通知は前の通知を置き換える。</summary>
    public sealed class Toast : MonoBehaviour
    {
        const float VisibleSeconds = 2.4f;
        static readonly Vector2 ToastSize = new Vector2(760, 52);

        /// <summary>通知の高さ。</summary>
        public static float Height => ToastSize.y;

        CanvasGroup group;
        TextMeshProUGUI text;

        /// <param name="top">親の上端から通知の中心までの距離（縦に長い端末でも上端に付いたまま）。</param>
        public static Toast Create(Transform root, float top)
        {
            var rt = Ui.Rect(root, "Toast", Vector2.zero, ToastSize);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -top);
            var toast = rt.gameObject.AddComponent<Toast>();
            toast.group = Ui.Group(rt.gameObject);
            toast.group.blocksRaycasts = false;
            toast.group.alpha = 0;
            Ui.Shadow(rt, "Shadow", new Vector2(0, -4), ToastSize, ToastSize.y / 2, 0.6f);
            Ui.Sliced(rt, "Plate", Chrome.Shade, Palette.VioletBright.WithAlpha(0.96f), Vector2.zero, ToastSize, ToastSize.y / 2);
            Ui.Sliced(rt, "Rim", Chrome.Rim, Palette.GoldMid.WithAlpha(0.9f), Vector2.zero, ToastSize, ToastSize.y / 2);
            Ui.Image(rt, "Spark", Chrome.Sparkle, Palette.GoldLight, new Vector2(-ToastSize.x / 2 + 30, 0), new Vector2(40, 40));
            toast.text = Ui.Text(rt, "Text", "", Fonts.Body, 22, Palette.Ivory, new Vector2(10, 0), ToastSize - new Vector2(70, 0));
            return toast;
        }

        public void Show(string message, Color? color = null)
        {
            if (string.IsNullOrEmpty(message))
                return;
            transform.SetAsLastSibling();
            text.text = message;
            text.color = color ?? Palette.Ivory;
            transform.localScale = Vector3.one * 0.9f;
            Tween.Scale(transform, Vector3.one, 0.3f, Ease.OutBack);
            Tween.Fade(group, 1f, 0.15f, Ease.OutQuad, () => Tween.Fade(group, 0f, 0.4f, Ease.OutQuad, null, VisibleSeconds));
        }
    }
}
