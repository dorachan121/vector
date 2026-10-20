using System;
using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>ロビーで席を選ぶカード（席名・説明・配当の要約・着席ボタン）。</summary>
    public static class SeatCard
    {
        static readonly Vector2 CardSize = new Vector2(600, 330);

        public static CasinoButton Create(Transform parent, Vector2 position, string title, string description, string terms, string buttonLabel, Color accent, Action onSit)
        {
            var rt = Ui.Rect(parent, "Seat" + title, position, CardSize);
            Ui.Image(rt, "Glow", Art.Glow, accent.WithAlpha(0.18f), Vector2.zero, CardSize * 1.4f);
            Ui.Panel(rt, "Plate", Vector2.zero, CardSize, new Color(0.03f, 0.07f, 0.05f, 0.92f), 26);
            Ui.Outline(rt, "Outline", Vector2.zero, CardSize, accent.WithAlpha(0.85f), 26);
            Ui.Text(rt, "Title", title, Fonts.Display, 56, accent, new Vector2(0, 110), new Vector2(CardSize.x, 70));
            Ui.Paragraph(rt, "Description", description, 24, Theme.Ivory, new Vector2(0, 30), new Vector2(CardSize.x - 80, 80), TMPro.TextAlignmentOptions.Top);
            Ui.Text(rt, "Terms", terms, Fonts.Label, 20, Theme.Muted, new Vector2(0, -40), new Vector2(CardSize.x - 40, 28));
            return Ui.Button(rt, "Sit", buttonLabel, new Vector2(0, -105), new Vector2(300, 70), accent, Theme.Ink, onSit, 32, 35);
        }
    }
}
