using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using Vector.EditorTools;

namespace Vector.Tests
{
    /// <summary>
    /// 画面の文言に使っている文字が、同梱の日本語フォントにすべて焼き込まれているかを確かめる。
    /// 失敗したらメニュー「Vector/Rebuild Japanese Font」を実行する。
    /// </summary>
    public class JapaneseFontCoverageTests
    {
        [Test]
        public void BundledFontCoversEveryUiCharacter()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(JapaneseFontBuilder.OutputPath);
            Assert.IsNotNull(font, "日本語フォントがありません。Vector/Rebuild Japanese Font を実行してください。");
            Assert.AreEqual(AtlasPopulationMode.Static, font.atlasPopulationMode);
            bool covered = font.HasCharacters(UiCharacterSet.Collect(), out List<char> missing);
            Assert.IsTrue(covered, $"日本語フォントに無い文字: {string.Concat(missing)}（Vector/Rebuild Japanese Font を実行してください）");
        }

        [Test]
        public void BundledFontIsRegisteredAsGlobalFallback()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(JapaneseFontBuilder.OutputPath);
            Assert.That(TMP_Settings.fallbackFontAssets, Does.Contain(font));
        }
    }
}
