using TMPro;
using UnityEngine;

namespace Vector.Game.UI.Widgets
{
    /// <summary>通知の赤い丸（件数や「!」）。0 件なら隠れ、件数があるあいだは小さく脈打つ。</summary>
    public sealed class Badge : MonoBehaviour
    {
        TextMeshProUGUI label;

        public static Badge Create(Transform parent, Vector2 position, float size = 34f)
        {
            var rt = Ui.Rect(parent, "Badge", position, Vector2.one * size);
            var badge = rt.gameObject.AddComponent<Badge>();
            Ui.Shadow(rt, "Shadow", new Vector2(0, -2), Vector2.one * size, size / 2, 0.5f);
            Ui.Sliced(rt, "Face", Chrome.Gloss, Palette.Ruby, Vector2.zero, Vector2.one * size, size / 2);
            Ui.Sliced(rt, "Rim", Chrome.Rim, Palette.Ivory, Vector2.zero, Vector2.one * size, size / 2);
            badge.label = Ui.Text(rt, "Count", "!", Fonts.Label, size * 0.6f, Palette.Ivory, new Vector2(0, 1), Vector2.one * size);
            Pulse.Add(badge, 0.12f, 4.5f);
            return badge;
        }

        /// <summary>件数を出す（0 で隠す）。</summary>
        public void SetCount(int count)
        {
            gameObject.SetActive(count > 0);
            label.text = count > 9 ? "9+" : count.ToString();
        }

        /// <summary>件数ではなく「!」で知らせる。</summary>
        public void SetAlert(bool on)
        {
            gameObject.SetActive(on);
            label.text = "!";
        }
    }
}
