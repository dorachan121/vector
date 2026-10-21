using System;
using UnityEngine;
using Vector.Core.Harpoon;
using Vector.Game.UI;
using static Vector.Game.UI.Sdf;
using static Vector.Game.UI.SpriteFactory;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// HARPOON の海の生き物とモリの絵を実行時に描く。どれも右向き（クラゲは上向き）で、左へ泳ぐときは左右を反転して使う。
    /// 座標は横幅を [-1, 1] とした正規化座標（縦は縦横比ぶん）で、輪郭は距離関数（SDF）でなめらかにする。
    /// </summary>
    public static class MarineArt
    {
        static readonly Color Ink = Theme.Hex("0B0D14");

        public static Sprite Of(int quarry)
        {
            switch (quarry)
            {
                case Quarry.Sardine: return Sardine;
                case Quarry.Clownfish: return Clownfish;
                case Quarry.Puffer: return Puffer;
                case Quarry.Turtle: return Turtle;
                case Quarry.Swordfish: return Swordfish;
                case Quarry.Shark: return Shark;
                case Quarry.Whale: return Whale;
                case Quarry.Leviathan: return Leviathan;
                default: return Jellyfish;
            }
        }

        // ------------------------------------------------------------------
        // 生き物
        // ------------------------------------------------------------------

        static Sprite Sardine => Creature("sardine", 160, 64,
            (x, y) => Mathf.Min(SdEllipse(x, y, 0.08f, 0, 0.78f, 0.26f), SdTriangle(x, y, -0.6f, 0, -0.98f, 0.3f, -0.98f, -0.3f)),
            (x, y) =>
            {
                var c = Color.Lerp(Theme.Hex("DCE7F2"), Theme.Hex("2F5D8C"), Smooth(-0.08f, 0.12f, y));
                // 体の横に並ぶ黒い点
                for (int i = 0; i < 4; i++)
                {
                    if (Dist(x, y, 0.32f - i * 0.17f, 0.06f) < 0.035f)
                        c = Theme.Hex("1B2C40");
                }
                return Eye(c, x, y, 0.62f, 0.05f, 0.075f);
            });

        static Sprite Clownfish => Creature("clownfish", 160, 104,
            (x, y) =>
            {
                float d = SdEllipse(x, y, 0.05f, 0, 0.72f, 0.46f);
                d = Mathf.Min(d, SdEllipse(x, y, -0.76f, 0, 0.22f, 0.38f));
                d = Mathf.Min(d, SdEllipse(x, y, -0.02f, 0.4f, 0.4f, 0.16f));
                return Mathf.Min(d, SdEllipse(x, y, 0.12f, -0.42f, 0.2f, 0.1f));
            },
            (x, y) =>
            {
                var c = Theme.Hex("FF7A1A");
                foreach (var (center, half) in new[] { (0.38f, 0.075f), (-0.12f, 0.085f), (-0.62f, 0.06f) })
                {
                    float from = Mathf.Abs(x - center);
                    if (from < half)
                        c = Theme.Hex("FFF6EA");
                    else if (from < half + 0.035f)
                        c = Ink;
                }
                return Eye(c, x, y, 0.55f, 0.12f, 0.09f);
            });

        static Sprite Puffer => Creature("puffer", 144, 128,
            (x, y) =>
            {
                float d = SdStar(x - 0.05f, y, 14, 0.62f, 0.78f);
                d = Mathf.Min(d, SdCircle(x, y, 0.05f, 0, 0.64f));
                return Mathf.Min(d, SdTriangle(x, y, -0.55f, 0, -0.96f, 0.26f, -0.96f, -0.26f));
            },
            (x, y) =>
            {
                float r = Dist(x, y, 0.05f, 0);
                if (r > 0.64f && x > -0.55f)
                    return Theme.Hex("C99A2E");
                var c = Color.Lerp(Theme.Hex("FFF3CF"), Theme.Hex("F2C14E"), Smooth(-0.3f, -0.1f, y));
                foreach (var (sx, sy) in new[] { (-0.2f, 0.32f), (0.1f, 0.42f), (-0.35f, 0.1f), (0.0f, 0.18f), (-0.15f, -0.05f) })
                {
                    if (Dist(x, y, sx, sy) < 0.06f)
                        c = Theme.Hex("8A5A1E");
                }
                if (Dist(x, y, 0.66f, -0.06f) < 0.05f)
                    c = Theme.Hex("8A5A1E");
                return Eye(c, x, y, 0.36f, 0.2f, 0.13f);
            });

        static Sprite Turtle => Creature("turtle", 176, 144,
            (x, y) =>
            {
                float d = SdEllipse(x, y, -0.05f, 0.02f, 0.58f, 0.46f);
                d = Mathf.Min(d, SdCircle(x, y, 0.7f, 0.08f, 0.17f));
                d = Mathf.Min(d, SdEllipse(x, y, 0.52f, 0.06f, 0.2f, 0.1f));
                d = Mathf.Min(d, SdEllipse(x, y, 0.3f, 0.52f, 0.3f, 0.1f));
                d = Mathf.Min(d, SdEllipse(x, y, 0.28f, -0.5f, 0.3f, 0.1f));
                d = Mathf.Min(d, SdEllipse(x, y, -0.55f, 0.4f, 0.17f, 0.08f));
                return Mathf.Min(d, SdEllipse(x, y, -0.55f, -0.38f, 0.17f, 0.08f));
            },
            (x, y) =>
            {
                float shell = SdEllipse(x, y, -0.05f, 0.02f, 0.58f, 0.46f);
                if (shell > 0)
                    return Eye(Theme.Hex("A9C27A"), x, y, 0.76f, 0.13f, 0.05f);
                var c = Theme.Hex("3F7A45");
                // 甲羅の真ん中の板と、そこから外へ伸びる継ぎ目
                if (SdEllipse(x, y, -0.05f, 0.02f, 0.28f, 0.22f) < 0)
                    c = Theme.Hex("5E9A55");
                float angle = Mathf.Atan2(y - 0.02f, x + 0.05f);
                float seam = Mathf.Abs(Mathf.Repeat(angle / (Mathf.PI / 3f), 1f) - 0.5f);
                if (seam > 0.46f && SdEllipse(x, y, -0.05f, 0.02f, 0.28f, 0.22f) > 0)
                    c = Theme.Hex("2B5530");
                if (Mathf.Abs(SdEllipse(x, y, -0.05f, 0.02f, 0.28f, 0.22f)) < 0.02f)
                    c = Theme.Hex("2B5530");
                return c;
            });

        static Sprite Swordfish => Creature("swordfish", 288, 96,
            (x, y) =>
            {
                float d = SdEllipse(x, y, 0, 0, 0.6f, 0.2f);
                d = Mathf.Min(d, SdSegment(x, y, 0.5f, 0.02f, 0.99f, 0.04f, 0.022f));
                d = Mathf.Min(d, SdTriangle(x, y, 0.28f, 0.14f, -0.18f, 0.14f, 0.12f, 0.32f));
                d = Mathf.Min(d, SdTriangle(x, y, -0.5f, 0, -0.95f, 0.31f, -0.72f, 0));
                return Mathf.Min(d, SdTriangle(x, y, -0.5f, 0, -0.95f, -0.31f, -0.72f, 0));
            },
            (x, y) =>
            {
                if (y > 0.17f && x > -0.2f && x < 0.3f)
                    return Theme.Hex("17386A");
                var c = Color.Lerp(Theme.Hex("C9D8E8"), Theme.Hex("1F4E8C"), Smooth(-0.08f, 0.06f, y));
                if (y > -0.04f && Mathf.Abs(Mathf.Repeat(x * 7f, 1f) - 0.5f) < 0.12f && x < 0.35f && x > -0.45f)
                    c = Color.Lerp(c, Theme.Hex("6FA6E0"), 0.5f);
                return Eye(c, x, y, 0.42f, 0.05f, 0.045f);
            });

        static Sprite Shark => Creature("shark", 288, 128,
            (x, y) =>
            {
                float d = SdEllipse(x, y, 0.02f, -0.02f, 0.72f, 0.24f);
                d = Mathf.Min(d, SdTriangle(x, y, -0.14f, 0.16f, 0.2f, 0.16f, -0.06f, 0.44f));
                d = Mathf.Min(d, SdTriangle(x, y, 0.12f, -0.12f, 0.36f, -0.12f, 0.02f, -0.38f));
                d = Mathf.Min(d, SdTriangle(x, y, -0.6f, -0.02f, -0.98f, 0.4f, -0.8f, -0.02f));
                return Mathf.Min(d, SdTriangle(x, y, -0.6f, -0.02f, -0.94f, -0.28f, -0.78f, -0.02f));
            },
            (x, y) =>
            {
                var c = Color.Lerp(Theme.Hex("E8EEF2"), Theme.Hex("6B7C8A"), Smooth(-0.12f, -0.02f, y + 0.05f * x));
                // えら
                for (int i = 0; i < 3; i++)
                {
                    if (Mathf.Abs(x - (0.36f + i * 0.045f)) < 0.008f && Mathf.Abs(y + 0.02f) < 0.09f)
                        c = Theme.Hex("3D4A55");
                }
                return Eye(c, x, y, 0.52f, 0.04f, 0.035f);
            });

        static Sprite Whale => Creature("whale", 320, 160,
            (x, y) =>
            {
                float d = SdEllipse(x, y, 0.08f, 0, 0.78f, 0.36f);
                d = Mathf.Min(d, SdEllipse(x, y, -0.64f, 0.05f, 0.26f, 0.12f));
                d = Mathf.Min(d, SdTriangle(x, y, -0.84f, 0.06f, -1f, 0.34f, -0.9f, 0.02f));
                d = Mathf.Min(d, SdTriangle(x, y, -0.84f, 0.06f, -1f, -0.2f, -0.92f, 0.1f));
                return Mathf.Min(d, SdEllipse(x, y, 0.28f, -0.3f, 0.17f, 0.07f));
            },
            (x, y) =>
            {
                var c = Theme.Hex("2E5C8A");
                // 腹のひだ
                if (y < -0.1f + 0.08f * x && x > -0.4f)
                    c = Mathf.Repeat(y * 22f, 1f) < 0.25f ? Theme.Hex("8FAFC4") : Theme.Hex("BFD4E0");
                if (Dist(x, y, -0.1f, 0.22f) < 0.05f || Dist(x, y, 0.25f, 0.26f) < 0.04f)
                    c = Color.Lerp(c, Color.white, 0.25f);
                return Eye(c, x, y, 0.6f, -0.05f, 0.035f);
            });

        /// <summary>深海の大蛇。くねる胴を円の連なりで描き、背びれと角は光る青緑。</summary>
        static Sprite Leviathan => Creature("leviathan", 384, 160,
            (x, y) =>
            {
                float d = SdEllipse(x, y, 0.8f, 0.04f, 0.19f, 0.13f);
                d = Mathf.Min(d, SdTriangle(x, y, 0.74f, 0.1f, 0.62f, 0.12f, 0.48f, 0.36f));
                d = Mathf.Min(d, SdTriangle(x, y, 0.84f, 0.12f, 0.74f, 0.14f, 0.7f, 0.38f));
                for (int i = 0; i < 16; i++)
                {
                    float t = i / 15f;
                    float r = 0.15f * (1 - 0.65f * t) + 0.02f;
                    var (cx, cy) = Spine(t);
                    d = Mathf.Min(d, SdCircle(x, y, cx, cy, r));
                    if (i % 2 == 1 && i < 14)
                        d = Mathf.Min(d, SdTriangle(x, y, cx - r * 0.6f, cy + r * 0.7f, cx + r * 0.6f, cy + r * 0.7f, cx - r * 0.3f, cy + r * 1.8f));
                }
                var (tx, ty) = Spine(1f);
                return Mathf.Min(d, SdTriangle(x, y, tx + 0.04f, ty, tx - 0.16f, ty + 0.16f, tx - 0.12f, ty - 0.12f));
            },
            (x, y) =>
            {
                float body = SdEllipse(x, y, 0.8f, 0.04f, 0.19f, 0.13f);
                float spineDistance = float.MaxValue;
                float spineY = 0;
                for (int i = 0; i < 16; i++)
                {
                    float t = i / 15f;
                    var (cx, cy) = Spine(t);
                    float r = 0.15f * (1 - 0.65f * t) + 0.02f;
                    float d = Dist(x, y, cx, cy) - r;
                    if (d < spineDistance)
                    {
                        spineDistance = d;
                        spineY = cy;
                    }
                }
                if (Mathf.Min(body, spineDistance) > 0)
                    return Theme.Hex("34E0C8");
                var c = y < spineY - 0.04f && body > 0 ? Theme.Hex("6E4FA8") : Theme.Hex("3B1F6B");
                // うろこ
                if (Mathf.Repeat(x * 18f + Mathf.Floor(y * 18f) * 0.5f, 1f) < 0.18f && Mathf.Repeat(y * 18f, 1f) < 0.5f)
                    c = Color.Lerp(c, Theme.Hex("8C6CD0"), 0.4f);
                if (Dist(x, y, 0.87f, 0.07f) < 0.035f)
                    return Theme.Hex("FF3B3B");
                return c;
            });

        static (float x, float y) Spine(float t) => (0.68f - 1.62f * t, 0.15f * Mathf.Sin(t * Mathf.PI * 2.4f) * (0.4f + t));

        /// <summary>金のクラゲ（上向き）。かさと、波打つ触手。</summary>
        static Sprite Jellyfish => Creature("jellyfish", 128, 176,
            (x, y) =>
            {
                float bell = Mathf.Max(SdEllipse(x, y, 0, 0.5f, 0.82f, 0.66f), 0.22f - y);
                float d = bell;
                for (int i = 0; i < 5; i++)
                {
                    float x0 = -0.5f + 0.25f * i;
                    if (y < 0.3f && y > -1.3f)
                    {
                        float wave = x0 + 0.09f * Mathf.Sin(y * 6f + i * 1.3f) * (0.3f - y);
                        float width = i == 2 ? 0.07f : 0.04f;
                        d = Mathf.Min(d, Mathf.Abs(x - wave) - width * (1f + 0.4f * y));
                    }
                }
                return d;
            },
            (x, y) =>
            {
                if (y < 0.22f)
                    return Theme.Hex("FFC94A").WithAlpha(0.85f);
                var c = Color.Lerp(Theme.Hex("FFB627"), Theme.Hex("FFF1C2"), Smooth(0.3f, 1.0f, y));
                if (Mathf.Abs(SdEllipse(x, y, 0, 0.5f, 0.5f, 0.38f)) < 0.04f && y > 0.3f)
                    c = Theme.Hex("FF8F3D");
                return c;
            });

        // ------------------------------------------------------------------
        // 道具と海の景色
        // ------------------------------------------------------------------

        /// <summary>モリ（右向き、白で描いて色は Image.color で付ける）。柄と、返しの付いた穂先。</summary>
        public static Sprite Harpoon => Cached("marine.harpoon", () => Paint(192, 40, (px, py) =>
        {
            float x = px / 96f - 1, y = (py - 20) / 96f;
            float d = SdSegment(x, y, -0.98f, 0, 0.6f, 0, 0.035f);
            d = Mathf.Min(d, SdTriangle(x, y, 0.55f, 0.14f, 0.99f, 0, 0.55f, -0.14f));
            d = Mathf.Min(d, SdTriangle(x, y, 0.5f, 0.05f, 0.62f, 0.05f, 0.44f, 0.19f));
            d = Mathf.Min(d, SdTriangle(x, y, 0.5f, -0.05f, 0.62f, -0.05f, 0.44f, -0.19f));
            return new Color(1, 1, 1, Coverage(d * 96f));
        }));

        /// <summary>海藻（下端を根元にして揺らす）。</summary>
        public static Sprite Kelp => Cached("marine.kelp", () => Paint(64, 256, (px, py) =>
        {
            float y = py / 256f;
            float center = 32 + 10 * Mathf.Sin(y * 9f);
            float width = 12 * (1 - y * 0.75f);
            float d = Mathf.Abs(px - center) - width;
            return new Color(1, 1, 1, Coverage(d) * Smooth(1f, 0.85f, y));
        }));

        // ------------------------------------------------------------------
        // 描画の補助
        // ------------------------------------------------------------------

        /// <summary>形（距離関数）と色（座標ごと）から生き物を描き、縁を少し暗くして輪郭を出す。</summary>
        static Sprite Creature(string key, int width, int height, Func<float, float, float> shape, Func<float, float, Color> skin)
            => Cached("marine." + key, () => Paint(width, height, (px, py) =>
            {
                float scale = width / 2f;
                float x = px / scale - 1;
                float y = (py - height / 2f) / scale;
                float d = shape(x, y) * scale;
                float alpha = Coverage(d);
                if (alpha <= 0)
                    return Color.clear;
                var c = skin(x, y);
                float rim = Mathf.Clamp01(1 + d / 3.5f);
                c = Shade(c, 1 - rim * 0.55f);
                // 上から差す光
                c = Shade(c, 0.9f + 0.18f * Mathf.Clamp01(y * 2 + 0.5f));
                c.a *= alpha;
                return c;
            }));

        static Color Eye(Color skin, float x, float y, float cx, float cy, float r)
        {
            float d = Dist(x, y, cx, cy);
            if (d > r)
                return skin;
            if (Dist(x, y, cx + r * 0.28f, cy + r * 0.28f) < r * 0.24f)
                return Color.white;
            return d < r * 0.66f ? Ink : Color.white;
        }

        static float Dist(float x, float y, float cx, float cy) => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

        static Color Shade(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, c.a);
    }
}
