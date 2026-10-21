using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ホームの背景（カジノのロビー）。奥の壁のアーチ、シャンデリアの光、ぼやけた灯り、左右のスロット台、手前へ伸びる赤い絨毯。
    /// 縦に長い端末や横に広い PC のウィンドウでも途切れないよう、基準の画面より大きく描く。
    /// </summary>
    public static class HomeBackdrop
    {
        /// <summary>地平（奥の壁と床の境目）の高さ。</summary>
        const float Horizon = -60f;

        public static RectTransform Create(Transform parent)
        {
            var rt = Ui.Stretch(parent, "Backdrop");
            Ui.Image(rt, "Sky", Chrome.Gradient(Theme.Hex("3C1A7E"), Theme.Hex("120630")), Color.white, new Vector2(0, 700), new Vector2(3400, 1600));
            Ui.Image(rt, "Floor", Chrome.Gradient(Theme.Hex("1C0B3A"), Theme.Hex("04020A")), Color.white, new Vector2(0, Horizon - 800), new Vector2(3400, 1600));
            Arches(rt);
            Chandelier(rt, new Vector2(0, 780));
            Bokeh(rt);
            // 左の丸いメニューの列と重ならないよう、台は床の手前（メニューの下）に置く
            SlotMachine(rt, new Vector2(-440, Horizon - 130), false);
            SlotMachine(rt, new Vector2(440, Horizon - 130), true);
            Ui.Image(rt, "HorizonGlow", Art.Glow, Palette.Gold.WithAlpha(0.28f), new Vector2(0, Horizon), new Vector2(2200, 140));
            // 手前へ伸びる絨毯（金の縁取りの上に赤）
            Ui.Image(rt, "CarpetEdge", Chrome.Trapezoid, Palette.GoldMid.WithAlpha(0.85f), new Vector2(0, Horizon - 620), new Vector2(1320, 1240));
            Ui.Image(rt, "Carpet", Chrome.Trapezoid, Theme.Hex("8E1B2C"), new Vector2(0, Horizon - 624), new Vector2(1250, 1236));
            Ui.Image(rt, "CarpetShine", Chrome.Gradient(new Color(1, 1, 1, 0f), new Color(1, 1, 1, 0.12f)), Color.white, new Vector2(0, Horizon - 400), new Vector2(700, 700));
            var twinkles = Ui.Rect(rt, "Twinkles", new Vector2(0, 500), new Vector2(1100, 900));
            Twinkle.Add(twinkles, 16, Palette.GoldLight, 46);
            return rt;
        }

        static void Arches(RectTransform rt)
        {
            for (int i = -3; i <= 3; i++)
            {
                var at = new Vector2(i * 330, Horizon + 420);
                Ui.Image(rt, "ArchRim", Chrome.Arch, Palette.GoldMid.WithAlpha(0.55f), at, new Vector2(300, 600));
                Ui.Image(rt, "Arch", Chrome.Arch, Theme.Hex("2A1060"), at + new Vector2(0, -6), new Vector2(284, 584));
                Ui.Image(rt, "Window", Chrome.Arch, Theme.Hex("5B2A9E").WithAlpha(0.7f), at + new Vector2(0, 30), new Vector2(200, 400));
                Ui.Image(rt, "Light", Art.Glow, Palette.Magenta.WithAlpha(0.25f), at + new Vector2(0, 150), new Vector2(260, 260));
            }
        }

        static void Chandelier(RectTransform rt, Vector2 at)
        {
            Ui.Image(rt, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.5f), at, new Vector2(1100, 560));
            Ui.Image(rt, "Core", Art.Glow, Color.white.WithAlpha(0.55f), at, new Vector2(360, 200));
            for (int i = 0; i < 13; i++)
            {
                float t = i / 12f;
                var p = at + new Vector2(Mathf.Lerp(-300, 300, t), -90 + Mathf.Sin(t * Mathf.PI) * -70);
                Ui.Image(rt, "Crystal", Art.Gem(4), Palette.GoldLight, p, Vector2.one * (16 + 10 * Mathf.Sin(t * Mathf.PI)));
            }
            var spark = Ui.Image(rt, "Sparkle", Chrome.Sparkle, Color.white, at + new Vector2(0, -20), Vector2.one * 170);
            Pulse.Add(spark, 0.12f, 1.4f);
        }

        /// <summary>ぼやけた灯り（毎回同じ配置になるよう、決まった種で並べる）。</summary>
        static void Bokeh(RectTransform rt)
        {
            var random = new System.Random(7);
            var colors = new[] { Palette.GoldLight, Palette.Magenta, Palette.Cyan, Palette.Amber };
            for (int i = 0; i < 26; i++)
            {
                var at = new Vector2((float)(random.NextDouble() - 0.5) * 1500, Horizon + 60 + (float)random.NextDouble() * 900);
                float size = 60 + (float)random.NextDouble() * 170;
                Ui.Image(rt, "Bokeh", Art.Glow, colors[i % colors.Length].WithAlpha(0.12f + (float)random.NextDouble() * 0.2f), at, Vector2.one * size);
            }
        }

        static void SlotMachine(RectTransform rt, Vector2 at, bool mirrored)
        {
            var cabinet = Ui.Rect(rt, "Slot", at, new Vector2(240, 400));
            cabinet.localScale = new Vector3(mirrored ? -1 : 1, 1, 1);
            Ui.Image(cabinet, "Glow", Art.Glow, Palette.Magenta.WithAlpha(0.35f), new Vector2(0, 120), new Vector2(360, 300));
            Ui.Panel(cabinet, "Body", Vector2.zero, new Vector2(240, 400), Theme.Hex("2C1356"), 36);
            Ui.Outline(cabinet, "Rim", Vector2.zero, new Vector2(240, 400), Palette.GoldMid.WithAlpha(0.8f), 36);
            Ui.Image(cabinet, "Screen", Chrome.Gradient(Palette.Cyan.WithAlpha(0.8f), Palette.Violet), Color.white, new Vector2(0, 70), new Vector2(180, 150));
            for (int i = -1; i <= 1; i++)
                Ui.Panel(cabinet, "Reel", new Vector2(i * 56, 70), new Vector2(46, 110), Palette.Ivory.WithAlpha(0.85f), 8);
            Ui.Image(cabinet, "Seven", Art.Gem(5), Palette.Ruby, new Vector2(0, 70), Vector2.one * 34);
            Ui.Panel(cabinet, "Tray", new Vector2(0, -110), new Vector2(160, 40), Palette.GoldDark, 12);
            Ui.Image(cabinet, "Topper", Art.Glow, Palette.GoldLight.WithAlpha(0.7f), new Vector2(0, 210), new Vector2(160, 80));
        }
    }
}
