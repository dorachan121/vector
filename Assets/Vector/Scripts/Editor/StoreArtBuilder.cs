using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector.Game.UI;
using Vector.Game.World;

namespace Vector.EditorTools
{
    /// <summary>
    /// アプリのアイコンとストア掲載用の画像を、ゲームの絵柄（図形で描くチップ・光・ロゴ・立ち絵）から描き出す。
    /// メニュー「Vector/Build Store Art」。Android のアダプティブアイコン（背景と前景の2層）をプレイヤー設定に登録し、
    /// Google Play に載せるアイコン（512×512）とフィーチャーグラフィック（1024×500）を Store フォルダに書き出す。
    /// </summary>
    public static class StoreArtBuilder
    {
        const string IconFolder = "Assets/Vector/Icons";
        const string IconPath = IconFolder + "/AppIcon.png";
        const string BackgroundPath = IconFolder + "/AppIconBackground.png";
        const string ForegroundPath = IconFolder + "/AppIconForeground.png";
        const string StoreFolder = "Store";

        /// <summary>アイコンの元画像の大きさ。Unity が端末ごとの大きさに縮める。</summary>
        const int IconSize = 1024;

        /// <summary>
        /// アダプティブアイコンの前景のチップの直径（元画像に対する割合）。端末がどんな形に切り抜いても欠けない、
        /// 真ん中の安全な円（直径 66/108）に収める。
        /// </summary>
        const float AdaptiveEmblem = 0.56f;

        /// <summary>ストアのアイコンのチップの直径（角を丸く切り抜かれるだけなので大きく描く）。</summary>
        const float StoreEmblem = 0.74f;

        [MenuItem("Vector/Build Store Art")]
        public static void Build()
        {
            Directory.CreateDirectory(IconFolder);
            Directory.CreateDirectory(StoreFolder);
            Save(IconPath, Render(IconSize, IconSize, root => { Background(root, IconSize); Emblem(root, IconSize * StoreEmblem); }));
            Save(BackgroundPath, Render(IconSize, IconSize, root => Background(root, IconSize)));
            Save(ForegroundPath, RenderTransparent(IconSize, IconSize, root => Emblem(root, IconSize * AdaptiveEmblem)));
            Save(StoreFolder + "/icon-512.png", Render(512, 512, root => { Background(root, 512); Emblem(root, 512 * StoreEmblem); }));
            Save(StoreFolder + "/feature-graphic.png", Render(1024, 500, FeatureGraphic));
            AssetDatabase.Refresh();
            RegisterIcons();
            Debug.Log($"[Vector] アイコンを {IconFolder} に、ストア用の画像を {StoreFolder} に書き出しました。");
        }

        // ------------------------------------------------------------------
        // 絵柄
        // ------------------------------------------------------------------

        /// <summary>夜のカジノの紫のグラデーションと、真ん中から射す金の光。</summary>
        static void Background(RectTransform root, float size)
        {
            var sky = Ui.Image(root, "Sky", Chrome.Gradient(Palette.VioletBright, Palette.NightDeep), Color.white, Vector2.zero, Vector2.zero);
            Stretch(sky.rectTransform);
            Ui.Image(root, "Glow", Art.Glow, Palette.Magenta.WithAlpha(0.4f), Vector2.zero, Vector2.one * size);
            Ui.Image(root, "Rays", Chrome.Rays(20), Palette.Gold.WithAlpha(0.22f), Vector2.zero, Vector2.one * size * 1.6f);
        }

        /// <summary>赤いカジノチップに金の V（VECTOR の頭文字）。</summary>
        static void Emblem(RectTransform root, float diameter)
        {
            Ui.Shadow(root, "Shadow", new Vector2(0, -diameter * 0.03f), Vector2.one * diameter, diameter / 2, 0.6f);
            Ui.Image(root, "Chip", Art.Chip(Theme.Hex("C8202D"), Palette.Ivory, Mathf.CeilToInt(diameter)), Color.white, Vector2.zero, Vector2.one * diameter);
            var letter = Ui.Headline(root, "Letter", "V", Fonts.Display, diameter * 0.56f, new Vector2(0, -diameter * 0.015f), Vector2.one * diameter);
            Ui.Outline(letter, Palette.Ink, 0.24f);
        }

        /// <summary>ストアの一番上に出る横長の絵（左にロゴ、右に看板の2人）。</summary>
        static void FeatureGraphic(RectTransform root)
        {
            Background(root, 1024);
            PortraitView.Create(root, "Zero", Cast.Of("zero").Look, new Vector2(390, -110), 520);
            PortraitView.Create(root, "Lena", Cast.Of("lena").Look, new Vector2(240, -140), 520);
            Ui.Image(root, "Shade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0f), Palette.NightDeep.WithAlpha(0.85f)), Color.white, new Vector2(0, -200), new Vector2(1024, 100));

            var logo = Ui.Headline(root, "Logo", "VECTOR", Fonts.Display, 150, new Vector2(-200, 70), new Vector2(620, 170));
            logo.characterSpacing = 10;
            Ui.Outline(logo, Palette.Ink, 0.22f);
            var sub = Ui.Text(root, "Sub", "CASINO RESORT", Fonts.Label, 36, Palette.Ivory, new Vector2(-200, -30), new Vector2(620, 50));
            sub.characterSpacing = 18;
            Ui.Outline(sub, Palette.Ink, 0.2f);
            Ui.Ribbon(root, "Tagline", "TEN ORIGINAL GAMES  ·  CASINO RPG", new Vector2(-200, -110), 560, Palette.Ruby, 22);
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        // ------------------------------------------------------------------
        // 描き出し
        // ------------------------------------------------------------------

        /// <summary>
        /// 開いているシーンを汚さないよう、見えないプレビュー用のシーンにキャンバスを作って draw で描き、その絵を返す。
        /// </summary>
        static Texture2D Render(int width, int height, Action<RectTransform> draw, Color? clear = null)
        {
            var scene = EditorSceneManager.NewPreviewScene();
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            try
            {
                var camera = new GameObject("Camera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
                camera.scene = scene;
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = clear ?? Palette.NightDeep;
                camera.targetTexture = target;

                var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
                SceneManager.MoveGameObjectToScene(canvasObject, scene);
                var canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 10;
                var root = (RectTransform)canvas.transform;
                root.sizeDelta = new Vector2(width, height);

                draw(root);
                Canvas.ForceUpdateCanvases();
                foreach (var text in root.GetComponentsInChildren<TMP_Text>())
                    text.ForceMeshUpdate();
                camera.Render();

                var previous = RenderTexture.active;
                RenderTexture.active = target;
                var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                RenderTexture.active = previous;
                return image;
            }
            finally
            {
                RenderTexture.ReleaseTemporary(target);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        /// <summary>
        /// 背景を透かした絵（アダプティブアイコンの前景）。黒地と白地に1回ずつ描き、2枚の差から各画素の不透明度と本来の色を求める。
        /// </summary>
        static Texture2D RenderTransparent(int width, int height, Action<RectTransform> draw)
        {
            var black = Render(width, height, draw, Color.black).GetPixels();
            var white = Render(width, height, draw, Color.white).GetPixels();
            var pixels = new Color[black.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                var b = black[i];
                var w = white[i];
                float alpha = Mathf.Clamp01(1 - Mathf.Max(w.r - b.r, w.g - b.g, w.b - b.b));
                pixels[i] = alpha <= 0 ? Color.clear : new Color(Mathf.Clamp01(b.r / alpha), Mathf.Clamp01(b.g / alpha), Mathf.Clamp01(b.b / alpha), alpha);
            }
            var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
            image.SetPixels(pixels);
            image.Apply();
            return image;
        }

        static void Save(string path, Texture2D image)
        {
            File.WriteAllBytes(path, image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
        }

        // ------------------------------------------------------------------
        // アイコンの登録
        // ------------------------------------------------------------------

        /// <summary>Android のアダプティブアイコン（背景・前景）と、ほかの場面で使う既定のアイコンを登録する。</summary>
        static void RegisterIcons()
        {
            foreach (var path in new[] { IconPath, BackgroundPath, ForegroundPath })
                ConfigureImport(path);
            var full = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            var background = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
            var foreground = AssetDatabase.LoadAssetAtPath<Texture2D>(ForegroundPath);

            var android = NamedBuildTarget.Android;
            var adaptive = PlayerSettings.GetSupportedIconKinds(android).First(kind => kind.ToString() == "Adaptive");
            var icons = PlayerSettings.GetPlatformIcons(android, adaptive);
            foreach (var icon in icons)
                icon.SetTextures(background, foreground);
            PlayerSettings.SetPlatformIcons(android, adaptive, icons);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { full }, UnityEditor.IconKind.Any);
            AssetDatabase.SaveAssets();
        }

        static void ConfigureImport(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}
