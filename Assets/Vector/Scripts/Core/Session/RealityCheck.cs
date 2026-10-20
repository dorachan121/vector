using System;

namespace Vector.Core
{
    /// <summary>
    /// リアリティチェック（責任あるゲーミングのための定期的なお知らせ）。
    /// 一定時間ごとに、プレイ時間とこのセッションの収支を確認してもらう。時刻は呼び出し側が渡す（テストしやすくするため）。
    /// </summary>
    public sealed class RealityCheck
    {
        readonly TimeSpan interval;
        readonly long startBalance;
        readonly DateTime startedAt;
        long deposits;
        DateTime nextAt;

        public RealityCheck(TimeSpan interval, long startBalance, DateTime now)
        {
            if (interval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(interval));
            this.interval = interval;
            this.startBalance = startBalance;
            startedAt = now;
            nextAt = now + interval;
        }

        public bool IsDue(DateTime now) => now >= nextAt;

        public TimeSpan Elapsed(DateTime now) => now - startedAt;

        /// <summary>キャッシャーで補充したチップ。収支には含めない。</summary>
        public void RecordDeposit(long amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            deposits += amount;
        }

        /// <summary>このセッションの勝ち負けの収支。</summary>
        public long Net(long balance) => balance - startBalance - deposits;

        /// <summary>お知らせを確認した。次は今から interval 後。</summary>
        public void Acknowledge(DateTime now) => nextAt = now + interval;
    }
}
