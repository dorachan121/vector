namespace Vector.Core.Crown
{
    /// <summary>最初に振った5個の目からオープニングベットの役を決める。</summary>
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
