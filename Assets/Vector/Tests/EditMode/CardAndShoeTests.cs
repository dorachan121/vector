using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    public class CardAndShoeTests
    {
        [Test]
        public void Card_IdRoundTripsForAll52Cards()
        {
            var seen = new HashSet<int>();
            for (int id = 0; id < Card.DistinctCount; id++)
            {
                var card = Card.FromId(id);
                Assert.AreEqual(id, card.Id);
                Assert.IsTrue(seen.Add(card.GetHashCode()));
            }
        }

        [TestCase(Rank.Ace, 1, "A")]
        [TestCase(Rank.Ten, 10, "10")]
        [TestCase(Rank.Jack, 11, "J")]
        [TestCase(Rank.King, 13, "K")]
        public void Card_ValueAndLabelFollowRank(Rank rank, int value, string label)
        {
            var card = new Card(rank, Suit.Hearts);
            Assert.AreEqual(value, card.Value);
            Assert.AreEqual(label, card.RankLabel);
            Assert.IsTrue(card.IsRed);
        }

        [Test]
        public void Card_RejectsInvalidRank()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Card((Rank)0, Suit.Spades));
            Assert.Throws<ArgumentOutOfRangeException>(() => Card.FromId(52));
        }

        [Test]
        public void Shoe_ContainsEveryCardOncePerDeck()
        {
            const int decks = 6;
            var shoe = new Shoe(decks, new SeededRandom(1));
            var counts = new int[Card.DistinctCount];
            counts[shoe.BurnedCard.Id]++;
            while (shoe.Remaining > 0)
                counts[shoe.Draw().Id]++;
            foreach (int c in counts)
                Assert.AreEqual(decks, c);
        }

        [Test]
        public void Shoe_BurnsOneCardAndPlacesCutCardBetween70And80Percent()
        {
            var shoe = new Shoe(6, new SeededRandom(2));
            Assert.AreEqual(shoe.TotalCards - 1, shoe.Remaining);
            int drawn = 1;
            while (!shoe.CutCardReached)
            {
                shoe.Draw();
                drawn++;
            }
            Assert.That(drawn, Is.InRange((int)(shoe.TotalCards * 0.70f), (int)(shoe.TotalCards * 0.80f)));
        }

        [Test]
        public void Shoe_ShuffleStartsNewShoe()
        {
            var shoe = new Shoe(1, new SeededRandom(3));
            Assert.AreEqual(1, shoe.ShoeNumber);
            shoe.Draw();
            shoe.Shuffle();
            Assert.AreEqual(2, shoe.ShoeNumber);
            Assert.AreEqual(shoe.TotalCards - 1, shoe.Remaining);
        }

        [Test]
        public void Shoe_ThrowsWhenEmpty()
        {
            var shoe = new Shoe(1, new SeededRandom(4));
            while (shoe.Remaining > 0)
                shoe.Draw();
            Assert.Throws<InvalidOperationException>(() => shoe.Draw());
        }

        [Test]
        public void SeededRandom_IsDeterministicAndInRange()
        {
            var a = new SeededRandom(42);
            var b = new SeededRandom(42);
            for (int i = 0; i < 1000; i++)
            {
                int x = a.Next(13);
                Assert.AreEqual(x, b.Next(13));
                Assert.That(x, Is.InRange(0, 12));
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Next(0));
        }

        [Test]
        public void SeededRandom_IsRoughlyUniform()
        {
            var random = new SeededRandom(7);
            var counts = new int[13];
            const int samples = 130_000;
            for (int i = 0; i < samples; i++)
                counts[random.Next(13)]++;
            foreach (int c in counts)
                Assert.That(c, Is.InRange(9_500, 10_500));
        }

        [Test]
        public void CryptoRandom_StaysInRange()
        {
            var random = new CryptoRandom();
            for (int i = 0; i < 200; i++)
                Assert.That(random.Next(52), Is.InRange(0, 51));
        }
    }
}
