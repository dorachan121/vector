namespace Vector.Core.Delve
{
    public enum DelvePhase : byte
    {
        /// <summary>ベット受付中。</summary>
        Betting,

        /// <summary>扉を選ぶか、引き返すかを決める番。</summary>
        Choosing,

        /// <summary>魔物に遭った、引き返した、最深部まで抜けた。精算待ち。</summary>
        Finished,

        /// <summary>精算済み。</summary>
        Complete,
    }
}
