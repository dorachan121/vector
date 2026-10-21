namespace Vector.Core
{
    /// <summary>精算済みの1ラウンドの金額。ゲームの種類によらず成績やクエストの集計に使う。</summary>
    public interface IRoundOutcome
    {
        /// <summary>このラウンドに賭けた総額。</summary>
        long TotalStake { get; }

        /// <summary>払い戻しの総額（賭け金を含む）。</summary>
        long TotalReturn { get; }
    }
}
