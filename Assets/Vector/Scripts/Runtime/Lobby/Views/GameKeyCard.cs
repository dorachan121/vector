using TMPro;
using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ゲームを選ぶ大きなカード（キービジュアル・ジャンル・RTP の札・名前・遊び方・特徴の札・テーブルの条件）。カルーセルに並べる。
    /// </summary>
    public static class GameKeyCard
    {
        public static readonly Vector2 Size = new Vector2(800, 1020);
        static readonly Vector2 ArtSize = new Vector2(Size.x - 32, 540);

        public static RectTransform Create(Transform parent, GameEntry entry)
        {
            var rt = Ui.Card(parent, "Card" + entry.Title, Vector2.zero, Size, Color.Lerp(entry.Accent, Palette.NightDeep, 0.72f), 36);
            var artCenter = new Vector2(0, Size.y / 2 - 16 - ArtSize.y / 2);
            var art = Ui.Clip(rt, "Art", artCenter, ArtSize);
            Ui.Image(art, "Sky", Chrome.Gradient(Color.Lerp(entry.Accent, Palette.Violet, 0.35f), Palette.NightDeep), Color.white, Vector2.zero, ArtSize);
            var rays = Ui.Image(art, "Rays", Chrome.Rays(16), entry.Accent.WithAlpha(0.3f), new Vector2(0, -20), Vector2.one * 980);
            Spin.Add(rays, 7f);
            Ui.Image(art, "Glow", Art.Glow, entry.Accent.WithAlpha(0.7f), new Vector2(0, -20), new Vector2(700, 520));
            var emblem = GameEmblem.Create(art, new Vector2(0, -30), 400, entry.Id);
            Float.Add(emblem, 8f, 0.8f);
            var twinkles = Ui.Rect(art, "Twinkles", Vector2.zero, ArtSize);
            Twinkle.Add(twinkles, 8, Palette.GoldLight, 40);
            Ui.Sliced(rt, "ArtRim", Chrome.Rim, entry.Accent.WithAlpha(0.8f), artCenter, ArtSize, 26);

            Pill(rt, entry.Genre, new Vector2(-Size.x / 2 + 150, Size.y / 2 - 52), 230, entry.Accent);
            var rtp = Ui.Rect(rt, "Rtp", new Vector2(Size.x / 2 - 96, Size.y / 2 - 86), Vector2.one * 140);
            Ui.Shadow(rtp, "Shadow", new Vector2(0, -5), Vector2.one * 140, 70, 0.6f);
            Ui.Sliced(rtp, "Face", Chrome.Gloss, Palette.Emerald, Vector2.zero, Vector2.one * 140, 70);
            Ui.Sliced(rtp, "Rim", Chrome.Rim, Palette.GoldLight, Vector2.zero, Vector2.one * 140, 70);
            Ui.Text(rtp, "Label", "RTP", Fonts.Label, 22, Palette.Ink, new Vector2(0, 28), new Vector2(120, 26));
            var value = Ui.Text(rtp, "Value", entry.Rtp, Fonts.Display, 34, Palette.Ivory, new Vector2(0, -10), new Vector2(124, 44));
            value.enableAutoSizing = true;
            value.fontSizeMin = 20;
            value.fontSizeMax = 34;
            Ui.Outline(value, Palette.Ink, 0.24f);

            float y = Size.y / 2 - 16 - ArtSize.y;
            var title = Ui.Headline(rt, "Title", entry.Title, Fonts.Display, 100, new Vector2(0, y - 72), new Vector2(Size.x - 40, 116));
            title.characterSpacing = 4;
            Ui.Paragraph(rt, "Pitch", entry.Pitch, 28, Palette.Ivory, new Vector2(0, y - 186), new Vector2(Size.x - 80, 92), TextAlignmentOptions.Top);
            for (int i = 0; i < entry.Features.Length; i++)
            {
                float x = (i - (entry.Features.Length - 1) / 2f) * 230;
                Ui.Sliced(rt, "Feature", Chrome.Shade, Palette.NightDeep.WithAlpha(0.9f), new Vector2(x, y - 290), new Vector2(214, 52), 26);
                Ui.Sliced(rt, "FeatureRim", Chrome.Rim, Palette.GoldMid.WithAlpha(0.8f), new Vector2(x, y - 290), new Vector2(214, 52), 26);
                Ui.Text(rt, "FeatureText", entry.Features[i], Fonts.Body, 24, Palette.GoldLight, new Vector2(x, y - 289), new Vector2(200, 34));
            }
            Ui.Text(rt, "Terms", entry.Terms, Fonts.Label, 22, Palette.Muted, new Vector2(0, y - 368), new Vector2(Size.x - 40, 30));
            return rt;
        }

        static void Pill(RectTransform parent, string text, Vector2 position, float width, Color color)
        {
            var size = new Vector2(width, 52);
            Ui.Shadow(parent, "PillShadow", position + new Vector2(0, -3), size, 26, 0.5f);
            Ui.Sliced(parent, "Pill", Chrome.Gloss, Color.Lerp(color, Palette.NightDeep, 0.25f), position, size, 26);
            Ui.Sliced(parent, "PillRim", Chrome.Rim, Palette.GoldLight.WithAlpha(0.8f), position, size, 26);
            Ui.Outline(Ui.Text(parent, "PillText", text, Fonts.Label, 26, Palette.Ivory, position + new Vector2(0, 1), size), Palette.Ink, 0.24f);
        }
    }
}
