namespace Vector.Core.Crown
{
    /// <summary>5個のダイスの役（弱い順）。</summary>
    public enum DiceHandKind : byte
    {
        NoHand,
        OnePair,
        TwoPair,
        ThreeOfAKind,

        /// <summary>1-2-3-4-5 か 2-3-4-5-6。</summary>
        Straight,

        FullHouse,
        FourOfAKind,

        /// <summary>5個とも同じ目（ファイブカード）。</summary>
        Yacht,
    }
}
