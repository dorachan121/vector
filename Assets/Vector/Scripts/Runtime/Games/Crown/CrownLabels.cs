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

        /// <summary>役名に、組になっている目を添える（例: TWO PAIR 6·5 / 王冠の目には ★ を付ける）。</summary>
        public static string Rank(DiceRank rank, int crown)
        {
            if (rank.IsCrownYachtFor(crown))
                return "CROWN YACHT";
            string name = Hand(rank.Kind);
            if (rank.MainFace == 0)
                return name;
            string faces = Face(rank.MainFace, crown);
            if (rank.SecondFace != 0)
                faces += "·" + Face(rank.SecondFace, crown);
            return name + "  " + faces;
        }

        /// <summary>
        /// 勝負の結果の行（あなた — ディーラー）。表示上は同じ役でも残りの目で勝敗が決まったときは、そのことを書き添える
        /// （同じに見えるのに負けた、と感じさせないため）。
        /// </summary>
        public static string Versus(DiceRank player, DiceRank dealer, int crown)
        {
            string p = Rank(player, crown), d = Rank(dealer, crown);
            string line = $"{p}  —  {d}";
            return p == d && player != dealer ? line + "（残りの目で決着）" : line;
        }

        static string Face(int face, int crown) => face == crown ? "★" + face : face.ToString();

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
