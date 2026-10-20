using System;
using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    public class TableSessionTests
    {
        static TableSession NewSession(long balance, GameMode mode = GameMode.Player)
        {
            var rules = TableRules.Standard;
            var round = new RoundEngine(rules, new Shoe(rules.DeckCount, new SeededRandom(3)), new FirstEmptyCellDealer());
            return new TableSession(rules, new Bankroll(balance), round, mode);
        }

        static void PlayOut(TableSession session)
        {
            session.Round.ChooseFamily(LineFamily.Row);
            while (session.Round.Phase != RoundPhase.Showdown)
            {
                if (session.Round.Phase == RoundPhase.PlayerTurn)
                    session.Round.PlacePlayerCard(0, Array.IndexOf(session.Round.Board.ToValues(), 0));
                else
                    session.Round.PlayDealerCard();
            }
        }

        [Test]
        public void Chips_CannotExceedAvailableBalanceOrTableMaximum()
        {
            var session = NewSession(150);
            Assert.AreEqual(ChipError.None, session.TryAddChip(BetSpot.Main, 100));
            Assert.AreEqual(50, session.Available);
            Assert.AreEqual(ChipError.InsufficientFunds, session.TryAddChip(BetSpot.Main, 100));
            Assert.AreEqual(ChipError.SpotUnavailable, session.TryAddChip(BetSpot.Tie, 10));

            var rich = NewSession(1_000_000);
            Assert.AreEqual(ChipError.None, rich.TryAddChip(BetSpot.Bonus, rich.Rules.BonusMaxBet));
            Assert.AreEqual(ChipError.AboveMaximum, rich.TryAddChip(BetSpot.Bonus, 1));
        }

        [Test]
        public void Deal_DebitsStakeAndSettleCreditsReturn()
        {
            var session = NewSession(1000);
            session.TryAddChip(BetSpot.Main, 100);
            session.Deal();
            Assert.AreEqual(900, session.Balance);
            Assert.IsTrue(session.Bets.IsEmpty);
            Assert.AreEqual(ChipError.NotBetting, session.TryAddChip(BetSpot.Main, 10));

            PlayOut(session);
            var result = session.Settle();
            Assert.AreEqual(900 + result.TotalReturn, session.Balance);
            Assert.AreEqual(1, session.Scoreboard.Entries.Count);
        }

        [Test]
        public void Deal_RejectsInvalidBetsWithoutDebiting()
        {
            var session = NewSession(1000);
            session.TryAddChip(BetSpot.Bonus, 5);
            Assert.AreEqual(BetError.NoPrimaryBet, session.CheckBets().Error);
            Assert.Throws<InvalidOperationException>(() => session.Deal());
            Assert.AreEqual(1000, session.Balance);
        }

        [Test]
        public void RebetAndDouble()
        {
            var session = NewSession(1000);
            Assert.IsFalse(session.CanRebet);
            session.TryAddChip(BetSpot.Main, 100);
            session.TryAddChip(BetSpot.Bonus, 10);
            Assert.IsTrue(session.TryDouble());
            Assert.AreEqual(200, session.Bets[BetSpot.Main]);
            Assert.AreEqual(20, session.Bets[BetSpot.Bonus]);

            session.Deal();
            PlayOut(session);
            session.Settle();
            session.BeginBetting();

            Assert.IsTrue(session.TryRebet());
            Assert.AreEqual(220, session.Bets.Total);
            Assert.IsFalse(session.TryRebet(), "既にベットがあるときはリベットできない");
        }

        [Test]
        public void IsBroke_WhenBalanceIsBelowTableMinimum()
        {
            Assert.IsTrue(NewSession(5).IsBroke);
            Assert.IsFalse(NewSession(10).IsBroke);
        }

        [Test]
        public void VoidRound_RefundsStakeOnceAndAllowsNextRound()
        {
            var session = NewSession(1000);
            session.TryAddChip(BetSpot.Main, 100);
            session.Deal();
            session.Round.ChooseFamily(LineFamily.Column);
            Assert.AreEqual(100, session.VoidRound());
            Assert.AreEqual(1000, session.Balance);
            Assert.AreEqual(0, session.VoidRound(), "二重に返金しない");
            Assert.AreEqual(1000, session.Balance);

            session.BeginBetting();
            Assert.AreEqual(ChipError.None, session.TryAddChip(BetSpot.Main, 10));
        }

        [Test]
        public void VoidRound_AfterSettlementRefundsNothing()
        {
            var session = NewSession(1000);
            session.TryAddChip(BetSpot.Main, 100);
            session.Deal();
            PlayOut(session);
            session.Settle();
            long balance = session.Balance;
            Assert.AreEqual(0, session.VoidRound());
            Assert.AreEqual(balance, session.Balance);
        }

        [Test]
        public void UndoAndClearOnlyDuringBetting()
        {
            var session = NewSession(1000);
            session.TryAddChip(BetSpot.Main, 10);
            session.TryAddChip(BetSpot.Main, 5);
            Assert.IsTrue(session.TryUndo(out _, out long amount));
            Assert.AreEqual(5, amount);
            session.ClearBets();
            Assert.IsTrue(session.Bets.IsEmpty);
        }
    }
}
