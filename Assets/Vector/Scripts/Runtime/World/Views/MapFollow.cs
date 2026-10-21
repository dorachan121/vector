using UnityEngine;

namespace Vector.Game.World
{
    /// <summary>
    /// 見取り図を主人公に合わせて横にずらす（縦画面では見取り図の幅が画面に収まらないため）。
    /// 端まで来たらそれ以上はずらさず、画面のほうが広ければ真ん中に置いたままにする。
    /// </summary>
    public sealed class MapFollow : MonoBehaviour
    {
        /// <summary>追いつく速さ（1秒あたり、残りの距離のうち詰める割合の目安）。</summary>
        const float Sharpness = 10f;

        RectTransform map;
        RectTransform viewport;
        RectTransform target;
        Vector2 center;

        public static void Attach(RectTransform map, RectTransform viewport, RectTransform target)
        {
            var follow = map.gameObject.AddComponent<MapFollow>();
            follow.map = map;
            follow.viewport = viewport;
            follow.target = target;
            follow.center = map.anchoredPosition;
            follow.map.anchoredPosition = follow.Goal();
        }

        Vector2 Goal()
        {
            float slack = Mathf.Max(0, (map.sizeDelta.x - viewport.rect.width) / 2 + 24);
            return new Vector2(center.x + Mathf.Clamp(-target.anchoredPosition.x, -slack, slack), center.y);
        }

        void LateUpdate()
        {
            if (target == null)
                return;
            map.anchoredPosition = Vector2.Lerp(map.anchoredPosition, Goal(), 1 - Mathf.Exp(-Sharpness * Time.unscaledDeltaTime));
        }
    }
}
