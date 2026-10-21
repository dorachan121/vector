using System;
using TMPro;
using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>ロビーでゲームを選ぶカード（ゲーム名・ジャンル・説明・テーブルの条件・着席ボタン）。</summary>
    public static class GameCard
    {
        public static readonly Vector2 Size = new Vector2(540, 206);

        public static RectTransform Create(Transform parent, Vector2 position, GameCatalog.Entry entry, params (string label, Action onClick)[] buttons)
        {
            var rt = Ui.Rect(parent, "Game" + entry.Title, position, Size);
            Ui.Image(rt, "Glow", Art.Glow, entry.Accent.WithAlpha(0.14f), Vector2.zero, Size * 1.3f);
            Ui.Panel(rt, "Plate", Vector2.zero, Size, new Color(0.03f, 0.06f, 0.05f, 0.92f), 22);
            Ui.Outline(rt, "Outline", Vector2.zero, Size, entry.Accent.WithAlpha(0.8f), 22);
            float left = -Size.x / 2 + 26;
            var title = Ui.Text(rt, "Title", entry.Title, Fonts.Display, 44, entry.Accent, new Vector2(left + 200, 64), new Vector2(400, 56), TextAlignmentOptions.Left);
            title.characterSpacing = 4;
            Ui.Text(rt, "Genre", entry.Genre, Fonts.Label, 18, Theme.Muted, new Vector2(Size.x / 2 - 120, 64), new Vector2(200, 26), TextAlignmentOptions.Right);
            Ui.Paragraph(rt, "Pitch", entry.Pitch, 20, Theme.Ivory, new Vector2(0, 2), new Vector2(Size.x - 52, 64));
            Ui.Text(rt, "Terms", entry.Terms, Fonts.Label, 16, Theme.Muted, new Vector2(left + 250, -52), new Vector2(500, 24), TextAlignmentOptions.Left);
            float width = buttons.Length > 1 ? 150 : 190;
            for (int i = 0; i < buttons.Length; i++)
            {
                float x = Size.x / 2 - 22 - width / 2 - (buttons.Length - 1 - i) * (width + 12);
                var (label, onClick) = buttons[i];
                Ui.Button(rt, "Sit" + i, label, new Vector2(x, -78), new Vector2(width, 48), entry.Accent, Theme.Ink, onClick, 22, 20);
            }
            return rt;
        }
    }
}
