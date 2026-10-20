using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>配当表のパネル。精算時は成立した行を光らせる。</summary>
    public sealed class PaytableView : MonoBehaviour
    {
        const float RowHeight = 27f;

        readonly Dictionary<string, Image> rows = new Dictionary<string, Image>();
        RectTransform body;
        float cursor;
        float width;

        public static PaytableView Create(Transform parent, Vector2 position, Vector2 size, GameMode mode, TableRules rules)
        {
            var rt = Ui.Rect(parent, "Paytable", position, size);
            var view = rt.gameObject.AddComponent<PaytableView>();
            Ui.Panel(rt, "Plate", Vector2.zero, size, Theme.Panel, 20);
            Ui.Outline(rt, "Outline", Vector2.zero, size, Theme.PanelLine, 20);
            view.body = Ui.Rect(rt, "Body", Vector2.zero, size);
            view.width = size.x - 40;
            view.cursor = size.y / 2 - 30;

            view.Heading("PAYTABLE");
            if (mode == GameMode.Player)
            {
                view.Row("main", "ROW / COLUMN / SLASH", rules.MainPays.ToString());
                view.Row("push", "TIE", "PUSH");
                view.Note("ディーラーは残り2種類の高い方で勝負");
            }
            else
            {
                view.Row(nameof(BetSpot.Player), "PLAYER", rules.BackPlayerPays.ToString());
                view.Row(nameof(BetSpot.Dealer), "DEALER", rules.BackDealerPays.ToString());
                view.Row(nameof(BetSpot.Tie), "TIE", rules.BackTiePays.ToString());
                view.Note("PLAYER / DEALER は引き分けで返金");
            }
            view.cursor -= 10;
            view.Heading("BONUS  (VECTOR + HAND)");
            foreach (var pair in rules.BonusPays)
                view.Row(pair.Key.ToString(), Labels.HandName(pair.Key), pair.Value.ToString());
            return view;
        }

        void Heading(string text)
        {
            Ui.Text(body, "Heading", text, Fonts.Label, 22, Theme.Gold, new Vector2(0, cursor), new Vector2(width, 30));
            cursor -= 34;
        }

        void Row(string key, string label, string pays)
        {
            var highlight = Ui.Panel(body, "Row" + key, new Vector2(0, cursor), new Vector2(width + 16, RowHeight), Color.clear, 8);
            Ui.Text(highlight.transform, "Label", label, Fonts.Label, 18, Theme.Ivory, new Vector2(-width / 2 + 4, 0), new Vector2(width, RowHeight), TextAlignmentOptions.Left)
                .rectTransform.pivot = new Vector2(0, 0.5f);
            Ui.Text(highlight.transform, "Pays", pays, Fonts.Label, 18, Theme.GoldBright, new Vector2(width / 2 - 4, 0), new Vector2(width, RowHeight), TextAlignmentOptions.Right)
                .rectTransform.pivot = new Vector2(1, 0.5f);
            rows[key] = highlight;
            cursor -= RowHeight;
        }

        void Note(string text)
        {
            Ui.Text(body, "Note", text, Fonts.Body, 15, Theme.Muted, new Vector2(0, cursor - 2), new Vector2(width, 22));
            cursor -= 26;
        }

        public void HighlightBonus(PokerHand hand) => Highlight(hand.ToString());

        public void HighlightSpot(BetSpot spot) => Highlight(spot == BetSpot.Main ? "main" : spot.ToString());

        void Highlight(string key)
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
