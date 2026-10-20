using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// スマートフォンのノッチやパンチホールを避けるため、RectTransform を Screen.safeArea に合わせる。
    /// 画面の回転などで安全領域が変わったら追従する。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        Rect applied;
        Vector2Int screen;

        void Update()
        {
            var area = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (area == applied && size == screen)
                return;
            applied = area;
            screen = size;
            if (size.x <= 0 || size.y <= 0)
                return;
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
            rt.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
