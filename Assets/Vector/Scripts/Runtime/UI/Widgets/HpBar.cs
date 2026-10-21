using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vector.Game.UI.Widgets
{
    /// <summary>体力などのゲージ。減ると白い残像が遅れて追いかける。</summary>
    public sealed class HpBar : MonoBehaviour
    {
        Image fill;
        Image trail;
        TextMeshProUGUI label;
        string caption;
        Color color;

        public static HpBar Create(Transform parent, string name, Vector2 position, float width, Color color, string caption = "HP")
        {
            const float height = 30f;
            var rt = Ui.Rect(parent, name, position, new Vector2(width, height));
            var bar = rt.gameObject.AddComponent<HpBar>();
            bar.caption = caption;
            bar.color = color;
            Ui.Panel(rt, "Back", Vector2.zero, new Vector2(width + 8, height + 8), new Color(0, 0, 0, 0.7f), 19);
            bar.trail = Ui.Panel(rt, "Trail", Vector2.zero, new Vector2(width, height), Theme.Ivory.WithAlpha(0.7f), 15);
            bar.fill = Ui.Panel(rt, "Fill", Vector2.zero, new Vector2(width, height), color, 15);
            foreach (var image in new[] { bar.trail, bar.fill })
            {
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Horizontal;
                image.fillAmount = 1;
            }
            Ui.Outline(rt, "Outline", Vector2.zero, new Vector2(width + 8, height + 8), Theme.Gold.WithAlpha(0.6f), 19);
            bar.label = Ui.Text(rt, "Label", "", Fonts.Label, 22, Theme.Ivory, Vector2.zero, new Vector2(width, height));
            return bar;
        }

        public void Set(int current, int max, bool animate)
        {
            float t = max > 0 ? Mathf.Clamp01(current / (float)max) : 0;
            label.text = $"{caption}  {Mathf.Max(0, current)} / {max}";
            fill.color = t <= 0.25f ? Theme.Negative : color;
            if (!animate)
            {
                Tween.Stop(fill);
                Tween.Stop(trail);
                fill.fillAmount = trail.fillAmount = t;
                return;
            }
            float fromFill = fill.fillAmount, fromTrail = trail.fillAmount;
            Tween.Run(fill, "fill", 0.25f, Ease.OutCubic, k => fill.fillAmount = Mathf.Lerp(fromFill, t, k));
            Tween.Run(trail, "fill", 0.6f, Ease.InOutQuad, k => trail.fillAmount = Mathf.Lerp(fromTrail, t, k), null, 0.25f);
        }
    }
}
