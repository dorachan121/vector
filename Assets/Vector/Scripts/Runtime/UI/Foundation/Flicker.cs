using UnityEngine;
using UnityEngine.UI;

namespace Vector.Game.UI
{
    /// <summary>炎や光源のゆらぎ。画像の透明度を2つの周期の揺れで変える。</summary>
    [RequireComponent(typeof(Graphic))]
    public sealed class Flicker : MonoBehaviour
    {
        Graphic graphic;
        Color color;
        float phase;

        public static void Add(Graphic target)
        {
            var flicker = target.gameObject.AddComponent<Flicker>();
            flicker.graphic = target;
            flicker.color = target.color;
            flicker.phase = Random.value * 10f;
        }

        void Update()
        {
            float t = Time.unscaledTime + phase;
            float k = 0.82f + 0.12f * Mathf.Sin(t * 7f) + 0.06f * Mathf.Sin(t * 19f);
            graphic.color = color.WithAlpha(color.a * k);
        }
    }
}
