using System.Collections.Generic;
using UnityEngine;

namespace Vector.Game.UI.Widgets
{
    /// <summary>遊び方の説明（ページ送り）。配当の数値は各ゲームのルールから生成する。</summary>
    public static class HelpDialog
    {
        public static Dialog Open(Transform root, IReadOnlyList<(string title, string body)> pages)
        {
            var dialog = Dialog.Open(root, "HOW TO PLAY", null, Ui.IsPortrait(root) ? new Vector2(1000, 1360) : new Vector2(1180, 820));
            var content = dialog.Content;
            var size = content.sizeDelta;

            var heading = Ui.Text(content, "Heading", "", Fonts.Label, 32, Theme.GoldBright, new Vector2(0, size.y / 2 - 24), new Vector2(size.x, 44));
            var body = Ui.Paragraph(content, "Body", "", 23, Theme.Ivory, new Vector2(0, -10), new Vector2(size.x - 80, size.y - 120));
            body.richText = true;
            var indicator = Ui.Text(content, "Page", "", Fonts.Label, 22, Theme.Muted, new Vector2(0, -size.y / 2 + 26), new Vector2(200, 30));

            int page = 0;
            CasinoButton prev = null, next = null;
            void Render()
            {
                heading.text = pages[page].title;
                body.text = pages[page].body;
                indicator.text = $"{page + 1} / {pages.Count}";
                prev.Interactable = page > 0;
                next.Interactable = page < pages.Count - 1;
            }
            var navColor = new Color(1, 1, 1, 0.1f);
            prev = Ui.Button(content, "Prev", "< PREV", new Vector2(-size.x / 2 + 90, -size.y / 2 + 26), new Vector2(160, 52), navColor, Theme.Ivory, () =>
            {
                page--;
                Render();
            }, 22);
            next = Ui.Button(content, "Next", "NEXT >", new Vector2(size.x / 2 - 90, -size.y / 2 + 26), new Vector2(160, 52), navColor, Theme.Ivory, () =>
            {
                page++;
                Render();
            }, 22);
            Render();
            dialog.AddButton("CLOSE", null, true);
            return dialog;
        }
    }
}
