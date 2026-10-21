using UnityEngine;
using UnityEngine.UI;

namespace Vector.Game.UI.Widgets
{
    /// <summary>光沢のあるゲージ（経験値・進み具合・体力）。値が変わるときは滑らかに伸び縮みする。</summary>
    public sealed class Meter : MonoBehaviour
    {
        Image fill;
        float shown = -1;

        public static Meter Create(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var rt = Ui.Rect(parent, name, position, size);
            var meter = rt.gameObject.AddComponent<Meter>();
            float radius = size.y / 2;
            Ui.Sliced(rt, "Track", Chrome.Shade, Palette.NightDeep.WithAlpha(0.95f), Vector2.zero, size, radius);
            var inner = size - new Vector2(6, 6);
            meter.fill = Ui.Sliced(rt, "Fill", Chrome.Gloss, color, Vector2.zero, inner, inner.y / 2);
            meter.fill.type = Image.Type.Filled;
            meter.fill.fillMethod = Image.FillMethod.Horizontal;
            Ui.Sliced(rt, "Rim", Chrome.Rim, Palette.GoldMid.WithAlpha(0.6f), Vector2.zero, size, radius);
            return meter;
        }

        public void SetFraction(float fraction, bool animate = true)
        {
            fraction = Mathf.Clamp01(fraction);
            float from = shown < 0 ? fraction : shown;
            shown = fraction;
            if (!animate || Mathf.Approximately(from, fraction))
            {
                fill.fillAmount = fraction;
                return;
            }
            Tween.Run(fill, "fill", 0.6f, Ease.OutCubic, t => fill.fillAmount = Mathf.LerpUnclamped(from, fraction, t));
        }
    }
}
