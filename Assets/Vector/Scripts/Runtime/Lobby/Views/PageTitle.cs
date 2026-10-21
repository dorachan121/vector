using TMPro;
using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>タブの画面の見出し（英字の大きな見出しと、日本語の名前の帯）。</summary>
    public static class PageTitle
    {
        public static void Create(Transform parent, float y, string english, string japanese)
        {
            var rt = Ui.Rect(parent, "PageTitle", new Vector2(0, y), new Vector2(Ui.Portrait.x, 110));
            Ui.Image(rt, "Band", Chrome.Gradient(Palette.Violet.WithAlpha(0.85f), Palette.NightDeep.WithAlpha(0.85f)), Color.white, Vector2.zero, new Vector2(Ui.Portrait.x * 3, 96));
            Ui.Image(rt, "Line", null, Palette.GoldMid, new Vector2(0, -48), new Vector2(Ui.Portrait.x * 3, 3));
            Ui.Image(rt, "Glow", Art.Glow, Palette.Gold.WithAlpha(0.3f), new Vector2(-300, 0), new Vector2(700, 140));
            float left = -Ui.Portrait.x / 2 + 48;
            var head = Ui.Headline(rt, "English", english, Fonts.Display, 64, new Vector2(left + 220, 2), new Vector2(440, 80), TextAlignmentOptions.Left);
            head.characterSpacing = 4;
            float width = head.GetPreferredValues(english).x + head.characterSpacing * english.Length;
            Ui.Text(rt, "Japanese", japanese, Fonts.Body, 28, Palette.Ivory, new Vector2(left + width + 24 + 200, -4), new Vector2(400, 40), TextAlignmentOptions.Left);
        }
    }
}
