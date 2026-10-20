using TMPro;
using UnityEngine;

namespace Vector.Game.UI.Widgets
{
    /// <summary>画面上部に短く出て消える通知。新しい通知は前の通知を置き換える。</summary>
    public sealed class Toast : MonoBehaviour
    {
        const float VisibleSeconds = 2.4f;
        static readonly Vector2 ToastSize = new Vector2(760, 52);

        CanvasGroup group;
        TextMeshProUGUI text;

        public static Toast Create(Transform root, Vector2 position)
        {
            var rt = Ui.Rect(root, "Toast", position, ToastSize);
            var toast = rt.gameObject.AddComponent<Toast>();
            toast.group = Ui.Group(rt.gameObject);
            toast.group.blocksRaycasts = false;
            toast.group.alpha = 0;
            Ui.Panel(rt, "Plate", Vector2.zero, ToastSize, new Color(0.04f, 0.04f, 0.05f, 0.92f), 26);
            Ui.Outline(rt, "Outline", Vector2.zero, ToastSize, Theme.Gold.WithAlpha(0.6f), 26);
            toast.text = Ui.Text(rt, "Text", "", Fonts.Body, 22, Theme.Ivory, Vector2.zero, ToastSize - new Vector2(30, 0));
            return toast;
        }

        public void Show(string message, Color? color = null)
        {
            if (string.IsNullOrEmpty(message))
                return;
            transform.SetAsLastSibling();
            text.text = message;
            text.color = color ?? Theme.Ivory;
            Tween.Fade(group, 1f, 0.15f, Ease.OutQuad, () => Tween.Fade(group, 0f, 0.4f, Ease.OutQuad, null, VisibleSeconds));
        }
    }
}
