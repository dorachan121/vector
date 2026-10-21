namespace Vector.Core.Crown
{
    /// <summary>
    /// オープニングベット（サイドベット）の役（弱い順）。最初に5個を振った時点の目だけで決まるので、
    /// 振り直しの打ち方には左右されない。
    /// </summary>
    public enum OpeningHand : byte
    {
        None,

        /// <summary>王冠の目のスリーカード（フルハウスを除く）。</summary>
        CrownThree,

        Straight,
        FullHouse,
        FourOfAKind,

        /// <summary>王冠の目のフォーカード。</summary>
        CrownFour,

        Yacht,

        /// <summary>王冠の目のヨット。</summary>
        CrownYacht,
    }
}
