using UnityEngine;

namespace Vector.Game
{
    /// <summary>起動時に一度だけ行う端末まわりの設定。</summary>
    static class Startup
    {
        /// <summary>カードやチップの動きが滑らかに見えるフレームレート（モバイルの既定は 30）。</summary>
        const int TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Configure()
        {
            Application.targetFrameRate = TargetFrameRate;
            // 2本指の同時タップで DEAL と LOBBY を同時に押す、といった操作を防ぐ
            Input.multiTouchEnabled = false;
        }
    }
}
