namespace Vector.Core.Orbit
{
    public enum OrbitPhase : byte
    {
        /// <summary>ベット受付中。</summary>
        Betting,

        /// <summary>結果が決まり、精算待ち。</summary>
        Spun,

        /// <summary>精算済み。</summary>
        Complete,
    }
}
