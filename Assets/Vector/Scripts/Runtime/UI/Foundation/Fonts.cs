using TMPro;
using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// TextMesh Pro のフォント。表示用は同梱の Anton / Oswald、本文は TextMesh Pro 既定の Liberation Sans。
    /// 日本語は同梱の BIZ UDGothic（ゲームで使う文字だけを焼き込んだもの）が TMP Settings の全体フォールバックとして補う。
    /// すべて SIL Open Font License のフォントで、端末のフォントには依存しない。
    /// </summary>
    public static class Fonts
    {
        static TMP_FontAsset display;
        static TMP_FontAsset label;

        /// <summary>ロゴや大きな数字に使う縦長の太字（Anton）。</summary>
        public static TMP_FontAsset Display
        {
            get
            {
                if (display == null)
                    display = Load("Anton SDF");
                return display;
            }
        }

        /// <summary>ボタンやラベルに使うカジノ看板風の書体（Oswald Bold）。</summary>
        public static TMP_FontAsset Label
        {
            get
            {
                if (label == null)
                    label = Load("Oswald Bold SDF");
                return label;
            }
        }

        /// <summary>説明文に使う書体（TextMesh Pro 既定の Liberation Sans）。</summary>
        public static TMP_FontAsset Body => TMP_Settings.defaultFontAsset;

        static TMP_FontAsset Load(string name)
        {
            var font = Resources.Load<TMP_FontAsset>(name);
            if (font != null)
                return font;
            Log.Warn($"フォント {name} が見つからないため既定のフォントを使います。");
            return TMP_Settings.defaultFontAsset;
        }
    }
}
