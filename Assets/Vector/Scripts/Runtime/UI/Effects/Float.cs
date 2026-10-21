using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>ふわふわと上下に揺れる（キャラクター・吹き出し・宝石）。</summary>
    public sealed class Float : MonoBehaviour
    {
        Vector2 origin;
        float height;
        float speed;
        float phase;

        public static Float Add(RectTransform target, float height = 8f, float speed = 1.6f)
        {
            var f = target.gameObject.AddComponent<Float>();
            f.origin = target.anchoredPosition;
            f.height = height;
            f.speed = speed;
            f.phase = Random.value * 6f;
            return f;
        }

        void Update() => ((RectTransform)transform).anchoredPosition = origin + new Vector2(0, Mathf.Sin(Time.unscaledTime * speed + phase) * height);
    }
}
