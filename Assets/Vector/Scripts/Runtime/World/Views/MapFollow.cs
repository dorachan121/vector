using UnityEngine;

namespace Vector.Game.World
{
    /// <summary>
    /// 見取り図を主人公に合わせて動かし、主人公がいつも見える場所の真ん中あたりに来るようにする。
    /// 見取り図の端まで来たらそれ以上は動かさず、見える場所のほうが広い向きには動かさない。
    /// </summary>
    public sealed class MapFollow : MonoBehaviour
    {
        /// <summary>追いつく速さ（1秒あたり、残りの距離のうち詰める割合の目安）。</summary>
        const float Sharpness = 10f;

        /// <summary>見取り図の端の外側に見せる余白。</summary>
        const float Margin = 24f;

        RectTransform map;
        RectTransform viewport;
        RectTransform target;

        public static void Attach(RectTransform map, RectTransform viewport, RectTransform target)
        {
            var follow = map.gameObject.AddComponent<MapFollow>();
            follow.map = map;
            follow.viewport = viewport;
            follow.target = target;
            follow.map.anchoredPosition = follow.Goal();
        }

        Vector2 Goal()
        {
            var view = viewport.rect.size;
            return new Vector2(Axis(map.sizeDelta.x, view.x, target.anchoredPosition.x), Axis(map.sizeDelta.y, view.y, target.anchoredPosition.y));
        }

        /// <summary>1つの向きのずらし量。見える幅より大きいときだけ、主人公の逆へ端まで動かす。</summary>
        static float Axis(float mapSize, float viewSize, float player)
        {
            if (mapSize <= viewSize)
                return 0;
            float slack = (mapSize - viewSize) / 2 + Margin;
            return Mathf.Clamp(-player, -slack, slack);
        }

        void LateUpdate()
        {
            if (target == null)
                return;
            map.anchoredPosition = Vector2.Lerp(map.anchoredPosition, Goal(), 1 - Mathf.Exp(-Sharpness * Time.unscaledDeltaTime));
        }
    }
}
