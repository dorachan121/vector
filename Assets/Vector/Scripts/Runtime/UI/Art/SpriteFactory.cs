using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// 実行時に作るスプライトの共通処理（キャッシュ・テクスチャ・距離関数からの塗り・9スライス）。
    /// 生成結果はキャッシュしてシーンをまたいで使い回す。
    /// </summary>
    internal static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        internal static Sprite Cached(string key, Func<Sprite> create)
        {
            if (cache.TryGetValue(key, out var sprite) && sprite != null)
                return sprite;
            sprite = create();
            sprite.name = key;
            cache[key] = sprite;
            return sprite;
        }

        internal static Texture2D NewTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };
            return tex;
        }

        internal static Sprite Finish(Texture2D tex, Color32[] px, FilterMode filter = FilterMode.Bilinear)
        {
            tex.filterMode = filter;
            tex.SetPixels32(px);
            tex.Apply(false, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        internal static Sprite SlicedSprite(Texture2D tex, Color32[] px, int border)
        {
            tex.SetPixels32(px);
            tex.Apply(false, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        /// <summary>正規化座標 [-1, 1] の距離関数から正方形のスプライトを作る。</summary>

        internal static Sprite Disc(int size, Func<float, float, float> sdf)
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

        /// <summary>
        /// 1ピクセルずつ色を決めてスプライトを作る。shader にはピクセルの中心座標（左下が原点、y は上向き）を渡す。
        /// border を指定すると、その幅の縁を伸ばさない9スライスのスプライトになる。
        /// </summary>
        internal static Sprite Paint(int w, int h, Func<float, float, Color> shader, int border = 0, FilterMode filter = FilterMode.Bilinear)
            => Paint(w, h, shader, new Vector4(border, border, border, border), filter);

        internal static Sprite Paint(int w, int h, Func<float, float, Color> shader, Vector4 border, FilterMode filter = FilterMode.Bilinear)
        {
            var tex = NewTexture(w, h);
            tex.filterMode = filter;
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                px[y * w + x] = shader(x + 0.5f, y + 0.5f);
            tex.SetPixels32(px);
            tex.Apply(false, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        /// <summary>
        /// x が edge0 から edge1 へ進むにつれて 0 から 1 へ滑らかに変わる値（GLSL の smoothstep）。
        /// Unity の Mathf.SmoothStep は「from から to への補間」で意味が違うので、境界のぼかしにはこちらを使う。
        /// </summary>
        internal static float Smooth(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3 - 2 * t);
        }

        /// <summary>距離からアンチエイリアス付きの不透明度（0〜1）。</summary>
        internal static float Coverage(float distance) => Mathf.Clamp01(0.5f - distance);

        internal static string Key(Color c) => ColorUtility.ToHtmlStringRGBA(c);

        internal static byte Alpha(float distance) => (byte)(Mathf.Clamp01(0.5f - distance) * 255f);

        internal static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }
    }
}
