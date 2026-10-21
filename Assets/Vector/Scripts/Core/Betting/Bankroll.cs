using System;

namespace Vector.Core
{
    /// <summary>所持金。マイナスにはならない。</summary>
    public sealed class Bankroll : IChipAccount
    {
        public long Balance { get; private set; }

        public Bankroll(long balance)
        {
            if (balance < 0)
                throw new ArgumentOutOfRangeException(nameof(balance));
            Balance = balance;
        }

        public bool CanAfford(long amount) => amount >= 0 && amount <= Balance;

        public bool TryDebit(long amount)
        {
            if (!CanAfford(amount))
                return false;
            Balance -= amount;
            return true;
        }

        public void Credit(long amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            Balance = checked(Balance + amount);
        }
    }
}
