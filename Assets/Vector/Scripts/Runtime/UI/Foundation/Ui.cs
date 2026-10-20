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
            // 文字の下端がまっすぐな字形（5 など）で、SDF の縁のぼかしが描画範囲で切れて細い線に見えるのを防ぐ
            tmp.extraPadding = true;
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

        /// <summary>カジノ風のボタン。背景・ラベル・押下時の拡縮とクリック音を備える。</summary>
        public static CasinoButton Button(Transform parent, string name, string label, Vector2 position, Vector2 size,
            Color background, Color textColor, Action onClick, float fontSize = 30, float radius = 14)
        {
            var bg = Panel(parent, name, position, size, background, radius, true);
            var text = Text(bg.transform, "Label", label, Fonts.Label, fontSize, textColor, Vector2.zero, size);
            var button = bg.gameObject.AddComponent<CasinoButton>();
            button.Init(bg, text, background, onClick);
            return button;
        }

        public static CanvasGroup Group(GameObject go)
        {
            var group = go.GetComponent<CanvasGroup>();
            return group != null ? group : go.AddComponent<CanvasGroup>();
        }
    }
}
