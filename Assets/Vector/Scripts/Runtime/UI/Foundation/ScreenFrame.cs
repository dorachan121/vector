using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// 画面の中身の枠を、端末の安全領域（ノッチやパンチホールを避けた範囲）いっぱいに合わせる。
    /// 大きさは基準解像度が収まる倍率で決め、余った長さだけ枠を伸ばす。そのため部品の大きさと横の配置はどの端末でも同じで、
    /// 上下（横向きなら左右）の端に付けた部品は端末の端に付き、余った長さは真ん中に回る。
    /// 縦横比が <see cref="MaxAspect"/> を超えるほど細長い端末だけ、枠を伸ばしきらずに中央に置く（余りの所には背景が見える）。
    /// 画面の回転や安全領域の変化には毎フレーム追従する。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ScreenFrame : MonoBehaviour
    {
        /// <summary>枠の内側から、枠の外（余りの所）まで覆うときに広げる幅（暗幕や画面全体の演出用）。</summary>
        public const float Bleed = 2000f;

        /// <summary>枠を伸ばす上限の縦横比（長い辺 ÷ 短い辺）。21:9 の端末まで伸ばしきる。</summary>
        public const float MaxAspect = 2.4f;

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
            var size = area / scale;
            float limit = Mathf.Min(size.x, size.y) * MaxAspect;
            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(Mathf.Min(size.x, limit), Mathf.Min(size.y, limit));
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
