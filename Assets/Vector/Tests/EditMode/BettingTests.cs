using System;
using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    public class BettingTests
    {
        static readonly TableRules Rules = TableRules.Standard;

        [Test]
        public void BetBoard_UndoRemovesChipsInReverseOrder()
        {
            var bets = new BetBoard();
            bets.Add((int)BetSpot.Main, 100);
            bets.Add((int)BetSpot.Bonus, 5);
            bets.Add((int)BetSpot.Main, 10);
            Assert.AreEqual(115, bets.Total);

            Assert.IsTrue(bets.TryUndo(out var spot, out long amount));
            Assert.AreEqual((int)BetSpot.Main, spot);
            Assert.AreEqual(10, amount);
            Assert.IsTrue(bets.TryUndo(out spot, out amount));
            Assert.AreEqual((int)BetSpot.Bonus, spot);
            Assert.AreEqual(0, bets[(int)BetSpot.Bonus]);
            Assert.AreEqual(100, bets.Total);
        }

        [Test]
        public void BetBoard_CloneKeepsAmountsAndHistory()
        {
            var bets = new BetBoard();
            bets.Add((int)BetSpot.Player, 50);
            bets.Add((int)BetSpot.Tie, 10);
            var copy = bets.Clone();
            bets.Clear();
            Assert.IsTrue(bets.IsEmpty);
            Assert.AreEqual(60, copy.Total);
            Assert.IsTrue(copy.TryUndo(out var spot, out _));
            Assert.AreEqual((int)BetSpot.Tie, spot);
        }

        [Test]
        public void BetBoard_RejectsNonPositiveChips()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BetBoard().Add((int)BetSpot.Main, 0));
            Assert.IsFalse(new BetBoard().TryUndo(out _, out _));
        }

        [Test]
        public void Validate_EnforcesModeLimitsAndPrimaryBet()
        {
            var bets = new BetBoard();
            Assert.AreEqual(BetError.NoPrimaryBet, WagerCheck.Validate(Rules.LimitsFor(GameMode.Player), bets).Error);

            bets.Add((int)BetSpot.Bonus, 5);
            Assert.AreEqual(BetError.NoPrimaryBet, WagerCheck.Validate(Rules.LimitsFor(GameMode.Player), bets).Error);

            bets.Add((int)BetSpot.Main, 5);
            var check = WagerCheck.Validate(Rules.LimitsFor(GameMode.Player), bets);
            Assert.AreEqual(BetError.BelowMinimum, check.Error);
            Assert.AreEqual((int)BetSpot.Main, check.Spot);
            Assert.AreEqual(Rules.MinBet, check.Limit);

            bets.Add((int)BetSpot.Main, 5);
            Assert.IsTrue(WagerCheck.Validate(Rules.LimitsFor(GameMode.Player), bets).IsValid);
            Assert.AreEqual(BetError.SpotUnavailable, WagerCheck.Validate(Rules.LimitsFor(GameMode.Audience), bets).Error);

            bets.Add((int)BetSpot.Main, Rules.MaxBet);
            Assert.AreEqual(BetError.AboveMaximum, WagerCheck.Validate(Rules.LimitsFor(GameMode.Player), bets).Error);
        }

        [Test]
        public void Settlement_MainBetWinsPushesAndLoses()
        {
            var win = Settlement.Settle(Rules, BetSpot.Main, 100, RoundWinner.Player, PokerHand.Nothing);
            Assert.AreEqual(BetOutcome.Win, win.Outcome);
            Assert.AreEqual(100 + Rules.MainPays.WinningsFor(100), win.Return);

            var push = Settlement.Settle(Rules, BetSpot.Main, 100, RoundWinner.Tie, PokerHand.Nothing);
            Assert.AreEqual(BetOutcome.Push, push.Outcome);
            Assert.AreEqual(0, push.Net);

            var lose = Settlement.Settle(Rules, BetSpot.Main, 100, RoundWinner.Dealer, PokerHand.Nothing);
            Assert.AreEqual(-100, lose.Net);
        }

        [Test]
        public void Settlement_BackBetsAndBonus()
        {
            Assert.AreEqual(BetOutcome.Push, Settlement.Settle(Rules, BetSpot.Dealer, 50, RoundWinner.Tie, PokerHand.Nothing).Outcome);
            Assert.AreEqual(BetOutcome.Lose, Settlement.Settle(Rules, BetSpot.Tie, 50, RoundWinner.Player, PokerHand.Nothing).Outcome);
            var tie = Settlement.Settle(Rules, BetSpot.Tie, 50, RoundWinner.Tie, PokerHand.Nothing);
            Assert.AreEqual(50 + Rules.BackTiePays.WinningsFor(50), tie.Return);

            var bonus = Settlement.Settle(Rules, BetSpot.Bonus, 10, RoundWinner.Dealer, PokerHand.FullHouse);
            Assert.AreEqual(10 + Rules.BonusPays[PokerHand.FullHouse].WinningsFor(10), bonus.Return);
            Assert.AreEqual(BetOutcome.Lose, Settlement.Settle(Rules, BetSpot.Bonus, 10, RoundWinner.Player, PokerHand.OnePair).Outcome);
        }

        [Test]
        public void Odds_RoundFractionalWinningsDown()
        {
            var odds = new Odds(5, 4);
            Assert.AreEqual(12, odds.WinningsFor(10));
            Assert.AreEqual(1, odds.WinningsFor(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Odds(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => odds.WinningsFor(-1));
        }

        [Test]
        public void Bankroll_NeverGoesNegative()
        {
            var bankroll = new Bankroll(100);
            Assert.IsFalse(bankroll.TryDebit(101));
            Assert.IsTrue(bankroll.TryDebit(100));
            Assert.AreEqual(0, bankroll.Balance);
            Assert.IsFalse(bankroll.TryDebit(-1));
            bankroll.Credit(30);
            Assert.AreEqual(30, bankroll.Balance);
            Assert.Throws<ArgumentOutOfRangeException>(() => bankroll.Credit(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Bankroll(-1));
        }
    }
}
