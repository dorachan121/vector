using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    public class RoundEngineTests
    {
        static RoundEngine NewEngine(ulong seed = 11)
            => new RoundEngine(TableRules.Standard, new Shoe(6, new SeededRandom(seed)), new FirstEmptyCellDealer());

        static BetBoard MainBet(long amount = 100)
        {
            var bets = new BetBoard();
            bets.Add((int)BetSpot.Main, amount);
            return bets;
        }

        [Test]
        public void Deal_PlacesCenterCardAndFourCardHand()
        {
            var engine = NewEngine();
            engine.Deal(GameMode.Player, MainBet());
            Assert.AreEqual(RoundPhase.ChoosingFamily, engine.Phase);
            Assert.AreEqual(Seat.House, engine.Board.OwnerOf(Lines.CenterCell));
            Assert.AreEqual(RoundEngine.HandSize, engine.Hand.Count);
            Assert.AreEqual(1, engine.RoundNumber);
        }

        [Test]
        public void Deal_RejectsInvalidBets()
        {
            var engine = NewEngine();
            Assert.Throws<InvalidOperationException>(() => engine.Deal(GameMode.Player, new BetBoard()));
            Assert.AreEqual(RoundPhase.Betting, engine.Phase);
        }

        [Test]
        public void FullRound_AlternatesTurnsAndSettles()
        {
            var engine = NewEngine();
            engine.Deal(GameMode.Player, MainBet());
            engine.ChooseFamily(LineFamily.Row);

            int playerMoves = 0, dealerMoves = 0;
            while (engine.Phase != RoundPhase.Showdown)
            {
                if (engine.Phase == RoundPhase.PlayerTurn)
                {
                    int cell = Array.IndexOf(engine.Board.ToValues(), 0);
                    engine.PlacePlayerCard(0, cell);
                    playerMoves++;
                }
                else
                {
                    Assert.AreEqual(RoundPhase.DealerTurn, engine.Phase);
                    var move = engine.PlayDealerCard();
                    Assert.AreEqual(Seat.Dealer, engine.Board.OwnerOf(move.Cell));
                    dealerMoves++;
                }
            }
            Assert.AreEqual(4, playerMoves);
            Assert.AreEqual(4, dealerMoves);
            Assert.AreEqual(0, engine.Hand.Count);

            var result = engine.Settle();
            Assert.AreEqual(RoundPhase.Complete, engine.Phase);
            Assert.AreEqual(LineFamily.Row, result.PlayerFamily);
            Assert.AreNotEqual(LineFamily.Row, result.DealerFamily);
            var values = engine.Board.ToValues();
            Assert.AreEqual(Scoring.FamilyScore(values, LineFamily.Row), result.PlayerScore);
            Assert.AreEqual(Scoring.DealerScore(values, LineFamily.Row), result.DealerScore);
            Assert.AreEqual(1, result.Bets.Count);
            Assert.AreEqual(100, result.TotalStake);
        }

        [Test]
        public void ActionsOutOfTurnAreRejected()
        {
            var engine = NewEngine();
            Assert.Throws<InvalidOperationException>(() => engine.ChooseFamily(LineFamily.Row));
            engine.Deal(GameMode.Player, MainBet());
            Assert.Throws<InvalidOperationException>(() => engine.PlacePlayerCard(0, 0));
            Assert.Throws<InvalidOperationException>(() => engine.PlayDealerCard());
            Assert.Throws<InvalidOperationException>(() => engine.Settle());
            Assert.Throws<InvalidOperationException>(() => engine.BeginBetting());
            engine.ChooseFamily(LineFamily.Column);
            Assert.Throws<InvalidOperationException>(() => engine.PlacePlayerCard(0, Lines.CenterCell));
            Assert.Throws<ArgumentOutOfRangeException>(() => engine.PlacePlayerCard(4, 0));
        }

        [Test]
        public void BeginBetting_ReshufflesOnlyAfterCutCard()
        {
            var engine = NewEngine();
            int shuffles = 0;
            for (int round = 0; round < 40; round++)
            {
                if (engine.BeginBetting())
                    shuffles++;
                PlayRound(engine);
            }
            Assert.AreEqual(1, shuffles, "312枚のシューでは約26ラウンドで1回だけシャッフルされる");
            Assert.AreEqual(2, engine.Shoe.ShoeNumber);
        }

        [Test]
        public void HouseWay_AlwaysChoosesAnEmptyCell()
        {
            var house = new HouseWay(new SeededRandom(5), 8);
            int[] values = { 13, 0, 2, 0, 7, 0, 0, 9, 0 };
            int cell = house.ChooseCell(values, 12, LineFamily.Row, new List<int> { 1, 13 });
            Assert.AreEqual(0, values[cell]);
        }

        [Test]
        public void Advisor_SuggestsLegalMoves()
        {
            var advisor = new PlayerAdvisor(new SeededRandom(9), TableRules.Standard, 12, 6);
            int[] values = { 0, 0, 0, 0, 7, 0, 0, 0, 0 };
            var hand = new List<int> { 13, 1, 12, 2 };
            var estimates = advisor.EstimateFamilies(values, hand);
            Assert.AreEqual(3, estimates.Length);
            var family = PlayerAdvisor.Best(estimates);
            var move = advisor.SuggestPlacement(values, hand, family);
            Assert.That(move.HandIndex, Is.InRange(0, 3));
            Assert.AreEqual(0, values[move.Cell]);
        }

        [Test]
        public void GreedyPolicy_CompletesItsOwnStrongLine()
        {
            // 上段に K K が並んでいれば、Row を選んだプレイヤーは3枚目の K を右上に置いて 39 を作る
            int[] values = { 13, 13, 0, 0, 7, 0, 0, 0, 0 };
            var p = GreedyPolicy.ChoosePlacement(values, new List<int> { 1, 13 }, LineFamily.Row);
            Assert.AreEqual(1, p.HandIndex);
            Assert.AreEqual(2, p.Cell);
        }

        static void PlayRound(RoundEngine engine)
        {
            engine.Deal(GameMode.Player, MainBet());
            engine.ChooseFamily(LineFamily.Slash);
            while (engine.Phase != RoundPhase.Showdown)
            {
                if (engine.Phase == RoundPhase.PlayerTurn)
                    engine.PlacePlayerCard(0, Array.IndexOf(engine.Board.ToValues(), 0));
                else
                    engine.PlayDealerCard();
            }
            engine.Settle();
        }
    }
}
