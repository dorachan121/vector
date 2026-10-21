using UnityEngine;
using Vector.Game.Audio;

namespace Vector.Game.Casino
{
    /// <summary>持ち時間。一時停止中（ダイアログ表示中など）は減らず、残り5秒からは秒ごとに音を鳴らす。</summary>
    public sealed class CountdownTimer
    {
        const float WarningSeconds = 5f;

        /// <summary>1フレームで減らす上限。アプリが裏に回っていた間に持ち時間が尽きないようにする。</summary>
        const float MaxStep = 0.25f;

        readonly float total;
        float remaining;

        public CountdownTimer(float seconds)
        {
            total = seconds;
            remaining = seconds;
        }

        public bool Expired => remaining <= 0;

        public void Reset() => remaining = total;

        public void Tick(StatusBanner status, bool paused)
        {
            if (!paused)
                remaining -= Mathf.Min(Time.unscaledDeltaTime, MaxStep);
            if (status.SetTimer(remaining, total) && remaining <= WarningSeconds && remaining > 0)
                Sfx.Play(SfxKind.Tick);
        }
    }
}
