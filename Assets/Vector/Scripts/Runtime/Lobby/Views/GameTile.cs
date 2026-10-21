using System;
using TMPro;
using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>ゲームを選ぶタイル（ゲームの絵・ジャンル・RTP・名前・遊び方の要約・着席ボタン）。縦画面で2列に並べる大きさ。</summary>
    public static class GameTile
    {
        public static readonly Vector2 Size = new Vector2(500, 440);

        public static RectTransform Create(Transform parent, Vector2 position, GameEntry entry, params (string label, Action onClick)[] buttons)
        {
            var rt = Ui.Card(parent, "Game" + entry.Title, position, Size, Palette.PanelTop, 28);

            // 絵の区画: ゲームの色の地・光・ゆっくり回る光の放射・紋章
            var art = new Vector2(Size.x - 28, 212);
            var artCenter = new Vector2(0, Size.y / 2 - 14 - art.y / 2);
            Ui.Sliced(rt, "Art", Chrome.Shade, Color.Lerp(entry.Accent, Palette.NightDeep, 0.55f), artCenter, art, 20);
            Ui.Image(rt, "Glow", Art.Glow, entry.Accent.WithAlpha(0.55f), artCenter, new Vector2(art.x * 0.8f, art.y * 1.3f));
            var rays = Ui.Image(rt, "Rays", Chrome.Rays(14), entry.Accent.WithAlpha(0.16f), artCenter, Vector2.one * 380);
            Spin.Add(rays, 8f);
            GameEmblem.Create(rt, artCenter + new Vector2(0, -6), 184, entry.Id);
            Pill(rt, entry.Genre, new Vector2(-Size.x / 2 + 104, Size.y / 2 - 38), 168, entry.Accent);
            Pill(rt, "RTP " + entry.Rtp, new Vector2(Size.x / 2 - 92, Size.y / 2 - 38), 148, Palette.Emerald);

            var title = Ui.Headline(rt, "Title", entry.Title, Fonts.Display, 46, new Vector2(0, -34), new Vector2(Size.x - 40, 56), TextAlignmentOptions.Left);
            title.characterSpacing = 3;
            Ui.Paragraph(rt, "Pitch", entry.Pitch, 19, Palette.Muted, new Vector2(0, -100), new Vector2(Size.x - 44, 64));

            const float y = -170, height = 70;
            float width = buttons.Length > 1 ? (Size.x - 56 - 12) / 2 : Size.x - 56;
            for (int i = 0; i < buttons.Length; i++)
            {
                var (label, onClick) = buttons[i];
                float x = (i - (buttons.Length - 1) / 2f) * (width + 12);
                bool primary = i == 0;
                Ui.Button(rt, "Play" + i, label, new Vector2(x, y), new Vector2(width, height), primary ? entry.Accent : new Color(1, 1, 1, 0.1f),
                    primary ? Palette.Ink : Palette.Ivory, onClick, buttons.Length > 1 ? 26 : 32, 22);
            }
            return rt;
        }

        static void Pill(RectTransform parent, string text, Vector2 position, float width, Color color)
        {
            var size = new Vector2(width, 38);
            Ui.Sliced(parent, "Pill", Chrome.Shade, Palette.NightDeep.WithAlpha(0.85f), position, size, 19);
            Ui.Sliced(parent, "PillRim", Chrome.Rim, color.WithAlpha(0.9f), position, size, 19);
            Ui.Text(parent, "PillText", text, Fonts.Label, 19, color, position + new Vector2(0, 1), size);
        }
    }
}
