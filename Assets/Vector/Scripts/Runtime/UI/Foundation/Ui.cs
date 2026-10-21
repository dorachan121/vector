using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vector.Game.UI
{
    /// <summary>
    /// uGUI をコードで組み立てるための小さなヘルパー群。座標は親の中心を原点とした px（1920×1080基準）で指定する。
    /// </summary>
    public static class Ui
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        /// <summary>角丸スプライトの元画像の半径。表示上の半径は pixelsPerUnitMultiplier で調整する。</summary>
        const int SpriteRadius = 32;

        /// <summary>スクリーン全体を覆うキャンバスを作る。</summary>
        static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return canvas;
        }

        /// <summary>
        /// 1画面ぶんの土台を作る。背景は画面いっぱいに敷き、中身の親は安全領域（ノッチを避けた範囲）に収める。
        /// </summary>
        public static RectTransform CreateScreen(string name)
        {
            var canvas = CreateCanvas(name + "Canvas", 0);
            var backdrop = Image(canvas.transform, "Backdrop", Art.Backdrop, Color.white, Vector2.zero, Vector2.zero);
            backdrop.rectTransform.anchorMin = Vector2.zero;
            backdrop.rectTransform.anchorMax = Vector2.one;
            var root = Stretch(canvas.transform, name);
            root.gameObject.AddComponent<SafeAreaFitter>();
            return root;
        }

        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>親いっぱいに広がる RectTransform。</summary>
        public static RectTransform Stretch(Transform parent, string name)
        {
            var rt = Rect(parent, name, Vector2.zero, Vector2.zero);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            return rt;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Color color, Vector2 position, Vector2 size, bool raycast = false)
        {
            var img = Rect(parent, name, position, size).gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null && sprite.border != Vector4.zero)
                img.type = UnityEngine.UI.Image.Type.Sliced;
            return img;
        }

        /// <summary>角丸パネル。radius は表示上の角の半径（px）。</summary>
        public static Image Panel(Transform parent, string name, Vector2 position, Vector2 size, Color color, float radius = 18, bool raycast = false)
        {
            var img = Image(parent, name, Art.RoundedRect(SpriteRadius), color, position, size, raycast);
            img.pixelsPerUnitMultiplier = SpriteRadius / Mathf.Max(1f, radius);
            return img;
        }

        /// <summary>角丸の枠線。</summary>
        public static Image Outline(Transform parent, string name, Vector2 position, Vector2 size, Color color, float radius = 18)
        {
            var img = Image(parent, name, Art.RoundedOutline(SpriteRadius, 3f), color, position, size);
            img.pixelsPerUnitMultiplier = SpriteRadius / Mathf.Max(1f, radius);
            return img;
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color,
            Vector2 position, Vector2 box, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var tmp = Rect(parent, name, position, box).gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.text = text;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        /// <summary>折り返しありの本文テキスト。</summary>
        public static TextMeshProUGUI Paragraph(Transform parent, string name, string text, float size, Color color,
            Vector2 position, Vector2 box, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            var tmp = Text(parent, name, text, Fonts.Body, size, color, position, box, align);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.lineSpacing = 12;
            return tmp;
        }

        /// <summary>
        /// 光沢のあるボタン。背景色が不透明なら宝石のような光沢、半透明なら暗いガラスの控えめなボタンになる。
        /// 影・艶・縁はボタンの子に置くので、ボタンごと隠したり動かしたりできる。
        /// </summary>
        public static CasinoButton Button(Transform parent, string name, string label, Vector2 position, Vector2 size,
            Color background, Color textColor, Action onClick, float fontSize = 30, float radius = 14)
        {
            var root = Rect(parent, name, position, size);
            bool glass = background.a < 0.5f;
            Shadow(root, "Shadow", new Vector2(0, -5), size, radius, glass ? 0.35f : 0.55f);
            var face = glass ? Palette.Violet.WithAlpha(0.88f) : background;
            var bg = Sliced(root, "Face", glass ? Chrome.Shade : Chrome.Gloss, face, Vector2.zero, size, radius, true);
            if (!glass)
                Sliced(root, "Shine", Chrome.Shine, Color.white, Vector2.zero, size, radius);
            Sliced(root, "Rim", Chrome.Rim, glass ? Palette.Muted.WithAlpha(0.45f) : Color.Lerp(background, Color.white, 0.55f).WithAlpha(0.9f), Vector2.zero, size, radius);
            var text = Text(root, "Label", label, Fonts.Label, fontSize, textColor, new Vector2(0, 1), size);
            if (textColor.grayscale > 0.5f)
                Outline(text, Color.Lerp(face, Color.black, 0.65f), 0.18f);
            var button = root.gameObject.AddComponent<CasinoButton>();
            button.Init(bg, text, face, onClick);
            return button;
        }

        /// <summary>丸い光沢のアイコンボタン（戻る・設定・ヘルプなど）。</summary>
        public static CasinoButton IconButton(Transform parent, string name, IconKind icon, Vector2 position, float diameter, Color background, Action onClick)
        {
            var button = Button(parent, name, "", position, Vector2.one * diameter, background, Palette.Ivory, onClick, 10, diameter / 2);
            var glyph = Image(button.transform, "Icon", Icons.Of(icon), background.a < 0.5f || background.grayscale < 0.6f ? Palette.Ivory : Palette.Ink, Vector2.zero, Vector2.one * diameter * 0.5f);
            glyph.transform.SetSiblingIndex(button.Label.transform.GetSiblingIndex());
            return button;
        }

        /// <summary>9スライスの角丸スプライトを、表示上の半径 radius で置く（Chrome の各スプライト用）。</summary>
        public static Image Sliced(Transform parent, string name, Sprite sprite, Color color, Vector2 position, Vector2 size, float radius, bool raycast = false)
        {
            var img = Image(parent, name, sprite, color, position, size, raycast);
            img.pixelsPerUnitMultiplier = Chrome.Radius / Mathf.Max(1f, radius);
            return img;
        }

        /// <summary>柔らかい影。中身の大きさ size と半径 radius を渡すと、ぼかしのぶん広げて置く。</summary>
        public static Image Shadow(Transform parent, string name, Vector2 position, Vector2 size, float radius, float alpha = 0.5f)
        {
            float multiplier = Chrome.Radius / Mathf.Max(1f, radius);
            float blur = Chrome.ShadowBlur / multiplier;
            var img = Image(parent, name, Chrome.SoftShadow, new Color(0, 0, 0, alpha), position, size + Vector2.one * blur * 2);
            img.pixelsPerUnitMultiplier = multiplier;
            return img;
        }

        /// <summary>
        /// 立体的なプレート（影・上が明るい地・金の額縁）。カード・ダイアログ・ヘッダーの土台。返すのは地の Image。
        /// </summary>
        public static Image Plate(Transform parent, string name, Vector2 position, Vector2 size, Color color, float radius = 24, bool frame = true, bool raycast = false)
        {
            var root = Rect(parent, name, position, size);
            Shadow(root, "Shadow", new Vector2(0, -8), size, radius, 0.6f);
            var body = Sliced(root, "Body", Chrome.Shade, color, Vector2.zero, size, radius, raycast);
            if (frame)
                Sliced(root, "Frame", Chrome.Frame, Palette.Gold, Vector2.zero, size, radius);
            return body;
        }

        /// <summary>見出しのリボン（左右に切れ込みのある帯）と、その上の文字。</summary>
        public static TextMeshProUGUI Ribbon(Transform parent, string name, string text, Vector2 position, float width, Color color, float fontSize = 34)
        {
            var size = new Vector2(width, width / 512f * 128f);
            float height = Mathf.Min(size.y, 96);
            size = new Vector2(width, height);
            var img = Image(parent, name, Chrome.Ribbon, color, position, size);
            img.pixelsPerUnitMultiplier = 128f / height;
            var label = Text(img.transform, "Label", text, Fonts.Label, fontSize, Palette.Ivory, new Vector2(0, height * 0.1f), new Vector2(width * 0.75f, height * 0.6f));
            Outline(label, Color.Lerp(color, Color.black, 0.7f), 0.2f);
            return label;
        }

        /// <summary>アイコン（<see cref="Icons"/>）。</summary>
        public static Image Icon(Transform parent, string name, IconKind icon, Color color, Vector2 position, float size)
            => Image(parent, name, Icons.Of(icon), color, position, Vector2.one * size);

        /// <summary>文字に縁取りを付ける（明るい文字を色の濃い地に載せるとき）。</summary>
        public static TextMeshProUGUI Outline(TextMeshProUGUI text, Color color, float width)
        {
            text.outlineColor = color;
            text.outlineWidth = width;
            return text;
        }

        /// <summary>金の上下グラデーションの大きな見出し文字（縁取り付き）。</summary>
        public static TextMeshProUGUI Headline(Transform parent, string name, string text, TMP_FontAsset font, float size, Vector2 position, Vector2 box, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var tmp = Text(parent, name, text, font, size, Color.white, position, box, align);
            tmp.enableVertexGradient = true;
            tmp.colorGradient = new VertexGradient(Palette.GoldLight, Palette.GoldLight, Palette.GoldMid, Palette.GoldDark);
            return Outline(tmp, Palette.Ink, 0.16f);
        }

        public static CanvasGroup Group(GameObject go)
        {
            var group = go.GetComponent<CanvasGroup>();
            return group != null ? group : go.AddComponent<CanvasGroup>();
        }
    }
}
