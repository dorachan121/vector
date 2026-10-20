using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// TextMesh Pro のフォント。表示用は同梱の Anton / Oswald（OFL）。
    /// 日本語はOSのフォントから実行時にフォールバックを作り、アセットには保存しない。
    /// </summary>
    public static class Fonts
    {
        static readonly string[] japaneseFamilies =
        {
            "Noto Sans JP", "Noto Sans CJK JP", "Yu Gothic UI", "Yu Gothic", "Meiryo UI", "Meiryo",
            "BIZ UDGothic", "Hiragino Sans", "Hiragino Kaku Gothic ProN", "MS Gothic",
        };

        static readonly string[] japaneseStyles = { "Bold", "Regular", "Medium" };

        /// <summary>差し替える前のフォールバック一覧（終了時に元へ戻す）。</summary>
        static readonly Dictionary<TMP_FontAsset, List<TMP_FontAsset>> originalFallbacks = new Dictionary<TMP_FontAsset, List<TMP_FontAsset>>();

        static TMP_FontAsset display;
        static TMP_FontAsset label;
        static TMP_FontAsset body;
        static bool initialized;

        /// <summary>ロゴや大きな数字に使う縦長の太字（Anton）。</summary>
        public static TMP_FontAsset Display
        {
            get
            {
                Init();
                return display;
            }
        }

        /// <summary>ボタンやラベルに使うカジノ看板風の書体（Oswald Bold）。</summary>
        public static TMP_FontAsset Label
        {
            get
            {
                Init();
                return label;
            }
        }

        /// <summary>説明文に使う書体（TextMesh Pro 既定の Liberation Sans）。</summary>
        public static TMP_FontAsset Body
        {
            get
            {
                Init();
                return body;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            initialized = false;
            originalFallbacks.Clear();
        }

        static void Init()
        {
            if (initialized)
                return;
            initialized = true;

            body = TMP_Settings.defaultFontAsset;
            display = Load("Anton SDF");
            label = Load("Oswald Bold SDF");

            var japanese = CreateJapaneseFallback();
            if (japanese == null)
                return;
            foreach (var font in new[] { display, label, body })
                AddFallback(font, japanese);
            Application.quitting += RestoreFallbacks;
        }

        static TMP_FontAsset Load(string name)
        {
            var font = Resources.Load<TMP_FontAsset>(name);
            if (font != null)
                return font;
            Log.Warn($"フォント {name} が見つからないため既定のフォントを使います。");
            return TMP_Settings.defaultFontAsset;
        }

        static void AddFallback(TMP_FontAsset font, TMP_FontAsset extra)
        {
            if (font == null || originalFallbacks.ContainsKey(font))
                return;
            originalFallbacks[font] = font.fallbackFontAssetTable;
            var list = font.fallbackFontAssetTable != null ? new List<TMP_FontAsset>(font.fallbackFontAssetTable) : new List<TMP_FontAsset>();
            list.Add(extra);
            font.fallbackFontAssetTable = list;
        }

        /// <summary>エディタでアセットに実行時フォントへの参照が残らないよう、元の一覧に戻す。</summary>
        static void RestoreFallbacks()
        {
            foreach (var pair in originalFallbacks)
            {
                if (pair.Key != null)
                    pair.Key.fallbackFontAssetTable = pair.Value;
            }
            originalFallbacks.Clear();
            initialized = false;
            Application.quitting -= RestoreFallbacks;
        }

        static TMP_FontAsset CreateJapaneseFallback()
        {
            foreach (var family in japaneseFamilies)
            {
                foreach (var style in japaneseStyles)
                {
                    var asset = TMP_FontAsset.CreateFontAsset(family, style, 72);
                    if (asset == null)
                        continue;
                    if (asset.HasCharacter('賭', true, true))
                    {
                        asset.name = $"{family} {style} (OS)";
                        return asset;
                    }
                    Object.Destroy(asset);
                }
            }
            Log.Warn("日本語フォントがOSに見つからないため、日本語テキストが表示できない可能性があります。");
            return null;
        }
    }
}
