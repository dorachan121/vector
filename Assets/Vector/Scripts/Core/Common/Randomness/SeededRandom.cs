using System;

namespace Vector.Core
{
    /// <summary>
    /// 再現可能なシード付き乱数（xorshift128+）。テスト・シミュレーション・AIの先読みに使う。
    /// </summary>
    public sealed class SeededRandom : IRandomSource
    {
        ulong s0;
        ulong s1;

        public SeededRandom(ulong seed)
        {
            s0 = SplitMix(ref seed);
            s1 = SplitMix(ref seed);
            if (s0 == 0 && s1 == 0)
                s1 = 1;
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            // 剰余による偏りを避けるため棄却サンプリングする
            ulong bound = (ulong)maxExclusive;
            ulong limit = ulong.MaxValue - ulong.MaxValue % bound;
            ulong r;
            do
            {
                r = NextULong();
            } while (r >= limit);
            return (int)(r % bound);
        }

        ulong NextULong()
        {
            ulong x = s0;
            ulong y = s1;
            s0 = y;
            x ^= x << 23;
            s1 = x ^ y ^ (x >> 17) ^ (y >> 26);
            return s1 + y;
        }

        static ulong SplitMix(ref ulong x)
        {
            ulong z = x += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}
