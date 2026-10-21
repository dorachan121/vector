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

    public static class OpeningHands
    {
        public static OpeningHand Of(DiceRank rank, int crown)
        {
            bool crowned = rank.MainFace == crown;
            switch (rank.Kind)
            {
                case DiceHandKind.Yacht: return crowned ? OpeningHand.CrownYacht : OpeningHand.Yacht;
                case DiceHandKind.FourOfAKind: return crowned ? OpeningHand.CrownFour : OpeningHand.FourOfAKind;
                case DiceHandKind.FullHouse: return OpeningHand.FullHouse;
                case DiceHandKind.Straight: return OpeningHand.Straight;
                case DiceHandKind.ThreeOfAKind: return crowned ? OpeningHand.CrownThree : OpeningHand.None;
                default: return OpeningHand.None;
            }
        }
    }
}
