using TMPro;
using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>キャラクターの吹き出し（白い角丸と、話し手のほうへ向く三角のしっぽ）。言うたびに軽く弾む。</summary>
    public sealed class SpeechBubble : MonoBehaviour
    {
        TextMeshProUGUI text;

        /// <param name="parent">置く親。</param>
        /// <param name="position">吹き出しの中心。</param>
        /// <param name="size">吹き出しの大きさ。</param>
        /// <param name="tail">しっぽの先の位置（吹き出しの中心から）。吹き出しの外を指す。</param>
        public static SpeechBubble Create(Transform parent, Vector2 position, Vector2 size, Vector2 tail)
        {
            var rt = Ui.Rect(parent, "Bubble", position, size);
            var bubble = rt.gameObject.AddComponent<SpeechBubble>();
            Ui.Shadow(rt, "Shadow", new Vector2(0, -6), size, 30, 0.5f);
            // しっぽの根元は吹き出しの縁より少し内側（本体に隠れる）から、先は tail まで
            float inside = Mathf.Min(size.x / 2 / Mathf.Max(1, Mathf.Abs(tail.x)), size.y / 2 / Mathf.Max(1, Mathf.Abs(tail.y))) * 0.8f;
            var root = tail * Mathf.Min(1, inside);
            var point = Ui.Image(rt, "Tail", PortraitArt.Triangle, Palette.Ivory, (root + tail) / 2, new Vector2(46, (tail - root).magnitude));
            point.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(tail.y, tail.x) * Mathf.Rad2Deg - 90);
            Ui.Panel(rt, "Body", Vector2.zero, size, Palette.Ivory, 30);
            Ui.Outline(rt, "Rim", Vector2.zero, size, Palette.GoldMid.WithAlpha(0.7f), 30);
            bubble.text = Ui.Paragraph(rt, "Text", "", 25, Palette.Ink, Vector2.zero, size - new Vector2(56, 36), TextAlignmentOptions.Center);
            return bubble;
        }

        public void Say(string line)
        {
            text.text = line;
            transform.localScale = Vector3.one * 0.85f;
            Tween.Scale(transform, Vector3.one, 0.35f, Ease.OutBack);
        }
    }
}
