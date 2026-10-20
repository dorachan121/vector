using TMPro;
using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// TextMesh Pro のフォント。表示用は同梱の Anton / Oswald、本文は同梱の BIZ UDPGothic（ゲームで使う文字だけを焼き込んだもの）。
    /// BIZ UDPGothic は TMP Settings の全体フォールバックにも登録してあり、Anton / Oswald の文言に混ざる日本語も補う。
    /// すべて SIL Open Font License のフォントで、端末のフォントには依存しない。
    /// </summary>
    public static class Fonts
    {
        static TMP_FontAsset display;
        static TMP_FontAsset label;
        static TMP_FontAsset body;

        /// <summary>ロゴや大きな数字に使う縦長の太字（Anton）。</summary>
        public static TMP_FontAsset Display => Cached(ref display, "Anton SDF");

        /// <summary>ボタンやラベルに使うカジノ看板風の書体（Oswald Bold）。</summary>
        public static TMP_FontAsset Label => Cached(ref label, "Oswald Bold SDF");

        /// <summary>説明文に使う書体（BIZ UDPGothic。日本語と英数字を同じ書体で表示する）。</summary>
        public static TMP_FontAsset Body => Cached(ref body, "BIZ UDPGothic SDF");

        static TMP_FontAsset Cached(ref TMP_FontAsset field, string name)
        {
            if (field == null)
                field = Load(name);
            return field;
        }

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
