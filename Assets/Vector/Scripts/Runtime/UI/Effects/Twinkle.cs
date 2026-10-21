using UnityEngine;
using UnityEngine.UI;

namespace Vector.Game.UI
{
    /// <summary>範囲の中のあちこちで、きらめきが現れては消える（背景や宝物の演出）。</summary>
    public sealed class Twinkle : MonoBehaviour
    {
        Image[] sparkles;
        float[] ages;
        float[] lifetimes;
        Vector2 area;
        float maxSize;

        public static Twinkle Add(RectTransform target, int count, Color color, float maxSize = 46f)
        {
            var twinkle = target.gameObject.AddComponent<Twinkle>();
            twinkle.area = target.sizeDelta;
            twinkle.maxSize = maxSize;
            twinkle.sparkles = new Image[count];
            twinkle.ages = new float[count];
            twinkle.lifetimes = new float[count];
            for (int i = 0; i < count; i++)
            {
                twinkle.sparkles[i] = Ui.Image(target, "Sparkle", Chrome.Sparkle, color, Vector2.zero, Vector2.one * maxSize);
                twinkle.Respawn(i);
                twinkle.ages[i] = Random.value * twinkle.lifetimes[i];
            }
            return twinkle;
        }

        void Respawn(int i)
        {
            ages[i] = 0;
            lifetimes[i] = 1.2f + Random.value * 1.6f;
            var rt = sparkles[i].rectTransform;
            rt.anchoredPosition = new Vector2((Random.value - 0.5f) * area.x, (Random.value - 0.5f) * area.y);
            float size = maxSize * (0.45f + Random.value * 0.55f);
            rt.sizeDelta = Vector2.one * size;
        }

        void Update()
        {
            for (int i = 0; i < sparkles.Length; i++)
            {
                ages[i] += Time.unscaledDeltaTime;
                if (ages[i] >= lifetimes[i])
                    Respawn(i);
                float t = ages[i] / lifetimes[i];
                float k = Mathf.Sin(t * Mathf.PI);
                sparkles[i].rectTransform.localScale = Vector3.one * k;
                sparkles[i].rectTransform.localEulerAngles = new Vector3(0, 0, t * 90f);
            }
        }
    }
}
