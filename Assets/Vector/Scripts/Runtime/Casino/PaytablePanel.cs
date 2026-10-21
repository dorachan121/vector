using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>配当表のパネル（見出し・行・注記）。精算時は成立した行を光らせる。</summary>
    public sealed class PaytablePanel : MonoBehaviour
    {
        const float RowHeight = 27f;

        readonly Dictionary<string, Image> rows = new Dictionary<string, Image>();
        RectTransform body;
        float cursor;
        float width;

        public static PaytablePanel Create(Transform parent, Vector2 position, Vector2 size, string name = "Paytable")
        {
            var rt = Ui.Rect(parent, name, position, size);
            var view = rt.gameObject.AddComponent<PaytablePanel>();
            Ui.Panel(rt, "Plate", Vector2.zero, size, Theme.Panel, 20);
            Ui.Outline(rt, "Outline", Vector2.zero, size, Theme.PanelLine, 20);
            view.body = Ui.Rect(rt, "Body", Vector2.zero, size);
            view.width = size.x - 40;
            view.cursor = size.y / 2 - 30;
            return view;
        }

        public void Heading(string text)
        {
            Ui.Text(body, "Heading", text, Fonts.Label, 22, Theme.Gold, new Vector2(0, cursor), new Vector2(width, 30));
            cursor -= 34;
        }

        public void Row(string key, string label, string pays)
        {
            var highlight = Ui.Panel(body, "Row" + key, new Vector2(0, cursor), new Vector2(width + 16, RowHeight), Color.clear, 8);
            Ui.Text(highlight.transform, "Label", label, Fonts.Label, 18, Theme.Ivory, new Vector2(-width / 2 + 4, 0), new Vector2(width, RowHeight), TextAlignmentOptions.Left)
                .rectTransform.pivot = new Vector2(0, 0.5f);
            Ui.Text(highlight.transform, "Pays", pays, Fonts.Label, 18, Theme.GoldBright, new Vector2(width / 2 - 4, 0), new Vector2(width, RowHeight), TextAlignmentOptions.Right)
                .rectTransform.pivot = new Vector2(1, 0.5f);
            rows[key] = highlight;
            cursor -= RowHeight;
        }

        public void Note(string text)
        {
            Ui.Text(body, "Note", text, Fonts.Body, 16, Theme.Muted, new Vector2(0, cursor - 2), new Vector2(width, 22));
            cursor -= 26;
        }

        public void Gap(float height = 10f) => cursor -= height;

        public void Highlight(string key)
        {
            if (rows.TryGetValue(key, out var row))
                Tween.Tint(row, Theme.Gold.WithAlpha(0.3f), 0.3f);
        }

        public void ClearHighlights()
        {
            foreach (var row in rows.Values)
                Tween.Tint(row, Color.clear, 0.3f);
        }
    }
}
