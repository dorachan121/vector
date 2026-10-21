using System;
using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    public class WagerSessionTests
    {
        const int Main = 0;
        const int Side = 1;
        const int Closed = 2;

        /// <summary>メイン $10〜$1,000、サイド $5〜$100、番号2は使えないテーブル。</summary>
        sealed class TestLimits : IBetLimits
        {
            public bool IsSpotAvailable(int spot) => spot == Main || spot == Side;
            public long MinFor(int spot) => spot == Side ? 5 : 10;
            public long MaxFor(int spot) => spot == Side ? 100 : 1_000;
            public bool IsPrimary(int spot) => spot == Main;
        }

        static WagerSession NewSession(long balance = 500) => new WagerSession(new TestLimits(), new Bankroll(balance));

        [Test]
        public void AddChip_ChecksSpotFundsAndMaximum()
        {
            var session = NewSession(500);
            Assert.AreEqual(ChipError.SpotUnavailable, session.TryAddChip(Closed, 10));
            Assert.AreEqual(ChipError.None, session.TryAddChip(Main, 400));
            Assert.AreEqual(ChipError.InsufficientFunds, session.TryAddChip(Main, 101));
            Assert.AreEqual(ChipError.None, session.TryAddChip(Side, 100));
            Assert.AreEqual(ChipError.InsufficientFunds, session.TryAddChip(Side, 1));
            Assert.AreEqual(0, session.Available);
            Assert.AreEqual(500, session.Balance, "引き落とすのは勝負に入るとき");

            var rich = NewSession(10_000);
            Assert.AreEqual(ChipError.None, rich.TryAddChip(Side, 100));
            Assert.AreEqual(ChipError.AboveMaximum, rich.TryAddChip(Side, 1));
        }

        [Test]
        public void Validation_RequiresPrimaryBetAndMinimum()
        {
            var session = NewSession();
            Assert.AreEqual(BetError.NoPrimaryBet, session.CheckBets().Error);
            session.TryAddChip(Side, 5);
            Assert.AreEqual(BetError.NoPrimaryBet, session.CheckBets().Error);
            session.TryAddChip(Main, 5);
            var check = session.CheckBets();
            Assert.AreEqual(BetError.BelowMinimum, check.Error);
            Assert.AreEqual(Main, check.Spot);
            Assert.AreEqual(10, check.Limit);
            session.TryAddChip(Main, 5);
            Assert.IsTrue(session.CheckBets().IsValid);
        }

        [Test]
        public void CommitSettle_DebitsThenCreditsAndRemembersRebet()
        {
            var session = NewSession(500);
            session.TryAddChip(Main, 100);
            session.TryAddChip(Side, 20);
            var inPlay = session.Commit();
            Assert.AreEqual(120, inPlay.Total);
            Assert.AreEqual(380, session.Balance);
            Assert.IsFalse(session.IsBetting);
            Assert.AreEqual(ChipError.NotBetting, session.TryAddChip(Main, 10));
            Assert.Throws<InvalidOperationException>(() => session.Commit());

            session.Settle(200);
            Assert.AreEqual(580, session.Balance);
            Assert.IsTrue(session.IsBetting);
            Assert.IsTrue(session.InPlay.IsEmpty);
            Assert.IsTrue(session.TryRebet());
            Assert.AreEqual(100, session.Bets[Main]);
            Assert.AreEqual(20, session.Bets[Side]);
            Assert.IsTrue(session.TryDouble());
            Assert.AreEqual(240, session.Bets.Total);
        }

        [Test]
        public void Double_RespectsTableMaximumAndFunds()
        {
            var session = NewSession(10_000);
            session.TryAddChip(Main, 600);
            Assert.IsFalse(session.CanDouble, "1,200 はマキシマム超過");
            session.ClearBets();
            session.TryAddChip(Main, 400);
            Assert.IsTrue(session.TryDouble());
            Assert.AreEqual(800, session.Bets[Main]);

            var poor = NewSession(150);
            poor.TryAddChip(Main, 100);
            Assert.IsFalse(poor.CanDouble);
        }

        [Test]
        public void Void_RefundsOnlyOnce()
        {
            var session = NewSession(500);
            session.TryAddChip(Main, 300);
            session.Commit();
            Assert.AreEqual(300, session.Void());
            Assert.AreEqual(500, session.Balance);
            Assert.AreEqual(0, session.Void());
            Assert.AreEqual(500, session.Balance);
            Assert.Throws<InvalidOperationException>(() => session.Settle(0));
        }

        [Test]
        public void Undo_RemovesChipsInReverseOrder()
        {
            var session = NewSession();
            session.TryAddChip(Main, 10);
            session.TryAddChip(Side, 5);
            session.TryAddChip(Main, 25);
            Assert.IsTrue(session.TryUndo(out int spot, out long amount));
            Assert.AreEqual(Main, spot);
            Assert.AreEqual(25, amount);
            Assert.IsTrue(session.TryUndo(out spot, out amount));
            Assert.AreEqual(Side, spot);
            Assert.AreEqual(10, session.Bets.Total);
        }

        [Test]
        public void Broke_WhenAvailableBelowMinimum()
        {
            var session = NewSession(9);
            Assert.IsTrue(session.IsBrokeFor(10));
            session.Deposit(1);
            Assert.IsFalse(session.IsBrokeFor(10));
        }

        [Test]
        public void SpotResult_PaysOddsAndPushes()
        {
            var win = SpotResult.Win(Main, 100, new Odds(3, 2));
            Assert.AreEqual(250, win.Return);
            Assert.AreEqual(150, win.Net);
            Assert.AreEqual(100, SpotResult.Push(Main, 100).Return);
            Assert.AreEqual(-100, SpotResult.Lose(Main, 100).Net);
            var all = new[] { win, SpotResult.Lose(Side, 20) };
            Assert.AreEqual(120, SpotResult.TotalStake(all));
            Assert.AreEqual(250, SpotResult.TotalReturn(all));
        }
    }
}
