using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    public class ScoringTests
    {
        //  1  2  3
        //  4  5  6
        //  7  8  9
        static readonly int[] Grid = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        [Test]
        public void FamilyScore_IsTheBestLineOfThatFamily()
        {
            Assert.AreEqual(24, Scoring.FamilyScore(Grid, LineFamily.Row));
            Assert.AreEqual(18, Scoring.FamilyScore(Grid, LineFamily.Column));
            Assert.AreEqual(15, Scoring.FamilyScore(Grid, LineFamily.Slash));
        }

        [Test]
        public void Dealer_TakesTheBetterOfTheTwoOtherFamilies()
        {
            Assert.AreEqual(LineFamily.Column, Scoring.DealerFamily(Grid, LineFamily.Row));
            Assert.AreEqual(18, Scoring.DealerScore(Grid, LineFamily.Row));
            Assert.AreEqual(LineFamily.Row, Scoring.DealerFamily(Grid, LineFamily.Slash));
            Assert.AreEqual(24, Scoring.DealerScore(Grid, LineFamily.Column));
        }

        [Test]
        public void Compare_ReportsWinnerFromPlayersPointOfView()
        {
            Assert.AreEqual(1, Scoring.Compare(Grid, LineFamily.Row));
            Assert.AreEqual(-1, Scoring.Compare(Grid, LineFamily.Slash));
            Assert.AreEqual(RoundWinner.Tie, Scoring.WinnerOf(20, 20));
            Assert.AreEqual(RoundWinner.Player, Scoring.WinnerOf(21, 20));
            Assert.AreEqual(RoundWinner.Dealer, Scoring.WinnerOf(19, 20));
        }

        [Test]
        public void BestLines_ReturnsEveryTiedLine()
        {
            int[] values = { 5, 5, 5, 5, 5, 5, 1, 1, 1 };
            var lines = Scoring.BestLines(values, LineFamily.Row);
            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual(0, lines[0].Index);
            Assert.AreEqual(1, lines[1].Index);
        }

        [Test]
        public void Lines_OtherReturnsTheTwoRemainingFamilies()
        {
            Assert.AreEqual(LineFamily.Column, Lines.Other(LineFamily.Row, 0));
            Assert.AreEqual(LineFamily.Slash, Lines.Other(LineFamily.Row, 1));
            Assert.AreEqual(LineFamily.Row, Lines.Other(LineFamily.Slash, 0));
            Assert.AreEqual(LineFamily.Column, Lines.Other(LineFamily.Slash, 1));
        }

        [Test]
        public void Board_TracksOwnersAndRejectsDoublePlacement()
        {
            var board = new Board();
            board.Place(Lines.CenterCell, new Card(Rank.King, Suit.Spades), Seat.House);
            Assert.AreEqual(Seat.House, board.OwnerOf(Lines.CenterCell));
            Assert.IsFalse(board.IsEmpty(Lines.CenterCell));
            Assert.AreEqual(13, board.ToValues()[Lines.CenterCell]);
            Assert.Throws<System.InvalidOperationException>(() => board.Place(Lines.CenterCell, new Card(Rank.Two, Suit.Clubs), Seat.Player));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => board.Place(9, new Card(Rank.Two, Suit.Clubs), Seat.Player));
            board.Clear();
            Assert.IsTrue(board.IsEmpty(Lines.CenterCell));
        }
    }
}
