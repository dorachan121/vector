using UnityEngine;
using static Vector.Game.UI.Sdf;
using static Vector.Game.UI.SpriteFactory;

namespace Vector.Game.UI
{
    /// <summary>
    /// 画面の枠まわりの装飾スプライト（光沢・陰影・柔らかい影・金の額縁・グラデーション・光の放射・きらめき・リボン）。
    /// 陰影は白の明るさで描いてあり、Image.color の色を掛けると、その色の立体的な部品になる。
    /// 角丸のものは半径 <see cref="Radius"/> の9スライスで、表示上の半径は pixelsPerUnitMultiplier で変える（<see cref="Ui"/> の各ヘルパー）。
    /// </summary>
    public static class Chrome
    {
        /// <summary>角丸の元画像の半径（px）。</summary>
        public const int Radius = 32;

        /// <summary>柔らかい影のぼかし幅（元画像の px）。</summary>
        public const int ShadowBlur = 16;

        const int Border = Radius + 2;

        /// <summary>ボタンの地。上が明るく下が暗い光沢と、下の縁の陰。</summary>
        public static Sprite Gloss => Cached("chrome.gloss", () => Paint(72, 128, (x, y) =>
        {
            float d = SdRoundBox(x - 36, y - 64, 35, 63, Radius);
            float t = y / 128f;
            float l = Mathf.Lerp(0.66f, 1f, Smooth(0f, 1f, t));
            float inside = -d;
            if (inside < 5 && t < 0.5f)
                l *= Mathf.Lerp(0.72f, 1f, inside / 5f);
            return new Color(l, l, l, Coverage(d));
        }, Border));

        /// <summary>ボタンの上半分に重ねる艶（白の半透明）。</summary>
        public static Sprite Shine => Cached("chrome.shine", () => Paint(72, 128, (x, y) =>
        {
            float d = SdRoundBox(x - 36, y - 64, 32, 60, Radius - 3);
            float t = y / 128f;
            float g = Smooth(0.5f, 0.58f, t) * Mathf.Lerp(0.14f, 0.42f, Mathf.Clamp01((t - 0.5f) / 0.5f));
            return new Color(1, 1, 1, Coverage(d) * g);
        }, Border));

        /// <summary>パネルやカードの地。上から下へなだらかに暗くなる。</summary>
        public static Sprite Shade => Cached("chrome.shade", () => Paint(72, 128, (x, y) =>
        {
            float d = SdRoundBox(x - 36, y - 64, 35, 63, Radius);
            float l = Mathf.Lerp(0.55f, 1f, Smooth(0f, 1f, y / 128f));
            return new Color(l, l, l, Coverage(d));
        }, Border));

        /// <summary>柔らかい影（黒）。中身の大きさにぼかし幅を足して置く（<see cref="Ui.Shadow"/>）。</summary>
        public static Sprite SoftShadow => Cached("chrome.shadow", () => Paint(128, 128, (x, y) =>
        {
            float d = SdRoundBox(x - 64, y - 64, 64 - ShadowBlur, 64 - ShadowBlur, Radius);
            float a = 1f - Smooth(-ShadowBlur * 0.5f, ShadowBlur, d);
            return new Color(0, 0, 0, a * a);
        }, Radius + ShadowBlur));

        /// <summary>金の額縁（角丸の輪）。断面に丸みがあり、上が明るい。金色を掛けて使う。</summary>
        public static Sprite Frame => Cached("chrome.frame", () => Paint(128, 128, (x, y) =>
        {
            const float thickness = 7f;
            float d = SdRoundBox(x - 64, y - 64, 63, 63, Radius);
            float ring = Mathf.Abs(d + thickness / 2) - thickness / 2;
            float s = Mathf.Clamp01(-d / thickness);
            float t = y / 128f;
            float l = Mathf.Lerp(0.5f, 1f, t) * (0.7f + 0.3f * Mathf.Sin(s * Mathf.PI));
            l = Mathf.Max(l, Mathf.Clamp01(1 - Mathf.Abs(s - 0.32f) * 7f) * Mathf.Lerp(0.7f, 1f, t));
            return new Color(l, l, l, Coverage(ring));
        }, Border));

        /// <summary>細い縁取り（内側に光の線）。ガラスの縁や選択中の枠。</summary>
        public static Sprite Rim => Cached("chrome.rim", () => Paint(72, 128, (x, y) =>
        {
            float d = SdRoundBox(x - 36, y - 64, 35, 63, Radius);
            float ring = Mathf.Abs(d + 1.5f) - 1.5f;
            float l = Mathf.Lerp(0.55f, 1f, y / 128f);
            return new Color(l, l, l, Coverage(ring));
        }, Border));

        /// <summary>上下2色の縦グラデーション（伸ばして背景に使う）。</summary>
        public static Sprite Gradient(Color top, Color bottom) => Cached($"chrome.grad{Key(top)}{Key(bottom)}", () =>
            Paint(4, 256, (x, y) => Color.Lerp(bottom, top, Smooth(0f, 1f, y / 256f))));

        /// <summary>光の放射（count 本）。報酬の演出やタイトルの後ろでゆっくり回す。</summary>
        public static Sprite Rays(int count) => Cached($"chrome.rays{count}", () => Paint(512, 512, (x, y) =>
        {
            float u = (x - 256) / 256f, v = (y - 256) / 256f;
            float r = Mathf.Sqrt(u * u + v * v);
            float ray = Smooth(0.15f, 0.55f, Mathf.Cos(Mathf.Atan2(v, u) * count));
            float fade = Mathf.Clamp01(1 - r);
            float core = Mathf.Clamp01(1 - r * 2.4f);
            return new Color(1, 1, 1, Mathf.Max(ray * fade * fade, core * core * 0.7f));
        }));

        /// <summary>十字のきらめき（中心が強く光る）。</summary>
        public static Sprite Sparkle => Cached("chrome.sparkle", () => Paint(128, 128, (x, y) =>
        {
            float u = (x - 64) / 64f, v = (y - 64) / 64f;
            float cross = Mathf.Exp(-Mathf.Abs(u) * 20f) * Mathf.Clamp01(1 - Mathf.Abs(v)) + Mathf.Exp(-Mathf.Abs(v) * 20f) * Mathf.Clamp01(1 - Mathf.Abs(u));
            float du = (u + v) * 0.7071f, dv = (u - v) * 0.7071f;
            float diagonal = (Mathf.Exp(-Mathf.Abs(du) * 26f) * Mathf.Clamp01(1 - Mathf.Abs(dv) * 2f) + Mathf.Exp(-Mathf.Abs(dv) * 26f) * Mathf.Clamp01(1 - Mathf.Abs(du) * 2f)) * 0.5f;
            float core = Mathf.Exp(-(u * u + v * v) * 22f);
            return new Color(1, 1, 1, Mathf.Clamp01(cross + diagonal + core));
        }));

        /// <summary>
        /// 見出しのリボン（左右に切れ込みのある帯）。左右の端だけを伸ばさない9スライスなので、横に伸ばして使う。
        /// </summary>
        public static Sprite Ribbon => Cached("chrome.ribbon", () => Paint(512, 128, (x, y) =>
        {
            float band = SdRoundBox(x - 256, y - 68, 208, 38, 6);
            float tailL = Mathf.Max(SdRoundBox(x - 50, y - 54, 50, 34, 2), -SdTriangle(x, y, -1, 20, -1, 88, 26, 54));
            float tailR = Mathf.Max(SdRoundBox(x - 462, y - 54, 50, 34, 2), -SdTriangle(x, y, 513, 20, 513, 88, 486, 54));
            float fold = Mathf.Min(SdTriangle(x, y, 48, 30, 48, 20, 66, 30), SdTriangle(x, y, 464, 30, 464, 20, 446, 30));
            Color c;
            if (band < 0.5f)
            {
                float t = (y - 30) / 76f;
                float l = Mathf.Lerp(0.72f, 1f, Smooth(0f, 1f, t));
                l = Mathf.Max(l, Mathf.Clamp01(1 - Mathf.Abs(t - 0.86f) * 14f));
                c = new Color(l, l, l, Coverage(band));
            }
            else if (fold < 0.5f)
                c = new Color(0.32f, 0.32f, 0.32f, Coverage(fold));
            else
            {
                float tail = Mathf.Min(tailL, tailR);
                c = new Color(0.6f, 0.6f, 0.6f, Coverage(tail));
            }
            return c;
        }, new Vector4(96, 0, 96, 0)));

        /// <summary>金貨（色付き）。縁の段差と星の刻印、左上からの光。大勝ちの演出や所持金の印に使う。</summary>
        public static Sprite Coin => Cached("chrome.coin", () => Paint(128, 128, (x, y) =>
        {
            float u = (x - 64) / 62f, v = (y - 64) / 62f;
            float r = Mathf.Sqrt(u * u + v * v);
            float edge = (r - 1f) * 62f;
            var light = Palette.GoldLight;
            var mid = Palette.GoldMid;
            var dark = Palette.GoldDark;
            float shade = Mathf.Clamp01(0.55f + (v - u) * 0.35f);
            var c = Color.Lerp(dark, mid, shade);
            if (r < 0.8f)
            {
                c = Color.Lerp(mid, light, Mathf.Clamp01(0.45f + (v - u) * 0.4f));
                float star = SdStar(u, v + 0.02f, 5, 0.22f, 0.5f) * 62f;
                c = Color.Lerp(c, Color.Lerp(dark, mid, 0.5f), Coverage(star) * 0.8f);
            }
            float groove = Mathf.Abs(r - 0.8f) * 62f - 1.2f;
            c = Color.Lerp(c, dark, Coverage(groove) * 0.8f);
            float spec = Mathf.Exp(-((u + 0.35f) * (u + 0.35f) + (v - 0.4f) * (v - 0.4f)) * 14f) * 0.6f;
            c = Color.Lerp(c, Color.white, spec);
            c.a = Coverage(edge);
            return c;
        }));

        /// <summary>カットした宝石（白の明暗で面を描く。色を掛けて使う）。コンプポイントの印。</summary>
        public static Sprite Jewel => Cached("chrome.jewel", () => Paint(128, 128, (x, y) =>
        {
            float u = (x - 64) / 60f, v = (y - 64) / 60f;
            // 上の冠（台形）と下のパビリオン（逆三角形）
            float crown = Mathf.Max(Mathf.Max(v - 0.62f, -(v - 0.12f)), Mathf.Abs(u) - (1f - (v - 0.12f) * 0.56f));
            float pavilion = SdTriangle(u, v, -1f, 0.12f, 1f, 0.12f, 0f, -0.98f) / 1f;
            float shape = Mathf.Min(crown, pavilion);
            float l;
            if (v > 0.12f)
                l = Mathf.Abs(u) < 0.42f ? 1f : 0.82f;
            else
                l = u < -0.34f ? 0.58f : u > 0.34f ? 0.72f : 0.88f;
            float edges = Mathf.Min(Mathf.Abs(v - 0.12f), Mathf.Abs(Mathf.Abs(u) - 0.42f) + Mathf.Max(0, 0.12f - v) * 10f);
            l = Mathf.Lerp(l, 1f, Mathf.Clamp01(1 - edges * 40f) * 0.5f);
            float spark = Mathf.Exp(-((u + 0.3f) * (u + 0.3f) + (v - 0.4f) * (v - 0.4f)) * 30f);
            l = Mathf.Lerp(l, 1.6f, spark);
            return new Color(Mathf.Min(1, l), Mathf.Min(1, l), Mathf.Min(1, l), Coverage(shape * 60f));
        }));

        /// <summary>上が半円のアーチ（奥の壁の窓や柱の間）。下ほど少し暗い。縦横比 1:2 のまま使う。</summary>
        public static Sprite Arch => Cached("chrome.arch", () => Paint(128, 256, (x, y) =>
        {
            float body = SdRoundBox(x - 64, y - 96, 62, 96, 0);
            float top = SdCircle(x, y, 64, 192, 62);
            float l = Mathf.Lerp(0.7f, 1f, y / 256f);
            return new Color(l, l, l, Coverage(Mathf.Min(body, top)));
        }));

        /// <summary>手前が広く奥が狭い台形（遠近のついた絨毯や床の光）。</summary>
        public static Sprite Trapezoid => Cached("chrome.trapezoid", () => Paint(256, 256, (x, y) =>
        {
            float half = Mathf.Lerp(127f, 40f, y / 256f);
            float d = Mathf.Abs(x - 128) - half;
            return new Color(1, 1, 1, Coverage(Mathf.Max(d, Mathf.Abs(y - 128) - 127)));
        }));

        /// <summary>紙吹雪の1片（角の丸い小さな長方形）。</summary>
        public static Sprite Confetti => Cached("chrome.confetti", () => Paint(24, 36, (x, y) =>
            new Color(1, 1, 1, Coverage(SdRoundBox(x - 12, y - 18, 11, 17, 4)))));
    }
}
