using System;

namespace Vector.Core
{
    /// <summary>ベット検証の結果と、問題のあったベットエリア（番号）・上下限。</summary>
    public readonly struct WagerCheck
    {
        public readonly BetError Error;
        public readonly int Spot;
        public readonly long Limit;

        public WagerCheck(BetError error, int spot = -1, long limit = 0)
        {
            Error = error;
            Spot = spot;
            Limit = limit;
        }

        public bool IsValid => Error == BetError.None;

        public static WagerCheck Ok => new WagerCheck(BetError.None);

        /// <summary>配る前のベット検証。</summary>
        public static WagerCheck Validate(IBetLimits limits, BetBoard bets)
        {
            bool hasPrimary = false;
            foreach (var bet in bets.NonZero())
            {
                if (!limits.IsSpotAvailable(bet.Key))
                    return new WagerCheck(BetError.SpotUnavailable, bet.Key);
                if (bet.Value < limits.MinFor(bet.Key))
                    return new WagerCheck(BetError.BelowMinimum, bet.Key, limits.MinFor(bet.Key));
                if (bet.Value > limits.MaxFor(bet.Key))
                    return new WagerCheck(BetError.AboveMaximum, bet.Key, limits.MaxFor(bet.Key));
                hasPrimary |= limits.IsPrimary(bet.Key);
            }
            return hasPrimary ? Ok : new WagerCheck(BetError.NoPrimaryBet);
        }

        /// <summary>エンジンが勝負を始める前の検証。不正なら例外（画面側は事前に <see cref="Validate"/> で確かめてから渡す）。</summary>
        public static void ThrowIfInvalid(IBetLimits limits, BetBoard bets)
        {
            if (bets == null)
                throw new ArgumentNullException(nameof(bets));
            var check = Validate(limits, bets);
            if (!check.IsValid)
                throw new InvalidOperationException($"ベットが不正です: {check.Error} ({check.Spot})");
        }
    }
}
