using System;
using System.Security.Cryptography;

namespace Vector.Core
{
    /// <summary>
    /// 暗号論的に安全な乱数。出目が予測できず偏りもないため、実際の配札に使う。
    /// </summary>
    public sealed class CryptoRandom : IRandomSource
    {
        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            return RandomNumberGenerator.GetInt32(maxExclusive);
        }
    }
}
