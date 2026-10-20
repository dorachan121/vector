using System;
using UnityEngine;

namespace Vector.Game.Persistence
{
    /// <summary>プレイヤーの設定。</summary>
    [Serializable]
    public sealed class ProfileSettings
    {
        /// <summary>効果音の音量（0〜1）。</summary>
        public float sfxVolume = 0.8f;

        /// <summary>配札などの演出を速くする。</summary>
        public bool fastDeal;

        /// <summary>戦略ヒント（各ラインの勝率と推奨手）を表示する。</summary>
        public bool showStrategy = true;

        public void Sanitize()
        {
            sfxVolume = float.IsNaN(sfxVolume) ? 0.8f : Mathf.Clamp01(sfxVolume);
        }
    }
}
