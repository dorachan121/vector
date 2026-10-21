using System;

namespace Vector.Core.Hanabi
{
    /// <summary>
    /// HANABI のルール（不変）。打ち上げた花火の玉は倍率を上げながら昇り、決まった高さ（暴発点）で弾ける。
    /// 弾ける前に開けば（CASH OUT）、その倍率で払い戻す。
    /// <para>暴発点は打ち上げの瞬間に整数の乱数 r（0 〜 <see cref="Resolution"/>−1）から
    /// floor(<see cref="ReturnPercent"/> × <see cref="Resolution"/> ÷ (r + 1)) [百分の一倍] で決まる。
    /// そのため倍率 m 以上まで昇る確率はほぼ 0.97 ÷ m で、<b>どの倍率で開いても RTP は 97% を超えない</b>（厳密値は <see cref="ReturnAt"/>）。</para>
    /// </summary>
    public sealed class HanabiRules : IBetLimits
    {
        public const int Main = 0;

        public static readonly HanabiRules Standard = new HanabiRules();

        /// <summary>払い戻し率の上限（%）。暴発点がこの割合で決まる。</summary>
        public const int ReturnPercent = 97;

        /// <summary>暴発点を決める乱数の幅。</summary>
        public const int Resolution = 1_000_000;

        /// <summary>倍率は百分の一倍で数える（100 で 1.00 倍）。</summary>
        public const int One = 100;

        /// <summary>暴発点の上限（1,000 倍）。</summary>
        public int MaxHundredths { get; } = 100_000;

        /// <summary>開ける最低の倍率（1.01 倍）。</summary>
        public int MinCashOut { get; } = 101;

        public long MinBet { get; } = 10;
        public long MaxBet { get; } = 1_000;
        public float BettingSeconds { get; } = 15f;

        /// <summary>乱数 r（0 〜 Resolution−1）から暴発点（百分の一倍、1.00 倍以上・上限まで）。</summary>
        public int CrashFor(int r)
        {
            if (r < 0 || r >= Resolution)
                throw new ArgumentOutOfRangeException(nameof(r));
            long crash = (long)ReturnPercent * Resolution / (r + 1);
            return (int)Math.Max(One, Math.Min(MaxHundredths, crash));
        }

        /// <summary>倍率 hundredths 以上まで昇る（その倍率で開ける）乱数の数。</summary>
        public long SurvivingDraws(int hundredths)
        {
            if (hundredths <= One)
                return Resolution;
            if (hundredths > MaxHundredths)
                return 0;
            return Math.Min(Resolution, (long)ReturnPercent * Resolution / hundredths);
        }

        /// <summary>倍率 hundredths で開けば払い戻しを受けられる確率。</summary>
        public double ProbabilityAtLeast(int hundredths) => SurvivingDraws(hundredths) / (double)Resolution;

        /// <summary>いつも倍率 hundredths で開く打ち方の RTP（厳密）。</summary>
        public double ReturnAt(int hundredths) => ProbabilityAtLeast(hundredths) * hundredths / One;

        /// <summary>打ち上げてすぐ弾ける（1.01 倍でも開けない）確率。</summary>
        public double InstantBurst => 1 - ProbabilityAtLeast(MinCashOut);

        public bool IsSpotAvailable(int spot) => spot == Main;
        public long MinFor(int spot) => MinBet;
        public long MaxFor(int spot) => MaxBet;
        public bool IsPrimary(int spot) => spot == Main;
    }
}
