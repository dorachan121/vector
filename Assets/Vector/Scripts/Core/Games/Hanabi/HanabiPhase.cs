namespace Vector.Core.Hanabi
{
    public enum HanabiPhase : byte
    {
        Betting,

        /// <summary>花火の玉が昇っている（開くか、弾けるのを待つ）。</summary>
        Rising,

        /// <summary>開いた、または弾けた（精算待ち）。</summary>
        Finished,

        Complete,
    }
}
