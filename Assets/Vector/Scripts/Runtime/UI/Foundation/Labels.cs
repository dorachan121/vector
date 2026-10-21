using System;
using Vector.Core;

namespace Vector.Game.UI
{
    /// <summary>画面に出す文言の集約。ルール側（Vector.Core）は文言を持たず、ここで表示用に変換する。</summary>
    public static class Labels
    {
        public static string Money(long amount) => "$" + amount.ToString("N0");

        public static string SignedMoney(long amount) => (amount >= 0 ? "+$" : "-$") + Math.Abs(amount).ToString("N0");

        /// <summary>チップの表記（1,000 → 1K）。</summary>
        public static string ChipValue(long amount)
        {
            if (amount >= 1000 && amount % 1000 == 0)
                return amount / 1000 + "K";
            return amount.ToString();
        }

        /// <summary>カードの表記（例: K♠）。</summary>
        public static string CardName(Card card) => card.RankLabel + "♠♥♦♣"[(int)card.Suit];

        public static string Family(LineFamily family)
        {
            switch (family)
            {
                case LineFamily.Row: return "ROW";
                case LineFamily.Column: return "COLUMN";
                default: return "SLASH";
            }
        }

        public static string Spot(BetSpot spot)
        {
            switch (spot)
            {
                case BetSpot.Main: return "MAIN";
                case BetSpot.Bonus: return "BONUS";
                case BetSpot.Player: return "PLAYER";
                case BetSpot.Dealer: return "DEALER";
                default: return "TIE";
            }
        }

        public static string Winner(RoundWinner winner)
        {
            switch (winner)
            {
                case RoundWinner.Player: return "PLAYER WINS";
                case RoundWinner.Dealer: return "DEALER WINS";
                default: return "TIE";
            }
        }

        public static string HandName(PokerHand hand)
        {
            switch (hand)
            {
                case PokerHand.RoyalFlush: return "ROYAL FLUSH";
                case PokerHand.StraightFlush: return "STRAIGHT FLUSH";
                case PokerHand.FiveOfAKind: return "FIVE OF A KIND";
                case PokerHand.FourOfAKind: return "FOUR OF A KIND";
                case PokerHand.FullHouse: return "FULL HOUSE";
                case PokerHand.Flush: return "FLUSH";
                case PokerHand.Straight: return "STRAIGHT";
                case PokerHand.ThreeOfAKind: return "THREE OF A KIND";
                case PokerHand.TwoPair: return "TWO PAIR";
                case PokerHand.OnePair: return "ONE PAIR";
                default: return "NO HAND";
            }
        }

        public static string Mode(GameMode mode) => mode == GameMode.Player ? "PLAYER SEAT" : "AUDIENCE";

        /// <summary>ベット検証の問題（ベットエリアの名前はゲームごとに渡す）。</summary>
        public static string WagerProblem(WagerCheck check, Func<int, string> spotName)
        {
            switch (check.Error)
            {
                case BetError.None: return null;
                case BetError.NoPrimaryBet: return "メインのベットを置いてください";
                case BetError.BelowMinimum: return $"{spotName(check.Spot)} のミニマムは {Money(check.Limit)} です";
                case BetError.AboveMaximum: return $"{spotName(check.Spot)} のマキシマムは {Money(check.Limit)} です";
                default: return $"{spotName(check.Spot)} には賭けられません";
            }
        }

        /// <summary>チップを置けなかった理由。</summary>
        public static string ChipProblem(ChipError error, string spotName, long maximum)
        {
            switch (error)
            {
                case ChipError.None: return null;
                case ChipError.InsufficientFunds: return "手持ちのチップが足りません";
                case ChipError.AboveMaximum: return $"{spotName} のマキシマムは {Money(maximum)} です";
                case ChipError.NotBetting: return "ベット受付は終了しました";
                default: return $"{spotName} には賭けられません";
            }
        }
    }
}
