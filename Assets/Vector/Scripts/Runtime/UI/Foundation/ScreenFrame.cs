using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// 画面の中身を基準解像度ちょうどの枠に入れ、端末の安全領域（ノッチやパンチホールを避けた範囲）に収まる最大の大きさで中央に置く。
    /// どの端末でも配置と比率が同じになり、縦横比の違いで余った所には背景が見える（背景は枠より大きく描いておく）。
    /// 画面の回転や安全領域の変化には毎フレーム追従する。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ScreenFrame : MonoBehaviour
    {
        /// <summary>枠の内側から、枠の外（余りの所）まで覆うときに広げる幅（暗幕や画面全体の演出用）。</summary>
        public const float Bleed = 2000f;

        Vector2 reference;
        RectTransform canvasRect;
        Vector2 appliedCanvas;
        Rect appliedSafe;

        public static ScreenFrame Attach(RectTransform rt, Vector2 reference)
        {
            var frame = rt.gameObject.AddComponent<ScreenFrame>();
            frame.reference = reference;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = reference;
            frame.canvasRect = (RectTransform)rt.GetComponentInParent<Canvas>().rootCanvas.transform;
            frame.Fit();
            return frame;
        }

        void LateUpdate() => Fit();

        void Fit()
        {
            var canvas = canvasRect.rect.size;
            var safe = NormalizedSafeArea();
            if (canvas == appliedCanvas && safe == appliedSafe)
                return;
            appliedCanvas = canvas;
            appliedSafe = safe;
            if (canvas.x <= 0 || canvas.y <= 0)
                return;
            var min = Vector2.Scale(safe.min, canvas);
            var max = Vector2.Scale(safe.max, canvas);
            var area = max - min;
            float scale = Mathf.Min(area.x / reference.x, area.y / reference.y);
            var rt = (RectTransform)transform;
            rt.anchoredPosition = (min + max) / 2 - canvas / 2;
            rt.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>安全領域を画面に対する割合で（取れなければ画面全体）。</summary>
        static Rect NormalizedSafeArea()
        {
            float width = Screen.width, height = Screen.height;
            var area = Screen.safeArea;
            if (width <= 0 || height <= 0 || area.width <= 0 || area.height <= 0)
                return new Rect(0, 0, 1, 1);
            return new Rect(area.x / width, area.y / height, area.width / width, area.height / height);
        }
    }
}
