using System;
using UnityEngine;

namespace Vector.Game.Audio
{
    /// <summary>
    /// 効果音を実行時に合成する。音声ファイルを持たないので容量が増えず、ライセンスの心配もない。
    /// 乱数は固定シードなので、毎回同じ音になる。
    /// </summary>
    static class SfxSynth
    {
        const int SampleRate = 44100;

        public static AudioClip Create(SfxKind kind)
        {
            var random = new System.Random((int)kind * 7919 + 17);
            float[] data;
            switch (kind)
            {
                case SfxKind.Button:
                    data = Render(0.06f, (t, i) => Sine(t, 1700) * Decay(t, 60) * 0.35f);
                    break;
                case SfxKind.Chip:
                    data = ChipClack(random, 0f);
                    break;
                case SfxKind.ChipsCollect:
                    data = Mix(ChipClack(random, 0f), ChipClack(random, 0.05f), ChipClack(random, 0.11f), ChipClack(random, 0.16f));
                    break;
                case SfxKind.CardDeal:
                    data = Swish(random, 0.16f, 5200f, 900f, 0.5f);
                    break;
                case SfxKind.CardFlip:
                    data = Mix(Swish(random, 0.05f, 3000f, 1500f, 0.45f), Render(0.05f, (t, i) => Sine(t, 180) * Decay(t, 70) * 0.4f));
                    break;
                case SfxKind.Tick:
                    data = Render(0.05f, (t, i) => (Sine(t, 1050) + 0.5f * Sine(t, 2100)) * Decay(t, 90) * 0.3f);
                    break;
                case SfxKind.Win:
                    data = Arpeggio(new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.085f, 0.55f, 0.32f);
                    break;
                case SfxKind.BigWin:
                    data = Mix(Arpeggio(new[] { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f, 1568f }, 0.07f, 0.9f, 0.3f), Shimmer(random, 1.1f));
                    break;
                case SfxKind.Lose:
                    data = Arpeggio(new[] { 392f, 311.13f }, 0.16f, 0.45f, 0.22f);
                    break;
                case SfxKind.Push:
                    data = Arpeggio(new[] { 587.33f, 587.33f }, 0.12f, 0.35f, 0.22f);
                    break;
                case SfxKind.Shuffle:
                    data = Riffle(random, 0.9f);
                    break;
                default:
                    data = Render(0.18f, (t, i) => Math.Sign(Sine(t, 110)) * Decay(t, 14) * 0.18f);
                    break;
            }
            var clip = AudioClip.Create("sfx_" + kind, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float[] Render(float seconds, Func<float, int, float> sample)
        {
            var data = new float[Mathf.CeilToInt(seconds * SampleRate)];
            for (int i = 0; i < data.Length; i++)
                data[i] = sample(i / (float)SampleRate, i);
            return data;
        }

        static float Sine(float t, float hz) => Mathf.Sin(2 * Mathf.PI * hz * t);

        static float Decay(float t, float rate) => Mathf.Exp(-t * rate);

        /// <summary>粘土チップがぶつかる短い「カチッ」。ノイズの立ち上がりと高い共振音。</summary>
        static float[] ChipClack(System.Random random, float offset)
        {
            float pitch = 1f + (float)(random.NextDouble() - 0.5) * 0.12f;
            return Render(offset + 0.09f, (t, i) =>
            {
                float local = t - offset;
                if (local < 0)
                    return 0;
                float noise = (float)(random.NextDouble() * 2 - 1) * Decay(local, 220);
                float ring = (Sine(local, 3300 * pitch) * 0.6f + Sine(local, 5200 * pitch) * 0.3f) * Decay(local, 75);
                return (noise * 0.5f + ring) * 0.38f;
            });
        }

        /// <summary>カードが滑る「シュッ」。帯域を下げていくノイズ。</summary>
        static float[] Swish(System.Random random, float seconds, float fromHz, float toHz, float gain)
        {
            float low = 0, band = 0;
            return Render(seconds, (t, i) =>
            {
                float p = t / seconds;
                float cutoff = Mathf.Lerp(fromHz, toHz, p);
                float f = 2 * Mathf.Sin(Mathf.PI * cutoff / SampleRate);
                float input = (float)(random.NextDouble() * 2 - 1);
                low += f * band;
                float high = input - low - 0.5f * band;
                band += f * high;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(p * 1.3f)) * (1 - p * 0.4f);
                return band * envelope * gain;
            });
        }

        static float[] Arpeggio(float[] notes, float step, float tail, float gain)
        {
            float total = step * (notes.Length - 1) + tail;
            return Render(total, (t, i) =>
            {
                float sum = 0;
                for (int n = 0; n < notes.Length; n++)
                {
                    float local = t - n * step;
                    if (local < 0)
                        continue;
                    float bell = Sine(local, notes[n]) + 0.35f * Sine(local, notes[n] * 2.01f) + 0.12f * Sine(local, notes[n] * 3.98f);
                    sum += bell * Decay(local, 5.5f) * Mathf.Clamp01(local * 400);
                }
                return sum * gain / Mathf.Sqrt(notes.Length);
            });
        }

        static float[] Shimmer(System.Random random, float seconds)
        {
            return Render(seconds, (t, i) =>
            {
                float sparkle = random.NextDouble() < 0.0015 ? 1 : 0;
                return (float)(random.NextDouble() * 2 - 1) * 0.04f * Decay(t, 3) + sparkle * 0.25f * Decay(t, 2);
            });
        }

        /// <summary>シャッフルのリフル音。細かいカードのはじきを密に並べる。</summary>
        static float[] Riffle(System.Random random, float seconds)
        {
            var data = new float[Mathf.CeilToInt(seconds * SampleRate)];
            int flicks = 70;
            for (int k = 0; k < flicks; k++)
            {
                int start = (int)(data.Length * (k / (float)flicks) * 0.85f) + random.Next(400);
                float amp = 0.12f + (float)random.NextDouble() * 0.08f;
                for (int i = 0; i < 700 && start + i < data.Length; i++)
                {
                    float t = i / (float)SampleRate;
                    data[start + i] += (float)(random.NextDouble() * 2 - 1) * amp * Decay(t, 500);
                }
            }
            return data;
        }

        static float[] Mix(params float[][] parts)
        {
            int length = 0;
            foreach (var p in parts)
                length = Math.Max(length, p.Length);
            var data = new float[length];
            foreach (var p in parts)
            {
                for (int i = 0; i < p.Length; i++)
                    data[i] += p[i];
            }
            for (int i = 0; i < data.Length; i++)
                data[i] = Mathf.Clamp(data[i], -1f, 1f);
            return data;
        }
    }
}
