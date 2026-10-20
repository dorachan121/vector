using UnityEngine;
using Vector.Game.Audio;

namespace Vector.Game.Table
{
    /// <summary>持ち時間。一時停止中（ダイアログ表示中など）は減らず、残り5秒からは秒ごとに音を鳴らす。</summary>
    public sealed class CountdownTimer
    {
        const float WarningSeconds = 5f;

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
                remaining -= Time.unscaledDeltaTime;
            if (status.SetTimer(remaining, total) && remaining <= WarningSeconds && remaining > 0)
                Sfx.Play(SfxKind.Tick);
        }
    }
}
