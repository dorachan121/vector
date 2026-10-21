namespace Vector.Core
{
    /// <summary>ベットエリアごとの上下限。ベットエリアはゲームごとに番号で表す。</summary>
    public interface IBetLimits
    {
        bool IsSpotAvailable(int spot);

        long MinFor(int spot);

        long MaxFor(int spot);

        /// <summary>サイドベット以外（これが1つも無いと配れない）。</summary>
        bool IsPrimary(int spot);
    }
}
