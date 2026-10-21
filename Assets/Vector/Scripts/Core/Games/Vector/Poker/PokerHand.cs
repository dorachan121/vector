namespace Vector.Core
{
    /// <summary>
    /// ボーナスベットの役（弱い順）。中央のベクターカードと手札4枚の計5枚で判定する。
    /// 6デッキのシューではファイブカードも成立し、ストレートフラッシュより出やすい。
    /// </summary>
    public enum PokerHand : byte
    {
        Nothing,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        FiveOfAKind,
        StraightFlush,
        RoyalFlush,
    }
}
