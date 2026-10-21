using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vector.Game.UI
{
    /// <summary>
    /// 一度きりの粒子の演出（紙吹雪・金貨・きらめき）。粒子は Image で、速度・重力・回転・ひらひら・消え方を毎フレーム更新し、
    /// すべて消えたら自分ごと片付ける。
    /// </summary>
    public sealed class ParticleBurst : MonoBehaviour
    {
        sealed class Particle
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public float Spin;
            public float Age;
            public float Life;
            public float Flutter;
            public Color Color;
        }

        readonly List<Particle> particles = new List<Particle>();
        float gravity;
        float drag;

        /// <summary>中心から全方向に弾ける紙吹雪。</summary>
        public static void Confetti(Transform parent, Vector2 origin, int count = 90)
        {
            var colors = new[] { Palette.Gold, Palette.Magenta, Palette.Cyan, Palette.Emerald, Palette.Amethyst, Palette.Ivory };
            var burst = Create(parent, origin, 1400f, 1.2f);
            for (int i = 0; i < count; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                float speed = 500f + Random.value * 900f;
                var v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.8f + 0.55f) * speed;
                burst.Spawn(Chrome.Confetti, colors[i % colors.Length], new Vector2(16, 24) * (0.7f + Random.value * 0.6f), v, 2.2f + Random.value * 1.2f, (Random.value - 0.5f) * 900f, 6f + Random.value * 8f);
            }
        }

        /// <summary>上から降り注ぐ金貨（大勝ちの演出）。</summary>
        public static void Coins(Transform parent, Vector2 area, int count = 60)
        {
            var burst = Create(parent, Vector2.zero, 1500f, 0.2f);
            for (int i = 0; i < count; i++)
            {
                var start = new Vector2((Random.value - 0.5f) * area.x, area.y / 2 + 60 + Random.value * area.y * 0.8f);
                var v = new Vector2((Random.value - 0.5f) * 240f, -200f - Random.value * 400f);
                var p = burst.Spawn(Chrome.Coin, Color.white, Vector2.one * (44f + Random.value * 30f), v, 2.6f + Random.value * 0.8f, (Random.value - 0.5f) * 500f, 5f + Random.value * 6f);
                p.Rect.anchoredPosition = start;
            }
        }

        /// <summary>小さなきらめきが散る（獲得・成功の手応え）。</summary>
        public static void Sparkles(Transform parent, Vector2 origin, Color color, int count = 18, float speed = 420f)
        {
            var burst = Create(parent, origin, 0f, 2.2f);
            for (int i = 0; i < count; i++)
            {
                float angle = i / (float)count * Mathf.PI * 2 + Random.value * 0.3f;
                var v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed * (0.5f + Random.value * 0.7f);
                burst.Spawn(Chrome.Sparkle, color, Vector2.one * (30f + Random.value * 34f), v, 0.6f + Random.value * 0.5f, 0, 0);
            }
        }

        static ParticleBurst Create(Transform parent, Vector2 origin, float gravity, float drag)
        {
            var rt = Ui.Rect(parent, "Particles", origin, Vector2.zero);
            rt.SetAsLastSibling();
            var burst = rt.gameObject.AddComponent<ParticleBurst>();
            burst.gravity = gravity;
            burst.drag = drag;
            return burst;
        }

        Particle Spawn(Sprite sprite, Color color, Vector2 size, Vector2 velocity, float life, float spin, float flutter)
        {
            var image = Ui.Image(transform, "P", sprite, color, Vector2.zero, size);
            image.rectTransform.localEulerAngles = new Vector3(0, 0, Random.value * 360f);
            var p = new Particle { Rect = image.rectTransform, Image = image, Velocity = velocity, Spin = spin, Life = life, Flutter = flutter, Color = color };
            particles.Add(p);
            return p;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            bool alive = false;
            foreach (var p in particles)
            {
                if (p.Age >= p.Life)
                    continue;
                alive = true;
                p.Age += dt;
                p.Velocity *= Mathf.Max(0, 1 - drag * dt);
                p.Velocity.y -= gravity * dt;
                p.Rect.anchoredPosition += p.Velocity * dt;
                var e = p.Rect.localEulerAngles;
                p.Rect.localEulerAngles = new Vector3(0, 0, e.z + p.Spin * dt);
                if (p.Flutter > 0)
                    p.Rect.localScale = new Vector3(Mathf.Abs(Mathf.Cos(p.Age * p.Flutter)), 1, 1);
                float fade = Mathf.Clamp01((p.Life - p.Age) / 0.4f);
                p.Image.color = p.Color.WithAlpha(p.Color.a * fade);
                if (p.Age >= p.Life)
                    p.Image.enabled = false;
            }
            if (!alive)
                Destroy(gameObject);
        }
    }
}
