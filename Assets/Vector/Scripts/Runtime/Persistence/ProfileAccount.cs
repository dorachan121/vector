using System;
using Vector.Core;

namespace Vector.Game.Persistence
{
    /// <summary>プレイヤーデータの所持金を口座として使う（ストーリーモードの参加費・賞金・引換券）。変更のたびに保存する。</summary>
    public sealed class ProfileAccount : IChipAccount
    {
        readonly ProfileService profiles;

        public ProfileAccount(ProfileService profiles)
        {
            this.profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        }

        public long Balance => profiles.Profile.balance;

        public bool TryDebit(long amount)
        {
            if (amount < 0 || amount > Balance)
                return false;
            profiles.Profile.balance -= amount;
            profiles.Save();
            return true;
        }

        public void Credit(long amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            profiles.Profile.balance = PlayerProfile.Clamp(profiles.Profile.balance + amount);
            profiles.Save();
        }
    }
}
