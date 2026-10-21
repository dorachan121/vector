using UnityEngine;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Core.Rpg;
using Vector.Game.Persistence;
using Vector.Game.Story;
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
        static StoryProgress story;

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

        /// <summary>ストーリーモードの進み具合（プレイヤーデータと同じ保存先）。</summary>
        public static StoryProgress Story => story ??= new StoryProgress(Profiles);

        /// <summary>フロアの主の挑戦の最中なら、その挑戦（テーブルはこれを手持ちにする）。</summary>
        public static ChallengeRun Challenge { get; set; }

        /// <summary>テーブルで決着した挑戦の結果（フロアのシーンが受け取って主の台詞を見せる）。</summary>
        public static ChallengeReward ChallengeResult { get; set; }

        /// <summary>ロビーで選んだ席。テーブルシーンはこれを見てモードを決める。</summary>
        public static GameMode SelectedMode { get; set; } = GameMode.Player;

        /// <summary>テーブルを離れたときに戻るシーン（ロビーか、RPG のフロア）。</summary>
        public static string ReturnScene { get; set; } = SceneNames.Lobby;

        /// <summary>この起動でタイトルを見せた（ホームに戻るたびには出さない）。</summary>
        public static bool TitleShown { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            profiles = null;
            story = null;
            SelectedMode = GameMode.Player;
            ReturnScene = SceneNames.Lobby;
            TitleShown = false;
            Challenge = null;
            ChallengeResult = null;
        }

        public static void ApplySettings()
        {
            var settings = Profiles.Profile.settings;
            Sfx.Volume = settings.sfxVolume;
            Tween.Speed = settings.fastDeal ? FastDealSpeed : 1f;
        }
    }
}
