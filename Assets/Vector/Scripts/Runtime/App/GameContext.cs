using UnityEngine;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.Persistence;
using Vector.Game.UI;

namespace Vector.Game
{
    /// <summary>
    /// シーンをまたいで共有する状態（プレイヤーデータと、ロビーで選んだ席）。
    /// 初回アクセス時にセーブデータを読み込み、設定を各サービスへ反映する。
    /// </summary>
    public static class GameContext
    {
        /// <summary>高速ディール時の演出速度倍率。</summary>
        const float FastDealSpeed = 1.8f;

        static ProfileService profiles;

        public static ProfileService Profiles
        {
            get
            {
                if (profiles == null)
                {
                    profiles = new ProfileService(new FileProfileStorage(Application.persistentDataPath));
                    profiles.Load();
                    ApplySettings();
                }
                return profiles;
            }
        }

        public static TableRules Rules => TableRules.Standard;

        /// <summary>ロビーで選んだ席。テーブルシーンはこれを見てモードを決める。</summary>
        public static GameMode SelectedMode { get; set; } = GameMode.Player;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            profiles = null;
            SelectedMode = GameMode.Player;
        }

        public static void ApplySettings()
        {
            var settings = Profiles.Profile.settings;
            Sfx.Volume = settings.sfxVolume;
            Tween.Speed = settings.fastDeal ? FastDealSpeed : 1f;
        }
    }
}
