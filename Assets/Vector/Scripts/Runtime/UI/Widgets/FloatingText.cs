using UnityEngine;

namespace Vector.Game.UI.Widgets
{
    /// <summary>ダメージや配当の数字を、その場に浮かび上がらせて消す。</summary>
    public static class FloatingText
    {
        public static void Show(Transform parent, string text, Color color, Vector2 position, float size = 64f, float rise = 90f, float seconds = 1.1f)
        {
            var label = Ui.Text(parent, "Float", text, Fonts.Display, size, color, position, new Vector2(600, size * 1.4f));
            label.outlineWidth = 0.18f;
            label.outlineColor = new Color(0, 0, 0, 0.85f);
            var rt = label.rectTransform;
            rt.localScale = Vector3.one * 0.6f;
            Tween.Scale(rt, Vector3.one, 0.22f, Ease.OutBack);
            Tween.Move(rt, position + new Vector2(0, rise), seconds, Ease.OutCubic);
            var group = Ui.Group(label.gameObject);
            Tween.Fade(group, 0f, 0.35f, Ease.OutQuad, () => Object.Destroy(label.gameObject), seconds - 0.35f);
        }

        /// <summary>画面（rt）を短く揺らす。強い一撃の演出。</summary>
        public static void Shake(RectTransform rt, float strength = 18f, float seconds = 0.35f)
        {
            var origin = rt.anchoredPosition;
            Tween.Run(rt, "shake", seconds, Ease.Linear, t =>
            {
                float decay = 1 - t;
                rt.anchoredPosition = origin + new Vector2(Mathf.Sin(t * 70f), Mathf.Cos(t * 53f)) * strength * decay;
            }, () => rt.anchoredPosition = origin);
        }
    }
}
