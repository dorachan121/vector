using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Vector.EditorTools
{
    /// <summary>
    /// 日本語フォント（BIZ UDPGothic Bold / SIL OFL）から、ゲームで使う文字だけを焼き込んだ静的な SDF フォントを作り、
    /// TextMesh Pro 全体のフォールバックに登録する。端末のフォントに頼らないので、どの OS でも同じ見た目になる。
    /// <para>画面の文言に新しい文字が増えると、エディタの起動時とビルドの前に自動で作り直す（<see cref="EnsureUpToDate"/>）。
    /// 手動ではメニュー「Vector/Rebuild Japanese Font」。漏れは JapaneseFontCoverageTests が検出する。</para>
    /// </summary>
    public static class JapaneseFontBuilder
    {
        public const string OutputPath = "Assets/Vector/Fonts/Resources/BIZ UDPGothic SDF.asset";
        const string SourceFontPath = "Assets/Vector/Fonts/BIZUDPGothic-Bold.ttf";
        const string MobileShader = "TextMeshPro/Mobile/Distance Field";
        const int SamplingPointSize = 40;
        /// <summary>文字の間隔（SDF のにじみ幅）。サンプリングサイズの3割ほど取り、隣の文字と干渉しないようにする。</summary>
        const int Padding = 12;
        const int MinAtlasSize = 1024;
        const int MaxAtlasSize = 4096;

        /// <summary>最後に確かめた文字の一覧（Library はリポジトリに含めない、端末ごとの作業領域）。</summary>
        const string StampPath = "Library/VectorJapaneseFont.txt";

        /// <summary>
        /// 画面の文言の文字がフォントに無ければ作り直す。文字の一覧が前回と同じなら何もしない。
        /// </summary>
        public static void EnsureUpToDate()
        {
            // プレイ中に作り直すと、画面が使っているフォントが消えて文字が豆腐になる。プレイを終えた後の再読み込みで確かめる
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            string characters = UiCharacterSet.Collect();
            if (File.Exists(StampPath) && File.ReadAllText(StampPath, Encoding.UTF8) == characters)
                return;
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputPath);
            bool covered = font != null && font.HasCharacters(characters, out List<char> _);
            if (covered || Rebuild())
                File.WriteAllText(StampPath, characters, Encoding.UTF8);
        }

        [MenuItem("Vector/Rebuild Japanese Font")]
        static void RebuildFromMenu() => Rebuild();

        /// <summary>フォントを作り直す。作れたら true。</summary>
        public static bool Rebuild()
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (source == null)
            {
                Debug.LogError($"[Vector] 元のフォント {SourceFontPath} が見つかりません。");
                return false;
            }

            string characters = UiCharacterSet.Collect();
            var atlas = AtlasSizeFor(characters.Length);
            var font = TMP_FontAsset.CreateFontAsset(source, SamplingPointSize, Padding, GlyphRenderMode.SDFAA,
                atlas.x, atlas.y, AtlasPopulationMode.Dynamic, false);
            font.name = "BIZ UDPGothic SDF";
            font.TryAddCharacters(characters, out string missing);
            font.atlasPopulationMode = AtlasPopulationMode.Static;

            AssetDatabase.DeleteAsset(OutputPath);
            AssetDatabase.CreateAsset(font, OutputPath);
            var texture = font.atlasTexture;
            texture.name = font.name + " Atlas";
            AssetDatabase.AddObjectToAsset(texture, font);
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
                Debug.Log($"[Vector] 日本語フォントを作成しました（{characters.Length} 文字・{atlas.x}×{atlas.y}）: {OutputPath}");
            else
                Debug.LogWarning($"[Vector] フォントに入らなかった文字があります（元のフォントに無いか、アトラスの空き不足）: {missing}");
            return true;
        }

        /// <summary>
        /// 文字数に合うアトラスの大きさ。1文字はおよそ（サンプリングサイズ + 余白×2）の正方形なので、
        /// 詰め込みの無駄を見込んで 1.25 倍の面積が入る大きさにする（正方形と横長を交互に広げる）。
        /// </summary>
        static Vector2Int AtlasSizeFor(int characterCount)
        {
            long cell = SamplingPointSize + Padding * 2;
            long needed = (long)(characterCount * 1.25) * cell * cell;
            int width = MinAtlasSize, height = MinAtlasSize;
            while ((long)width * height < needed && height < MaxAtlasSize)
            {
                if (width == height)
                    width *= 2;
                else
                    height *= 2;
            }
            return new Vector2Int(width, height);
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
