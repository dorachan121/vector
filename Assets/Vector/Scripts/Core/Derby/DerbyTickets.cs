using System;

namespace Vector.Core.Derby
{
    /// <summary>
    /// 馬券（ベットエリア）の番号。単勝（WIN）は走者の番号 0〜3、馬単（EXACTA: 1着と2着を着順どおり）は 4 + 1着×4 + 2着。
    /// 走者の番号はスートの番号と同じ（♠=青龍, ♥=朱雀, ♦=白虎, ♣=玄武）。
    /// </summary>
    public static class DerbyTickets
    {
        public const int Runners = 4;
        public const int Count = Runners + Runners * Runners;

        public static int Win(int runner)
        {
            CheckRunner(runner);
            return runner;
        }

        public static int Exacta(int first, int second)
        {
            CheckRunner(first);
            CheckRunner(second);
            if (first == second)
                throw new ArgumentException("1着と2着は別の走者を指定してください。", nameof(second));
            return Runners + first * Runners + second;
        }

        public static bool IsWin(int spot) => spot >= 0 && spot < Runners;

        public static bool IsExacta(int spot) => spot >= Runners && spot < Count && First(spot) != Second(spot);

        public static bool IsValid(int spot) => IsWin(spot) || IsExacta(spot);

        /// <summary>単勝ならその走者、馬単なら1着の走者。</summary>
        public static int First(int spot) => spot < Runners ? spot : (spot - Runners) / Runners;

        /// <summary>馬単の2着の走者。</summary>
        public static int Second(int spot) => (spot - Runners) % Runners;

        static void CheckRunner(int runner)
        {
            if (runner < 0 || runner >= Runners)
                throw new ArgumentOutOfRangeException(nameof(runner));
        }
    }
}
