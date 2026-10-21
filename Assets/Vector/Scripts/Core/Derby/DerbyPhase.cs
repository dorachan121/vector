namespace Vector.Core.Derby
{
    public enum DerbyPhase : byte
    {
        /// <summary>出走表とオッズを発表して馬券を受け付けている。</summary>
        Betting,

        /// <summary>レース中（カードを1枚ずつ引いて進める）。</summary>
        Running,

        /// <summary>1着と2着が決まり、精算待ち。</summary>
        Finished,

        /// <summary>精算済み。</summary>
        Complete,
    }
}
