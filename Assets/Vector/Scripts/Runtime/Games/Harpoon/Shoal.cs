using System;
using System.Collections.Generic;
using UnityEngine;
using Vector.Core.Harpoon;
using Vector.Game.UI;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// 海を泳ぐ獲物の群れ（出現・移動・退場）と、泡と海藻の揺れ。どの獲物がいつ現れるかは演出用の乱数で決め、
    /// 捕まえる確率には関係しない（確率は狙った獲物の種類だけで決まる）。
    /// </summary>
    public sealed class Shoal : MonoBehaviour
    {
        const int MaxFish = 13;
        const int BubbleCount = 26;

        /// <summary>押した位置から獲物の縁までこの距離なら、その獲物を狙ったことにする（指で押しやすく）。</summary>
        const float TapSlack = 50f;

        /// <summary>砂地の高さ（獲物はこの上を泳ぐ）。</summary>
        const float SeabedHeight = 80f;

        static readonly int[] Spawnable =
        {
            Quarry.Sardine, Quarry.Clownfish, Quarry.Puffer, Quarry.Turtle, Quarry.Swordfish, Quarry.Shark, Quarry.Whale, Quarry.Jellyfish,
        };

        readonly List<FishView> fish = new List<FishView>();
        readonly List<(RectTransform rect, float speed, float phase)> bubbles = new List<(RectTransform, float, float)>();
        readonly List<(RectTransform rect, float phase)> kelp = new List<(RectTransform, float)>();
        readonly System.Random visual = new System.Random();
        RectTransform field;
        RectTransform layer;
        Vector2 half;
        float nextSpawn;
        float nextLeviathan;
        float clock;

        /// <summary>獲物が押された（押した位置の近くに獲物がいたときだけ）。</summary>
        public event Action<FishView> Tapped;

        public static Shoal Create(RectTransform field)
        {
            var shoal = field.gameObject.AddComponent<Shoal>();
            shoal.field = field;
            shoal.half = field.sizeDelta / 2;
            var tap = Ui.Image(field, "TapArea", null, Color.clear, Vector2.zero, field.sizeDelta, true);
            tap.gameObject.AddComponent<OceanPress>().Pressed += shoal.OnPressed;
            shoal.BuildScenery();
            shoal.layer = Ui.Rect(field, "Fish", Vector2.zero, Vector2.zero);
            shoal.nextLeviathan = 15f + (float)shoal.visual.NextDouble() * 25f;
            for (int i = 0; i < 9; i++)
                shoal.Spawn(true);
            return shoal;
        }

        /// <summary>海の底（砂地・岩・海藻）と、差し込む光と泡。</summary>
        void BuildScenery()
        {
            for (int i = 0; i < 5; i++)
            {
                var beam = Ui.Image(field, "LightBeam", Art.VerticalFade, Color.white.WithAlpha(0.06f), new Vector2(-760 + i * 380, half.y - 260), new Vector2(150, 640));
                beam.rectTransform.localEulerAngles = new Vector3(0, 0, -14 + i * 4);
            }
            for (int i = 0; i < BubbleCount; i++)
            {
                float size = 8 + (float)visual.NextDouble() * 16;
                var bubble = Ui.Image(field, "Bubble", Art.Ring(0.16f), Color.white.WithAlpha(0.35f), RandomBubbleStart(true), Vector2.one * size);
                bubbles.Add((bubble.rectTransform, 50 + (float)visual.NextDouble() * 70, (float)visual.NextDouble() * 6f));
            }
            float bed = -half.y + SeabedHeight / 2;
            Ui.Image(field, "Sand", Chrome.Gradient(Theme.Hex("C2A878").WithAlpha(0.5f), Theme.Hex("6E5A3A").WithAlpha(0.65f)), Color.white,
                new Vector2(0, bed), new Vector2(half.x * 2, SeabedHeight));
            for (int i = 0; i < 9; i++)
            {
                float x = -half.x + 110 + i * (half.x * 2 - 220) / 8f;
                Ui.Image(field, "Rock", Art.Circle, Theme.Hex("3A4A58"), new Vector2(x + 30, bed + 10), new Vector2(70 + (i % 3) * 30, 46));
                var blade = Ui.Image(field, "Kelp", MarineArt.Kelp, (i % 2 == 0 ? Theme.Hex("2E8A5A") : Theme.Hex("4FA36A")).WithAlpha(0.85f),
                    new Vector2(x - 20, bed), new Vector2(46, 170 + (i % 4) * 40));
                blade.rectTransform.pivot = new Vector2(0.5f, 0);
                kelp.Add((blade.rectTransform, i * 0.9f));
            }
        }

        Vector2 RandomBubbleStart(bool anywhere)
        {
            float x = -half.x + (float)visual.NextDouble() * half.x * 2;
            float y = anywhere ? -half.y + (float)visual.NextDouble() * half.y * 2 : -half.y + SeabedHeight / 2;
            return new Vector2(x, y);
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime * Tween.Speed;
            clock += dt;
            nextSpawn -= dt;
            nextLeviathan -= dt;
            if (nextSpawn <= 0)
            {
                Spawn(false);
                nextSpawn = 0.55f + (float)visual.NextDouble() * 0.7f;
            }

            for (int i = fish.Count - 1; i >= 0; i--)
            {
                var f = fish[i];
                f.Step(dt);
                if (!f.Hooked && f.IsGone(half.x))
                {
                    Destroy(f.Rect.gameObject);
                    fish.RemoveAt(i);
                }
            }
            for (int i = 0; i < bubbles.Count; i++)
            {
                var (rect, speed, phase) = bubbles[i];
                var at = rect.anchoredPosition + new Vector2(Mathf.Sin(clock * 2f + phase) * 12f * dt, speed * dt);
                rect.anchoredPosition = at.y > half.y ? RandomBubbleStart(false) : at;
            }
            foreach (var (rect, phase) in kelp)
                rect.localEulerAngles = new Vector3(0, 0, 7f * Mathf.Sin(clock * 0.9f + phase));
        }

        /// <summary>獲物を1匹（イワシは群れで）出す。scatter なら最初から画面のどこかにいる状態にする。</summary>
        void Spawn(bool scatter)
        {
            if (fish.Count >= MaxFish)
                return;
            int quarry = PickQuarry();
            var spec = SwimSpec.Of(quarry);
            float direction = visual.Next(2) == 0 ? 1 : -1;
            float speed = (spec.MinSpeed + (float)visual.NextDouble() * (spec.MaxSpeed - spec.MinSpeed)) * direction;
            float low = -half.y + SeabedHeight + spec.Size.y / 2 + 10;
            float high = half.y - spec.Size.y / 2 - 30;
            float y = low + (float)visual.NextDouble() * Mathf.Max(0, high - low);
            float x = scatter
                ? -half.x * 0.8f + (float)visual.NextDouble() * half.x * 1.6f
                : -direction * (half.x + spec.Size.x / 2 + 10);
            int count = spec.School > 1 ? 1 + visual.Next(spec.School) : 1;
            for (int i = 0; i < count && fish.Count < MaxFish; i++)
            {
                var offset = i == 0 ? Vector2.zero : new Vector2(-direction * (50 + visual.Next(90)) * i * 0.7f, visual.Next(-50, 51));
                var at = new Vector2(x, Mathf.Clamp(y + offset.y, low, high)) + new Vector2(offset.x, 0);
                fish.Add(FishView.Create(layer, quarry, at, speed, visual));
            }
        }

        int PickQuarry()
        {
            if (nextLeviathan <= 0 && !fish.Exists(f => f.Quarry == Quarry.Leviathan))
            {
                nextLeviathan = 35f + (float)visual.NextDouble() * 35f;
                return Quarry.Leviathan;
            }
            bool whaleSwimming = fish.Exists(f => f.Quarry == Quarry.Whale);
            float total = 0;
            foreach (int q in Spawnable)
            {
                if (!(q == Quarry.Whale && whaleSwimming))
                    total += SwimSpec.Of(q).Weight;
            }
            float pick = (float)visual.NextDouble() * total;
            foreach (int q in Spawnable)
            {
                if (q == Quarry.Whale && whaleSwimming)
                    continue;
                pick -= SwimSpec.Of(q).Weight;
                if (pick <= 0)
                    return q;
            }
            return Quarry.Sardine;
        }

        void OnPressed(Vector2 point)
        {
            var target = At(point);
            if (target != null)
                Tapped?.Invoke(target);
        }

        /// <summary>押した位置の近くにいる、狙える獲物（重なっていれば近いほう）。</summary>
        public FishView At(Vector2 point)
        {
            FishView best = null;
            float bestGap = float.MaxValue;
            foreach (var f in fish)
            {
                if (!IsTargetable(f))
                    continue;
                float gap = Vector2.Distance(point, f.Position) - f.Radius;
                if (gap < TapSlack && gap < bestGap)
                {
                    best = f;
                    bestGap = gap;
                }
            }
            return best;
        }

        /// <summary>その種類の狙える獲物のうち、from に最も近いもの（LOCK で自動で狙う相手）。</summary>
        public FishView Nearest(int quarry, Vector2 from)
        {
            FishView best = null;
            float bestDistance = float.MaxValue;
            foreach (var f in fish)
            {
                if (f.Quarry != quarry || !IsTargetable(f))
                    continue;
                float distance = Vector2.Distance(from, f.Position);
                if (distance < bestDistance)
                {
                    best = f;
                    bestDistance = distance;
                }
            }
            return best;
        }

        /// <summary>画面の中にいて、まだモリが向かっていない。</summary>
        public bool IsTargetable(FishView f) => !f.Hooked && Mathf.Abs(f.Position.x) < half.x - 40 && fish.Contains(f);

        /// <summary>捕まえた獲物を海から取り除く。</summary>
        public void Remove(FishView f)
        {
            fish.Remove(f);
            if (f.Rect != null)
                Destroy(f.Rect.gameObject);
        }
    }
}
