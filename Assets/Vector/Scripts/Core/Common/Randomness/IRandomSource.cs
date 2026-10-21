namespace Vector.Core
{
    /// <summary>
    /// 乱数源の抽象。本番は暗号論的乱数、テストとシミュレーションはシード付き乱数を差し込む。
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>[0, maxExclusive) の一様な整数を返す。</summary>
        int Next(int maxExclusive);
    }
}
