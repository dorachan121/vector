using System;
using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    public class PokerEvaluatorTests
    {
        static Card C(string text)
        {
            // 例: "KS" = スペードのK, "10H" = ハートの10
            char suitChar = text[text.Length - 1];
            string rankText = text.Substring(0, text.Length - 1);
            Rank rank;
            switch (rankText)
            {
                case "A": rank = Rank.Ace; break;
                case "J": rank = Rank.Jack; break;
                case "Q": rank = Rank.Queen; break;
                case "K": rank = Rank.King; break;
                default: rank = (Rank)int.Parse(rankText); break;
            }
            var suit = (Suit)"SHDC".IndexOf(suitChar);
            return new Card(rank, suit);
        }

        static PokerHand Eval(params string[] cards) => PokerEvaluator.Evaluate(Array.ConvertAll(cards, C));

        [Test]
        public void RecognisesEveryCategory()
        {
            Assert.AreEqual(PokerHand.RoyalFlush, Eval("10S", "JS", "QS", "KS", "AS"));
            Assert.AreEqual(PokerHand.StraightFlush, Eval("5H", "6H", "7H", "8H", "9H"));
            Assert.AreEqual(PokerHand.FiveOfAKind, Eval("7S", "7H", "7D", "7C", "7S"));
            Assert.AreEqual(PokerHand.FourOfAKind, Eval("7S", "7H", "7D", "7C", "2S"));
            Assert.AreEqual(PokerHand.FullHouse, Eval("7S", "7H", "7D", "2C", "2S"));
            Assert.AreEqual(PokerHand.Flush, Eval("2D", "7D", "9D", "JD", "KD"));
            Assert.AreEqual(PokerHand.Straight, Eval("9S", "10H", "JD", "QC", "KS"));
            Assert.AreEqual(PokerHand.ThreeOfAKind, Eval("7S", "7H", "7D", "2C", "3S"));
            Assert.AreEqual(PokerHand.TwoPair, Eval("7S", "7H", "2D", "2C", "3S"));
            Assert.AreEqual(PokerHand.OnePair, Eval("7S", "7H", "2D", "4C", "3S"));
            Assert.AreEqual(PokerHand.Nothing, Eval("7S", "9H", "2D", "4C", "3S"));
        }

        [Test]
        public void AceCountsLowAndHighInStraights()
        {
            Assert.AreEqual(PokerHand.Straight, Eval("AS", "2H", "3D", "4C", "5S"));
            Assert.AreEqual(PokerHand.Straight, Eval("10S", "JH", "QD", "KC", "AS"));
            Assert.AreEqual(PokerHand.Nothing, Eval("JS", "QH", "KD", "AC", "2S"));
        }

        [Test]
        public void IdenticalCardsFromAMultiDeckShoeFormAFlushPair()
        {
            Assert.AreEqual(PokerHand.Flush, Eval("2S", "2S", "5S", "9S", "KS"));
        }

        [Test]
        public void RejectsWrongCardCount()
        {
            Assert.Throws<ArgumentException>(() => PokerEvaluator.Evaluate(new[] { C("2S") }));
        }
    }
}
