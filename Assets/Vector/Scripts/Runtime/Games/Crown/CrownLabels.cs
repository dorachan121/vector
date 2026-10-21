using Vector.Core.Crown;

namespace Vector.Game.Crown
{
    /// <summary>CROWN DICE の画面の文言。</summary>
    public static class CrownLabels
    {
        public static string Hand(DiceHandKind kind)
        {
            switch (kind)
            {
                case DiceHandKind.Yacht: return "YACHT";
                case DiceHandKind.FourOfAKind: return "FOUR OF A KIND";
                case DiceHandKind.FullHouse: return "FULL HOUSE";
                case DiceHandKind.Straight: return "STRAIGHT";
                case DiceHandKind.ThreeOfAKind: return "THREE OF A KIND";
                case DiceHandKind.TwoPair: return "TWO PAIR";
                case DiceHandKind.OnePair: return "ONE PAIR";
                default: return "NO HAND";
            }
        }

        /// <summary>役名に、組になっている目を添える（例: FULL HOUSE ⚅ / 王冠の目なら CROWN を付ける）。</summary>
        public static string Rank(DiceRank rank, int crown)
        {
            if (rank.IsCrownYachtFor(crown))
                return "CROWN YACHT";
            string name = Hand(rank.Kind);
            if (rank.MainFace == 0)
                return name;
            return name + "  " + (rank.MainFace == crown ? "★" + rank.MainFace : rank.MainFace.ToString());
        }

        public static string Opening(OpeningHand hand)
        {
            switch (hand)
            {
                case OpeningHand.CrownYacht: return "CROWN YACHT";
                case OpeningHand.Yacht: return "YACHT";
                case OpeningHand.CrownFour: return "CROWN FOUR";
                case OpeningHand.FourOfAKind: return "FOUR OF A KIND";
                case OpeningHand.FullHouse: return "FULL HOUSE";
                case OpeningHand.Straight: return "STRAIGHT";
                case OpeningHand.CrownThree: return "CROWN THREE";
                default: return "NO HAND";
            }
        }

        public static string Spot(int spot) => spot == CrownRules.Ante ? "ANTE" : "OPENING";
    }
}
