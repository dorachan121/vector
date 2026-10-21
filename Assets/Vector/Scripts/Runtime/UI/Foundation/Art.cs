using System;
using System.Collections.Generic;
using UnityEngine;
using Vector.Core;

namespace Vector.Game.UI
{
    /// <summary>
    /// ゲーム内のグラフィックを実行時に生成する。画像ファイルを持たず、どの解像度でも破綻しないよう
    /// 距離関数（SDF）でアンチエイリアス付きに描く。生成結果はキャッシュしてシーンをまたいで使い回す。
    /// </summary>
    public static class Art
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        // ------------------------------------------------------------------
        // 基本図形
        // ------------------------------------------------------------------

        /// <summary>角丸の白い四角（9スライス用）。Image.type = Sliced で使う。</summary>
        public static Sprite RoundedRect(int radius) => Rounded($"rr{radius}", radius, d => d);

        /// <summary>角丸の枠線（9スライス用）。</summary>
        public static Sprite RoundedOutline(int radius, float thickness)
            => Rounded($"ro{radius}_{thickness}", radius, d => Mathf.Abs(d + thickness / 2f) - thickness / 2f);

        /// <summary>角丸の四角の符号付き距離を shape で変形して塗る（9スライス用）。</summary>
        static Sprite Rounded(string key, int radius, Func<float, float> shape) => Cached(key, () =>
        {
            int size = radius * 2 + 4;
            var tex = NewTexture(size, size);
            var px = new Color32[size * size];
            float half = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = SdRoundBox(x + 0.5f - half, y + 0.5f - half, half - 1, half - 1, radius);
                px[y * size + x] = new Color32(255, 255, 255, Alpha(shape(d)));
            }
            return SlicedSprite(tex, px, radius + 1);
        });

        public static Sprite Circle => Cached("circle", () => Disc(256, (x, y) => SdCircle(x, y, 0, 0, 0.98f)));

        public static Sprite Ring(float thickness) => Cached($"ring{thickness}", () =>
            Disc(256, (x, y) => Mathf.Abs(SdCircle(x, y, 0, 0, 0.98f - thickness / 2f)) - thickness / 2f));

        /// <summary>中心が明るく外へ消えていく円。光彩やスポットライトに使う。</summary>
        public static Sprite Glow => Cached("glow", () =>
        {
            const int size = 128;
            var tex = NewTexture(size, size);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size * 2 - 1;
                float v = (y + 0.5f) / size * 2 - 1;
                float r = Mathf.Sqrt(u * u + v * v);
                float a = Mathf.Clamp01(1 - r);
                a = a * a * (3 - 2 * a);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            return Finish(tex, px);
        });

        /// <summary>上から下への縦グラデーション（白→透明）。</summary>
        public static Sprite VerticalFade => Cached("vfade", () =>
        {
            const int w = 4, h = 64;
            var tex = NewTexture(w, h);
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                px[y * w + x] = new Color32(255, 255, 255, (byte)(255 * (y / (float)(h - 1))));
            return Finish(tex, px);
        });

        // ------------------------------------------------------------------
        // 背景・テーブル
        // ------------------------------------------------------------------

        /// <summary>画面全体の背景。中央がわずかに明るい漆黒のビネット。</summary>
        public static Sprite Backdrop => Cached("backdrop", () =>
        {
            const int w = 320, h = 180;
            var tex = NewTexture(w, h);
            var px = new Color32[w * h];
            var inner = Theme.Hex("1B2420");
            var outer = Theme.Backdrop;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w * 2 - 1;
                float v = (y + 0.5f) / h * 2 - 1;
                float r = Mathf.Sqrt(u * u * 0.8f + v * v * 1.2f);
                float t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(r / 1.25f));
                var c = Color.Lerp(inner, outer, t);
                c *= 1 + (Hash(x, y) - 0.5f) * 0.04f;
                c.a = 1;
                px[y * w + x] = c;
            }
            return Finish(tex, px, FilterMode.Bilinear);
        });

        /// <summary>
        /// カジノテーブル。角の大きく丸い長方形に、革張りのレール・金の縁取り・起毛したフェルトを描く。
        /// </summary>
        public static Sprite Table => TableIn(Theme.FeltCenter, Theme.FeltEdge);

        /// <summary>フェルトの色を変えたテーブル（ゲームごとの色）。</summary>
        public static Sprite TableIn(Color feltCenter, Color feltEdge) => Cached($"table{ColorUtility.ToHtmlStringRGB(feltCenter)}{ColorUtility.ToHtmlStringRGB(feltEdge)}", () =>
        {
            const int w = 1496, h = 696;
            const float radius = 96f;
            const float rail = 40f;
            var tex = NewTexture(w, h);
            var px = new Color32[w * h];
            float hw = w / 2f, hh = h / 2f;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float lx = x + 0.5f - hw;
                float ly = y + 0.5f - hh;
                float d = SdRoundBox(lx, ly, hw - 2, hh - 2, radius);
                if (d > 1.5f)
                {
                    px[y * w + x] = new Color32(0, 0, 0, 0);
                    continue;
                }

                Color c;
                float inside = -d;
                if (inside < rail)
                {
                    // レール: 断面が丸く見えるよう明暗をつけた革
                    float t = inside / rail;
                    float bulge = Mathf.Sin(t * Mathf.PI);
                    c = Color.Lerp(Theme.RailDark, Theme.RailLight, bulge * 0.85f);
                    float spec = Mathf.Pow(Mathf.Max(0, Mathf.Sin(t * Mathf.PI * 0.9f + 0.4f)), 18) * 0.35f;
                    c += new Color(spec, spec * 0.85f, spec * 0.6f, 0);
                    c *= 1 + (Hash(x, y) - 0.5f) * 0.06f;
                    // 外周の影と内側の金の縁取り
                    if (t > 0.9f)
                        c = Color.Lerp(c, Theme.GoldDeep, Mathf.SmoothStep(0.9f, 0.97f, t) * 0.9f);
                    if (t > 0.96f)
                        c = Color.Lerp(c, Theme.Gold, Mathf.SmoothStep(0.96f, 1f, t));
                }
                else
                {
                    // フェルト: 中央が明るい放射グラデーション + 繊維のノイズ + レール際の影
                    float u = lx / hw;
                    float v = ly / hh;
                    float r = Mathf.Sqrt(u * u * 0.9f + v * v * 1.1f);
                    c = Color.Lerp(feltCenter, feltEdge, Mathf.SmoothStep(0.05f, 1.15f, r));
                    float grain = (Hash(x, y) - 0.5f) * 0.07f + (Hash(x / 3, y / 3 + 977) - 0.5f) * 0.05f;
                    c *= 1 + grain;
                    float shadow = Mathf.Clamp01((inside - rail) / 30f);
                    c *= Mathf.Lerp(0.55f, 1f, shadow * shadow * (3 - 2 * shadow));
                }
                c.a = Mathf.Clamp01(0.5f - d);
                px[y * w + x] = c;
            }
            return Finish(tex, px, FilterMode.Bilinear);
        });

        // ------------------------------------------------------------------
        // トランプ
        // ------------------------------------------------------------------

        /// <summary>スートのマーク（白）。色は Image.color で付ける。</summary>
        public static Sprite Suit(Suit suit) => Cached($"suit{suit}", () => Disc(160, SuitDistance(suit)));

        static Func<float, float, float> SuitDistance(Suit suit)
        {
            switch (suit)
            {
                case Core.Suit.Hearts:
                    return (x, y) => Heart(x, y + 0.04f);
                case Core.Suit.Diamonds:
                    return (x, y) => SdRhombus(x, y, 0.66f, 0.95f);
                case Core.Suit.Spades:
                    return (x, y) => Mathf.Min(Heart(x, -(y - 0.16f)), SdTriangle(x, y, 0, -0.1f, -0.34f, -0.94f, 0.34f, -0.94f));
                default:
                    return (x, y) =>
                    {
                        float d = SdCircle(x, y, 0, 0.44f, 0.36f);
                        d = Mathf.Min(d, SdCircle(x, y, -0.42f, -0.1f, 0.36f));
                        d = Mathf.Min(d, SdCircle(x, y, 0.42f, -0.1f, 0.36f));
                        d = Mathf.Min(d, SdCircle(x, y, 0, 0.08f, 0.22f));
                        d = Mathf.Min(d, SdTriangle(x, y, 0, 0.0f, -0.32f, -0.94f, 0.32f, -0.94f));
                        return d;
                    };
            }
        }

        static float Heart(float x, float y)
        {
            float d = SdCircle(x, y, -0.43f, 0.38f, 0.5f);
            d = Mathf.Min(d, SdCircle(x, y, 0.43f, 0.38f, 0.5f));
            d = Mathf.Min(d, SdTriangle(x, y, -0.9f, 0.22f, 0.9f, 0.22f, 0, -0.92f));
            return d;
        }

        /// <summary>カードの裏面。ワインレッドに金の菱形格子とメダリオン。</summary>
        public static Sprite CardBack => Cached("cardback", () =>
        {
            const int w = 300, h = 420;
            const float radius = 22f;
            var tex = NewTexture(w, h);
            var px = new Color32[w * h];
            float hw = w / 2f, hh = h / 2f;
            var gold = Theme.Gold;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float lx = x + 0.5f - hw;
                float ly = y + 0.5f - hh;
                float d = SdRoundBox(lx, ly, hw - 1, hh - 1, radius);
                Color c = Theme.CardFace;
                float inner = SdRoundBox(lx, ly, hw - 16, hh - 16, radius - 10);
                if (inner < 0)
                {
                    c = Theme.CardBack;
                    // 菱形格子
                    float a = (lx + ly) / 18f;
                    float b = (lx - ly) / 18f;
                    float la = Mathf.Abs(a - Mathf.Round(a)) * 18f;
                    float lb = Mathf.Abs(b - Mathf.Round(b)) * 18f;
                    float line = Mathf.Min(la, lb);
                    c = Color.Lerp(c, Color.Lerp(Theme.CardBack, gold, 0.55f), Mathf.Clamp01(1.4f - line));
                    // 中央のメダリオン
                    float m = SdRhombus(lx / 70f, ly / 96f, 1f, 1f) * 70f;
                    if (m < 0)
                        c = Color.Lerp(Theme.Hex("4A0812"), Theme.CardBack, Mathf.Clamp01(-m / 40f));
                    float rim = Mathf.Abs(m) - 2.2f;
                    c = Color.Lerp(c, gold, Alpha(rim) / 255f);
                    // 内枠の金線
                    float frame = Mathf.Abs(inner + 6f) - 1.6f;
                    c = Color.Lerp(c, gold, Alpha(frame) / 255f);
                    // 紙の陰影
                    c *= 1 + (Hash(x, y) - 0.5f) * 0.05f;
                }
                c.a = Alpha(d) / 255f;
                px[y * w + x] = c;
            }
            return Finish(tex, px, FilterMode.Bilinear);
        });

        // ------------------------------------------------------------------
        // チップ
        // ------------------------------------------------------------------

        /// <summary>カジノチップ。縁に6か所のインサート、内側に破線のリングを描く。</summary>
        public static Sprite Chip(Color body, Color accent) => Cached($"chip{ColorUtility.ToHtmlStringRGB(body)}{ColorUtility.ToHtmlStringRGB(accent)}", () =>
        {
            const int size = 200;
            var tex = NewTexture(size, size);
            var px = new Color32[size * size];
            float half = size / 2f;
            float outer = half - 2;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float lx = x + 0.5f - half;
                float ly = y + 0.5f - half;
                float r = Mathf.Sqrt(lx * lx + ly * ly);
                float d = r - outer;
                if (d > 1.5f)
                {
                    px[y * size + x] = new Color32(0, 0, 0, 0);
                    continue;
                }
                float angle = Mathf.Atan2(ly, lx);
                Color c = body;
                float rn = r / outer;
                // 縁のインサート（6か所）
                if (rn > 0.74f)
                {
                    float seg = Mathf.Repeat(angle / (Mathf.PI * 2) * 6f, 1f);
                    if (seg > 0.18f && seg < 0.46f)
                        c = accent;
                }
                // 内側の盛り上がりと破線リング
                if (rn < 0.66f)
                    c = Color.Lerp(body, Color.white, 0.12f);
                float ringD = Mathf.Abs(r - outer * 0.7f) - 1.6f;
                float dash = Mathf.Repeat(angle / (Mathf.PI * 2) * 24f, 1f);
                if (dash < 0.55f)
                    c = Color.Lerp(c, accent, Alpha(ringD) / 255f);
                float edgeD = Mathf.Abs(r - outer * 0.74f) - 0.8f;
                c = Color.Lerp(c, Color.black, Alpha(edgeD) / 255f * 0.25f);
                // 立体感
                float light = 1 + (-ly / outer) * 0.12f;
                c *= light;
                float rim = Mathf.Clamp01((rn - 0.93f) / 0.07f);
                c = Color.Lerp(c, c * 0.55f, rim);
                c.a = Mathf.Clamp01(0.5f - d);
                px[y * size + x] = c;
            }
            return Finish(tex, px, FilterMode.Bilinear);
        });

        // ------------------------------------------------------------------
        // 紋章・シンボル（白で描き、色は Image.color で付ける）
        // ------------------------------------------------------------------

        /// <summary>盾（COLOSSEUM のプレイヤーの紋章）。上が角丸の四角、下が尖ったヒーターシールド。</summary>
        public static Sprite Shield => Cached("shield", () => Disc(256, (x, y) =>
            Mathf.Min(SdRoundBox(x, y - 0.36f, 0.74f, 0.52f, 0.22f), SdTriangle(x, y, -0.74f, -0.1f, 0.74f, -0.1f, 0f, -0.96f))));

        /// <summary>角のある獣の頭の影絵（COLOSSEUM の獣、DELVE の魔物）。目は別の画像で重ねる。</summary>
        public static Sprite BeastHead => Cached("beast", () => Disc(256, (x, y) =>
        {
            float d = SdCircle(x, y, 0f, -0.08f, 0.52f);
            d = Mathf.Min(d, SdCircle(x, y, 0f, -0.48f, 0.3f));
            // 外へ反り上がる角
            d = Mathf.Min(d, SdTriangle(x, y, -0.42f, 0.22f, -0.12f, 0.36f, -0.9f, 0.94f));
            d = Mathf.Min(d, SdTriangle(x, y, 0.42f, 0.22f, 0.12f, 0.36f, 0.9f, 0.94f));
            // 耳
            d = Mathf.Min(d, SdTriangle(x, y, -0.46f, 0.08f, -0.92f, 0.2f, -0.5f, -0.2f));
            d = Mathf.Min(d, SdTriangle(x, y, 0.46f, 0.08f, 0.92f, 0.2f, 0.5f, -0.2f));
            return d;
        }));

        /// <summary>
        /// ALCHEMIA の位ごとのシンボルの形。1 素=小さな球、2 晶=菱形、3 霊薬=六角形、4 秘宝=六芒星、5 賢者の石=三角を抱いた円環。
        /// </summary>
        public static Sprite Gem(int tier) => Cached($"gem{tier}", () => Disc(192, GemDistance(tier)));

        static Func<float, float, float> GemDistance(int tier)
        {
            switch (tier)
            {
                case 1: return (x, y) => SdCircle(x, y, 0, 0, 0.5f);
                case 2: return (x, y) => SdRhombus(x, y, 0.62f, 0.9f);
                case 3: return (x, y) => SdPolygon(x, y, 6, 0.78f);
                case 4: return (x, y) => SdStar(x, y, 6, 0.5f, 0.95f);
                default:
                    return (x, y) =>
                    {
                        float ring = Mathf.Abs(SdCircle(x, y, 0, 0, 0.82f)) - 0.12f;
                        float tri = SdTriangle(x, y, 0f, 0.66f, -0.58f, -0.36f, 0.58f, -0.36f);
                        return Mathf.Min(ring, tri);
                    };
            }
        }

        /// <summary>迷宮の木の扉（色付き）。石のアーチに縦の板と鉄の帯、取っ手の輪。</summary>
        public static Sprite Door => Cached("door", () =>
        {
            const int w = 200, h = 300;
            var tex = NewTexture(w, h);
            var px = new Color32[w * h];
            var stone = Theme.Hex("4A4640");
            var wood = Theme.Hex("6B4426");
            var dark = Theme.Hex("2B1A0E");
            var iron = Theme.Hex("2F3236");
            for (int py = 0; py < h; py++)
            for (int pxl = 0; pxl < w; pxl++)
            {
                // 正規化座標: x は [-1, 1]、y は下端 -1.5 〜 上端 1.5
                float x = (pxl + 0.5f) / w * 2 - 1;
                float y = ((py + 0.5f) / h * 2 - 1) * 1.5f;
                float outer = Mathf.Min(SdRoundBox(x, y + 0.35f, 0.98f, 1.15f, 0.02f), SdCircle(x, y, 0, 0.5f, 0.98f));
                float inner = Mathf.Min(SdRoundBox(x, y + 0.42f, 0.78f, 1.04f, 0.02f), SdCircle(x, y, 0, 0.5f, 0.78f));
                float scale = w / 2f;
                Color c;
                if (inner < 0)
                {
                    // 縦の板と継ぎ目、鉄の帯
                    float plank = Mathf.Repeat((x + 1) * 3.2f, 1f);
                    c = Color.Lerp(wood, dark, Mathf.Clamp01(1.4f - Mathf.Abs(plank - 0.5f) * 40f) * 0.8f);
                    c *= 0.9f + Hash(pxl / 3, py) * 0.18f;
                    bool band = Mathf.Abs(y - 0.55f) < 0.07f || Mathf.Abs(y + 0.85f) < 0.07f;
                    if (band)
                        c = iron * (0.9f + Hash(pxl, py) * 0.2f);
                    float ring = Mathf.Abs(SdCircle(x, y, 0.5f, -0.1f, 0.1f)) - 0.025f;
                    c = Color.Lerp(c, Theme.Hex("8A8F96"), Alpha(ring * scale) / 255f);
                }
                else
                {
                    c = stone * (0.85f + Hash(pxl / 4, py / 4) * 0.3f);
                }
                c.a = Alpha(outer * scale) / 255f;
                px[py * w + pxl] = c;
            }
            return Finish(tex, px, FilterMode.Bilinear);
        });

        // ------------------------------------------------------------------
        // フロア（ストーリーモード）
        // ------------------------------------------------------------------

        /// <summary>カジノの絨毯の1マス（つなげて敷ける）。地の色に、細い格子と中央の菱形の柄。</summary>
        public static Sprite Carpet(Color ground, Color motif) => Cached($"carpet{ColorUtility.ToHtmlStringRGB(ground)}{ColorUtility.ToHtmlStringRGB(motif)}", () =>
        {
            const int size = 64;
            var tex = NewTexture(size, size);
            tex.wrapMode = TextureWrapMode.Repeat;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size * 2 - 1;
                float v = (y + 0.5f) / size * 2 - 1;
                var c = ground * (1 + (Hash(x, y) - 0.5f) * 0.08f);
                // 斜めの格子（マスの角どうしを結ぶ）
                float lattice = Mathf.Min(Mathf.Abs(Mathf.Abs(u) - Mathf.Abs(v)), 1f) * size / 2;
                c = Color.Lerp(c, motif, Mathf.Clamp01(1.2f - lattice) * 0.45f);
                // 中央の菱形と四隅の点
                float diamond = SdRhombus(u, v, 0.32f, 0.32f) * size / 2;
                c = Color.Lerp(c, motif, Alpha(diamond) / 255f * 0.55f);
                float ring = Mathf.Abs(SdRhombus(u, v, 0.5f, 0.5f) * size / 2) - 0.8f;
                c = Color.Lerp(c, motif, Alpha(ring) / 255f * 0.35f);
                c.a = 1;
                px[y * size + x] = c;
            }
            return Finish(tex, px, FilterMode.Bilinear);
        });

        /// <summary>髪（円の上側）。顔の円に重ねて使う。</summary>
        public static Sprite HairCap => Cached("haircap", () => Disc(128, (x, y) => Mathf.Max(SdCircle(x, y, 0, 0, 0.96f), -(y + 0.12f))));

        // ------------------------------------------------------------------
        // 補助
        // ------------------------------------------------------------------

        static Sprite Cached(string key, Func<Sprite> create)
        {
            if (cache.TryGetValue(key, out var sprite) && sprite != null)
                return sprite;
            sprite = create();
            sprite.name = key;
            cache[key] = sprite;
            return sprite;
        }

        static Texture2D NewTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };
            return tex;
        }

        static Sprite Finish(Texture2D tex, Color32[] px, FilterMode filter = FilterMode.Bilinear)
        {
            tex.filterMode = filter;
            tex.SetPixels32(px);
            tex.Apply(false, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        static Sprite SlicedSprite(Texture2D tex, Color32[] px, int border)
        {
            tex.SetPixels32(px);
            tex.Apply(false, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        /// <summary>正規化座標 [-1, 1] の距離関数から正方形のスプライトを作る。</summary>
        static Sprite Disc(int size, Func<float, float, float> sdf)
        {
            var tex = NewTexture(size, size);
            var px = new Color32[size * size];
            float scale = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / scale - 1;
                float v = (y + 0.5f) / scale - 1;
                float d = sdf(u, v) * scale;
                px[y * size + x] = new Color32(255, 255, 255, Alpha(d));
            }
            return Finish(tex, px);
        }

        static byte Alpha(float distance) => (byte)(Mathf.Clamp01(0.5f - distance) * 255f);

        static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }

        static float SdCircle(float x, float y, float cx, float cy, float r)
        {
            float dx = x - cx, dy = y - cy;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        static float SdRoundBox(float x, float y, float hw, float hh, float r)
        {
            float qx = Mathf.Abs(x) - hw + r;
            float qy = Mathf.Abs(y) - hh + r;
            float ox = Mathf.Max(qx, 0), oy = Mathf.Max(qy, 0);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
        }

        static float SdRhombus(float x, float y, float hw, float hh)
        {
            float ax = Mathf.Abs(x), ay = Mathf.Abs(y);
            float k = hw * hh / Mathf.Sqrt(hw * hw + hh * hh);
            return (ax / hw + ay / hh - 1f) * k;
        }

        /// <summary>正 n 角形（apothem は中心から辺までの距離）。</summary>
        static float SdPolygon(float x, float y, int n, float apothem)
        {
            float sector = Mathf.PI * 2 / n;
            float a = Mathf.Repeat(Mathf.Atan2(y, x) + Mathf.PI / 2, sector) - sector / 2;
            return Mathf.Sqrt(x * x + y * y) * Mathf.Cos(a) - apothem;
        }

        /// <summary>n 芒星（inner / outer は谷と先端の半径）。距離は近似だが、縁のアンチエイリアスには十分。</summary>
        static float SdStar(float x, float y, int n, float inner, float outer)
        {
            float sector = Mathf.PI * 2 / n;
            float a = Mathf.Abs(Mathf.Repeat(Mathf.Atan2(y, x) + Mathf.PI / 2, sector) - sector / 2) / (sector / 2);
            float radius = Mathf.Lerp(outer, inner, a);
            return (Mathf.Sqrt(x * x + y * y) - radius) * 0.7f;
        }

        static float SdTriangle(float px, float py, float ax, float ay, float bx, float by, float cx, float cy)
        {
            // Inigo Quilez の三角形の距離関数
            float e0x = bx - ax, e0y = by - ay;
            float e1x = cx - bx, e1y = cy - by;
            float e2x = ax - cx, e2y = ay - cy;
            float v0x = px - ax, v0y = py - ay;
            float v1x = px - bx, v1y = py - by;
            float v2x = px - cx, v2y = py - cy;
            float t0 = Mathf.Clamp01((v0x * e0x + v0y * e0y) / (e0x * e0x + e0y * e0y));
            float t1 = Mathf.Clamp01((v1x * e1x + v1y * e1y) / (e1x * e1x + e1y * e1y));
            float t2 = Mathf.Clamp01((v2x * e2x + v2y * e2y) / (e2x * e2x + e2y * e2y));
            float pq0x = v0x - e0x * t0, pq0y = v0y - e0y * t0;
            float pq1x = v1x - e1x * t1, pq1y = v1y - e1y * t1;
            float pq2x = v2x - e2x * t2, pq2y = v2y - e2y * t2;
            float s = Mathf.Sign(e0x * e2y - e0y * e2x);
            float d0 = pq0x * pq0x + pq0y * pq0y, s0 = s * (v0x * e0y - v0y * e0x);
            float d1 = pq1x * pq1x + pq1y * pq1y, s1 = s * (v1x * e1y - v1y * e1x);
            float d2 = pq2x * pq2x + pq2y * pq2y, s2 = s * (v2x * e2y - v2y * e2x);
            float d = Mathf.Min(d0, Mathf.Min(d1, d2));
            float sg = Mathf.Min(s0, Mathf.Min(s1, s2));
            return -Mathf.Sqrt(d) * Mathf.Sign(sg);
        }
    }
}
