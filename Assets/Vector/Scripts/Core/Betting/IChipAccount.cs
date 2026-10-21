namespace Vector.Core
{
    /// <summary>チップの出し入れができる口座（所持金）。</summary>
    public interface IChipAccount
    {
        long Balance { get; }

        bool TryDebit(long amount);

        void Credit(long amount);
    }
}
