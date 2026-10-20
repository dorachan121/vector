using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Vector.EditorTools
{
    /// <summary>
    /// 日本語フォント（BIZ UDPGothic Bold / SIL OFL）から、ゲームで使う文字だけを焼き込んだ静的な SDF フォントを作り、
    /// TextMesh Pro 全体のフォールバックに登録する。端末のフォントに頼らないので、どの OS でも同じ見た目になる。
    /// <para>画面の文言を追加・変更したら、メニュー「Vector/Rebuild Japanese Font」を実行すること
    /// （漏れは JapaneseFontCoverageTests が検出する）。</para>
    /// </summary>
    public static class JapaneseFontBuilder
    {
        public const string OutputPath = "Assets/Vector/Fonts/Resources/BIZ UDPGothic SDF.asset";
        const string SourceFontPath = "Assets/Vector/Fonts/BIZUDPGothic-Bold.ttf";
        const string MobileShader = "TextMeshPro/Mobile/Distance Field";
        const int SamplingPointSize = 40;
        /// <summary>文字の間隔（SDF のにじみ幅）。サンプリングサイズの3割ほど取り、隣の文字と干渉しないようにする。</summary>
        const int Padding = 12;
        const int AtlasWidth = 2048;
        const int AtlasHeight = 1024;

        [MenuItem("Vector/Rebuild Japanese Font")]
        public static void Rebuild()
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (source == null)
            {
                Debug.LogError($"[Vector] 元のフォント {SourceFontPath} が見つかりません。");
                return;
            }

            string characters = UiCharacterSet.Collect();
            var font = TMP_FontAsset.CreateFontAsset(source, SamplingPointSize, Padding, GlyphRenderMode.SDFAA,
                AtlasWidth, AtlasHeight, AtlasPopulationMode.Dynamic, false);
            font.name = "BIZ UDPGothic SDF";
            font.TryAddCharacters(characters, out string missing);
            font.atlasPopulationMode = AtlasPopulationMode.Static;

            AssetDatabase.DeleteAsset(OutputPath);
            AssetDatabase.CreateAsset(font, OutputPath);
            var atlas = font.atlasTexture;
            atlas.name = font.name + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, font);
            var material = font.material;
            material.name = font.name + " Material";
            material.shader = Shader.Find(MobileShader);
            AssetDatabase.AddObjectToAsset(material, font);
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();

            RegisterGlobalFallback(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputPath));
            // 元のフォントファイルを掴んだままだと、差し替えや削除ができなくなるため解放する
            FontEngine.UnloadAllFontFaces();

            if (string.IsNullOrEmpty(missing))
                Debug.Log($"[Vector] 日本語フォントを作成しました（{characters.Length} 文字）: {OutputPath}");
            else
                Debug.LogWarning($"[Vector] 元のフォントに無い文字があります: {missing}");
        }

        /// <summary>TMP Settings のフォールバック一覧に登録する（どのフォントで描いても日本語が出るように）。</summary>
        static void RegisterGlobalFallback(TMP_FontAsset font)
        {
            var settings = TMP_Settings.instance;
            var list = TMP_Settings.fallbackFontAssets ?? new List<TMP_FontAsset>();
            list.RemoveAll(f => f == null || f.name == font.name);
            list.Add(font);
            TMP_Settings.fallbackFontAssets = list;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }
}
