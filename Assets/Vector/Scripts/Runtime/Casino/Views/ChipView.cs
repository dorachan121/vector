using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>1枚のチップ。</summary>
    public sealed class ChipView : MonoBehaviour
    {
        Image body;
        TextMeshProUGUI label;

        public long Value { get; private set; }
        public RectTransform Rect => (RectTransform)transform;

        public static ChipView Create(Transform parent, long value, float diameter, Vector2 position)
        {
            var rt = Ui.Rect(parent, "Chip" + value, position, new Vector2(diameter, diameter));
            var view = rt.gameObject.AddComponent<ChipView>();
            Ui.Image(rt, "Shadow", Art.Circle, new Color(0, 0, 0, 0.45f), new Vector2(0, -diameter * 0.05f), Vector2.one * diameter * 1.02f);
            view.body = Ui.Image(rt, "Body", null, Color.white, Vector2.zero, Vector2.one * diameter);
            view.label = Ui.Text(rt, "Value", "", Fonts.Label, diameter * 0.3f, Color.white, Vector2.zero, Vector2.one * diameter);
            view.SetValue(value);
            return view;
        }

        public void SetValue(long value)
        {
            Value = value;
            body.sprite = Art.Chip(ChipSet.Body(value), ChipSet.Accent(value));
            label.text = Labels.ChipValue(value);
            label.color = ChipSet.LabelColor(value);
        }
    }
}
