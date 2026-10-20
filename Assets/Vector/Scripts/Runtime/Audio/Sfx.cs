using System.Collections.Generic;
using UnityEngine;

namespace Vector.Game.Audio
{
    /// <summary>
    /// 効果音の再生窓口。音源は初回再生時に合成してキャッシュし、AudioSource のプールで重ねて鳴らす。
    /// </summary>
    public static class Sfx
    {
        const int Voices = 10;

        static readonly Dictionary<SfxKind, AudioClip> clips = new Dictionary<SfxKind, AudioClip>();
        static AudioSource[] sources;
        static int next;

        /// <summary>全体の音量（0〜1）。設定画面から変更する。</summary>
        public static float Volume { get; set; } = 0.8f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            clips.Clear();
            sources = null;
            next = 0;
        }

        public static void Play(SfxKind kind, float volume = 1f, float pitch = 1f)
        {
            if (Volume <= 0f || volume <= 0f)
                return;
            var source = NextSource();
            source.pitch = pitch;
            source.PlayOneShot(Clip(kind), Mathf.Clamp01(volume * Volume));
        }

        static AudioClip Clip(SfxKind kind)
        {
            if (!clips.TryGetValue(kind, out var clip) || clip == null)
            {
                clip = SfxSynth.Create(kind);
                clips[kind] = clip;
            }
            return clip;
        }

        static AudioSource NextSource()
        {
            if (sources == null || sources[0] == null)
            {
                var host = new GameObject("[Sfx]") { hideFlags = HideFlags.HideInHierarchy };
                Object.DontDestroyOnLoad(host);
                sources = new AudioSource[Voices];
                for (int i = 0; i < Voices; i++)
                {
                    sources[i] = host.AddComponent<AudioSource>();
                    sources[i].playOnAwake = false;
                    sources[i].spatialBlend = 0f;
                }
            }
            next = (next + 1) % sources.Length;
            return sources[next];
        }
    }
}
