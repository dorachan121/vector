namespace Vector.Core.Sugoroku
{
    public enum SugorokuPhase : byte
    {
        /// <summary>ベット受付中。</summary>
        Betting,

        /// <summary>2つの目が出ていて、どちらで進むか選ぶところ。</summary>
        Choosing,

        /// <summary>上がった、または回数を使い切った（精算待ち）。</summary>
        Finished,

        /// <summary>精算済み。</summary>
        Complete,
    }
}
