using UnityEngine;
using Vector.Core.Harpoon;
using Vector.Game.UI;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// 泳いでいる獲物1匹（絵と動き）。横へ泳ぎながら上下に揺れ、逃げるときは一瞬速くなる。動かすのは <see cref="Shoal"/>。
    /// </summary>
    public sealed class FishView
    {
        const float DashSeconds = 0.8f;
        const float DashBoost = 3.2f;

        readonly float velocity;
        readonly float baseY;
        readonly float amplitude;
        readonly float frequency;
        readonly float phase;
        float age;
        float dash;

        FishView(int quarry, RectTransform rect, Vector2 size, float velocity, float baseY, float amplitude, float frequency, float phase)
        {
            Quarry = quarry;
            Rect = rect;
            Size = size;
            this.velocity = velocity;
            this.baseY = baseY;
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.phase = phase;
        }

        public int Quarry { get; }
        public RectTransform Rect { get; }
        public Vector2 Size { get; }
        public Vector2 Position => Rect.anchoredPosition;

        /// <summary>タップで狙える範囲の半径。</summary>
        public float Radius => Mathf.Max(Size.x, Size.y) * 0.45f;

        /// <summary>モリが向かっている・捕まった（ほかの1発では狙えない）。</summary>
        public bool Hooked { get; set; }

        /// <summary>モリに捕まって泳ぎを止めた（あとは引き寄せられるだけ）。</summary>
        public bool Held { get; private set; }

        public void Hold()
        {
            Hooked = true;
            Held = true;
        }

        public static FishView Create(Transform parent, int quarry, Vector2 at, float velocity, System.Random random)
        {
            var spec = SwimSpec.Of(quarry);
            var rect = Ui.Rect(parent, "Fish" + quarry, at, spec.Size);
            if (quarry == Core.Harpoon.Quarry.Leviathan)
                Ui.Image(rect, "Aura", Art.Glow, HarpoonLabels.ColorOf(quarry).WithAlpha(0.35f), Vector2.zero, spec.Size * 1.4f);
            else if (quarry == Core.Harpoon.Quarry.Jellyfish)
                Ui.Image(rect, "Aura", Art.Glow, Palette.GoldLight.WithAlpha(0.5f), new Vector2(0, spec.Size.y * 0.2f), spec.Size * 1.6f);
            Ui.Image(rect, "Body", MarineArt.Of(quarry), Color.white, Vector2.zero, spec.Size);
            // クラゲは向きを変えず、ほかは泳ぐ向きに合わせて左右を反転する（絵は右向き）
            if (quarry != Core.Harpoon.Quarry.Jellyfish && velocity < 0)
                rect.localScale = new Vector3(-1, 1, 1);
            float amplitude = quarry == Core.Harpoon.Quarry.Jellyfish ? 46 : 8 + (float)random.NextDouble() * 14;
            float frequency = quarry == Core.Harpoon.Quarry.Jellyfish ? 1.1f : 1.4f + (float)random.NextDouble() * 1.2f;
            return new FishView(quarry, rect, spec.Size, velocity, at.y, amplitude, frequency, (float)random.NextDouble() * Mathf.PI * 2);
        }

        public void Step(float dt)
        {
            if (Held)
                return;
            age += dt;
            float boost = dash > 0 ? DashBoost : 1f;
            dash -= dt;
            var at = Rect.anchoredPosition;
            at.x += velocity * boost * dt;
            at.y = baseY + amplitude * Mathf.Sin(age * frequency + phase);
            Rect.anchoredPosition = at;
            if (Quarry == Core.Harpoon.Quarry.Jellyfish)
                Rect.localScale = new Vector3(1, 1 + 0.06f * Mathf.Sin(age * 4f + phase), 1);
            else
                Rect.localEulerAngles = new Vector3(0, 0, Mathf.Cos(age * frequency + phase) * amplitude * 0.25f * Mathf.Sign(velocity));
        }

        /// <summary>モリをかわして逃げる。</summary>
        public void Dart()
        {
            Hooked = false;
            dash = DashSeconds;
        }

        /// <summary>泳いで画面の外へ出きった。</summary>
        public bool IsGone(float halfWidth)
            => velocity > 0 ? Position.x - Size.x / 2 > halfWidth : Position.x + Size.x / 2 < -halfWidth;
    }
}
