using System;
using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    /// <summary>VECTOR のモードごとのベットエリアと上下限を、共通の賭け金の管理（WagerSession）とラウンドの進行で確かめる。</summary>
    public class VectorWageringTests
    {
        static readonly TableRules Rules = TableRules.Standard;

        static RoundEngine NewRound() => new RoundEngine(Rules, new Shoe(Rules.DeckCount, new SeededRandom(3)), new FirstEmptyCellDealer());

        static WagerSession NewSession(long balance, GameMode mode = GameMode.Player) => new WagerSession(Rules.LimitsFor(mode), new Bankroll(balance));

        static void PlayOut(RoundEngine round)
        {
            round.ChooseFamily(LineFamily.Row);
            while (round.Phase != RoundPhase.Showdown)
            {
                if (round.Phase == RoundPhase.PlayerTurn)
                    round.PlacePlayerCard(0, Array.IndexOf(round.Board.ToValues(), 0));
                else
                    round.PlayDealerCard();
            }
        }

        [Test]
        public void Chips_FollowModeSpotsAndTableLimits()
        {
            var session = NewSession(150);
            Assert.AreEqual(ChipError.None, session.TryAddChip((int)BetSpot.Main, 100));
            Assert.AreEqual(50, session.Available);
            Assert.AreEqual(ChipError.InsufficientFunds, session.TryAddChip((int)BetSpot.Main, 100));
            Assert.AreEqual(ChipError.SpotUnavailable, session.TryAddChip((int)BetSpot.Tie, 10));

            var audience = NewSession(1000, GameMode.Audience);
            Assert.AreEqual(ChipError.SpotUnavailable, audience.TryAddChip((int)BetSpot.Main, 10));
            Assert.AreEqual(ChipError.None, audience.TryAddChip((int)BetSpot.Tie, 10));

            var rich = NewSession(1_000_000);
            Assert.AreEqual(ChipError.None, rich.TryAddChip((int)BetSpot.Bonus, Rules.BonusMaxBet));
            Assert.AreEqual(ChipError.AboveMaximum, rich.TryAddChip((int)BetSpot.Bonus, 1));
        }

        [Test]
        public void BonusAloneIsNotEnoughToDeal()
        {
            var session = NewSession(1000);
            session.TryAddChip((int)BetSpot.Bonus, 5);
            Assert.AreEqual(BetError.NoPrimaryBet, session.CheckBets().Error);
            Assert.Throws<InvalidOperationException>(() => session.Commit());
            Assert.AreEqual(1000, session.Balance);
        }

        [Test]
        public void DealDebitsStakeAndSettleCreditsReturn()
        {
            var session = NewSession(1000);
            var round = NewRound();
            session.TryAddChip((int)BetSpot.Main, 100);
            session.TryAddChip((int)BetSpot.Bonus, 10);
            round.Deal(GameMode.Player, session.Commit());
            Assert.AreEqual(890, session.Balance);

            PlayOut(round);
            var result = round.Settle();
            Assert.AreEqual(110, result.TotalStake);
            session.Settle(result.TotalReturn);
            Assert.AreEqual(890 + result.TotalReturn, session.Balance);
        }

        [Test]
        public void AbortedRoundRefundsStakeOnce()
        {
            var session = NewSession(1000);
            var round = NewRound();
            session.TryAddChip((int)BetSpot.Main, 100);
            round.Deal(GameMode.Player, session.Commit());
            round.ChooseFamily(LineFamily.Column);

            Assert.AreEqual(100, round.Abort());
            Assert.AreEqual(100, session.Void());
            Assert.AreEqual(1000, session.Balance);
            Assert.AreEqual(0, round.Abort(), "二重に返金しない");
            Assert.AreEqual(0, session.Void(), "二重に返金しない");

            round.BeginBetting();
            Assert.AreEqual(ChipError.None, session.TryAddChip((int)BetSpot.Main, 10));
        }
    }
}
