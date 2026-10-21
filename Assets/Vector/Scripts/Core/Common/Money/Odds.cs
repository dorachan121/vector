using System;

namespace Vector.Core
{
    /// <summary>配当倍率「勝ち額 : 賭け額」。5:4 なら 4 賭けて 5 の勝ち（賭け金は別に戻る）。</summary>
    public readonly struct Odds
    {
        public readonly int Win;
        public readonly int Per;

        public Odds(int win, int per = 1)
        {
            if (win <= 0)
                throw new ArgumentOutOfRangeException(nameof(win));
            if (per <= 0)
                throw new ArgumentOutOfRangeException(nameof(per));
            Win = win;
            Per = per;
        }

        public double Ratio => (double)Win / Per;

        /// <summary>勝ち額（賭け金を含まない）。端数はカジノの慣例どおり切り捨て。</summary>
        public long WinningsFor(long stake)
        {
            if (stake < 0)
                throw new ArgumentOutOfRangeException(nameof(stake));
            return checked(stake * Win) / Per;
        }

        public override string ToString() => $"{Win}:{Per}";
    }
}
