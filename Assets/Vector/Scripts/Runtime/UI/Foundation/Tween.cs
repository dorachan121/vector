using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vector.Game.UI
{
    /// <summary>
    /// 軽量なトゥイーン。外部ライブラリを使わず、1つのランナーが毎フレームまとめて進める。
    /// 同じ対象・同じチャンネルに新しいトゥイーンを掛けると古いものは打ち切られる。
    /// 対象が破棄されたトゥイーンは自動的に消える。
    /// </summary>
    public static class Tween
    {
        sealed class Job
        {
            public UnityEngine.Object Target;
            public string Channel;
            public float Delay;
            public float Duration;
            public float Elapsed;
            public Ease Ease;
            public Action<float> Step;
            public Action Done;
            public bool Cancelled;
        }

        sealed class Runner : MonoBehaviour
        {
            readonly List<Job> jobs = new List<Job>();
            readonly List<Job> pending = new List<Job>();

            public void Add(Job job) => pending.Add(job);

            void Update()
            {
                jobs.AddRange(pending);
                pending.Clear();
                for (int i = jobs.Count - 1; i >= 0; i--)
                {
                    var job = jobs[i];
                    if (job.Cancelled || job.Target == null)
                    {
                        Forget(job);
                        jobs.RemoveAt(i);
                        continue;
                    }
                    float dt = Time.unscaledDeltaTime * Speed;
                    if (job.Delay > 0)
                    {
                        job.Delay -= dt;
                        if (job.Delay > 0)
                            continue;
                        dt = -job.Delay;
                    }
                    job.Elapsed += dt;
                    float t = job.Duration <= 0 ? 1 : Mathf.Clamp01(job.Elapsed / job.Duration);
                    job.Step(Evaluate(job.Ease, t));
                    if (t < 1)
                        continue;
                    Forget(job);
                    jobs.RemoveAt(i);
                    job.Done?.Invoke();
                }
            }
        }

        static Runner runner;
        static readonly Dictionary<(UnityEngine.Object, string), Job> active = new Dictionary<(UnityEngine.Object, string), Job>();

        /// <summary>演出全体の速度倍率（設定の「高速ディール」で上げる）。</summary>
        public static float Speed { get; set; } = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            runner = null;
            active.Clear();
            Speed = 1f;
        }

        /// <summary>0→1 の値を ease 付きで step に渡し続ける汎用トゥイーン。</summary>
        public static void Run(UnityEngine.Object target, string channel, float duration, Ease ease, Action<float> step, Action done = null, float delay = 0)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            Kill(target, channel);
            var job = new Job
            {
                Target = target,
                Channel = channel,
                Duration = duration,
                Ease = ease,
                Step = step ?? throw new ArgumentNullException(nameof(step)),
                Done = done,
                Delay = delay,
            };
            active[(target, channel)] = job;
            GetRunner().Add(job);
        }

        static void Kill(UnityEngine.Object target, string channel)
        {
            if (active.TryGetValue((target, channel), out var job))
            {
                job.Cancelled = true;
                active.Remove((target, channel));
            }
        }

        /// <summary>対象に掛かっているトゥイーンをすべて止める（途中の値のまま）。</summary>
        public static void Stop(UnityEngine.Object target)
        {
            foreach (var key in new List<(UnityEngine.Object, string)>(active.Keys))
            {
                if (key.Item1 == target)
                    Kill(key.Item1, key.Item2);
            }
        }

        public static void Move(RectTransform rt, Vector2 to, float duration, Ease ease = Ease.OutCubic, Action done = null, float delay = 0)
        {
            Vector2? from = null;
            Run(rt, "move", duration, ease, t =>
            {
                from ??= rt.anchoredPosition;
                rt.anchoredPosition = Vector2.LerpUnclamped(from.Value, to, t);
            }, done, delay);
        }

        public static void Scale(Transform tr, Vector3 to, float duration, Ease ease = Ease.OutCubic, Action done = null, float delay = 0)
        {
            Vector3? from = null;
            Run(tr, "scale", duration, ease, t =>
            {
                from ??= tr.localScale;
                tr.localScale = Vector3.LerpUnclamped(from.Value, to, t);
            }, done, delay);
        }

        public static void Rotate(Transform tr, float toZ, float duration, Ease ease = Ease.OutCubic, Action done = null, float delay = 0)
        {
            float? from = null;
            Run(tr, "rotate", duration, ease, t =>
            {
                from ??= tr.localEulerAngles.z;
                tr.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(from.Value, toZ, t));
            }, done, delay);
        }

        public static void Fade(CanvasGroup group, float to, float duration, Ease ease = Ease.OutQuad, Action done = null, float delay = 0)
        {
            float? from = null;
            Run(group, "fade", duration, ease, t =>
            {
                from ??= group.alpha;
                group.alpha = Mathf.LerpUnclamped(from.Value, to, t);
            }, done, delay);
        }

        public static void Tint(Graphic graphic, Color to, float duration, Ease ease = Ease.OutQuad, Action done = null, float delay = 0)
        {
            Color? from = null;
            Run(graphic, "color", duration, ease, t =>
            {
                from ??= graphic.color;
                graphic.color = Color.LerpUnclamped(from.Value, to, t);
            }, done, delay);
        }

        /// <summary>コルーチン内で待つ（演出速度の倍率を反映）。</summary>
        public static IEnumerator Wait(float seconds)
        {
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime * Speed)
                yield return null;
        }

        static Runner GetRunner()
        {
            if (runner == null)
            {
                var go = new GameObject("[Tween]") { hideFlags = HideFlags.HideInHierarchy };
                UnityEngine.Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<Runner>();
            }
            return runner;
        }

        static void Forget(Job job)
        {
            var key = (job.Target, job.Channel);
            if (active.TryGetValue(key, out var current) && current == job)
                active.Remove(key);
        }

        static float Evaluate(Ease ease, float t)
        {
            switch (ease)
            {
                case Ease.OutQuad:
                    return 1 - (1 - t) * (1 - t);
                case Ease.InOutQuad:
                    return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
                case Ease.OutCubic:
                    return 1 - Mathf.Pow(1 - t, 3);
                case Ease.InCubic:
                    return t * t * t;
                case Ease.OutBack:
                    const float c1 = 1.70158f, c3 = c1 + 1;
                    return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
                default:
                    return t;
            }
        }
    }
}
