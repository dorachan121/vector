using System;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Derby;

namespace Vector.Tests
{
    public class DerbyTests
    {
        static readonly int[] FullDeck = { 13, 13, 13, 13 };

        [Test]
        public void Tickets_EncodeWinAndExacta()
        {
            Assert.AreEqual(2, DerbyTickets.Win(2));
            int exacta = DerbyTickets.Exacta(3, 1);
            Assert.IsTrue(DerbyTickets.IsExacta(exacta));
            Assert.AreEqual(3, DerbyTickets.First(exacta));
            Assert.AreEqual(1, DerbyTickets.Second(exacta));
            Assert.IsFalse(DerbyTickets.IsValid(DerbyTickets.Runners + 0 * 4 + 0), "同じ走者の馬単は無い");
            Assert.Throws<ArgumentException>(() => DerbyTickets.Exacta(1, 1));
            int valid = 0;
            for (int t = 0; t < DerbyTickets.Count; t++)
                valid += DerbyTickets.IsValid(t) ? 1 : 0;
            Assert.AreEqual(4 + 12, valid);
        }

        [Test]
        public void Probabilities_AreSymmetricForEvenRace()
        {
            var p = RaceProbabilities.Compute(new[] { 10, 10, 10, 10 }, FullDeck);
            for (int r = 0; r < 4; r++)
            {
                Assert.AreEqual(0.25, p.Win[r], 1e-12);
                for (int s = 0; s < 4; s++)
                    Assert.AreEqual(r == s ? 0 : 1.0 / 12, p.Exacta[r, s], 1e-12);
            }
        }

        [Test]
        public void Probabilities_SumToOneAndRowsMatchWin()
        {
            var p = RaceProbabilities.Compute(new[] { 7, 10, 9, 12 }, FullDeck);
            double total = 0;
            for (int r = 0; r < 4; r++)
            {
                double row = 0;
                for (int s = 0; s < 4; s++)
                    row += p.Exacta[r, s];
                Assert.AreEqual(p.Win[r], row, 1e-12);
                total += p.Win[r];
            }
            Assert.AreEqual(1.0, total, 1e-12);
            Assert.That(p.Win[0], Is.GreaterThan(p.Win[2]));
            Assert.That(p.Win[2], Is.GreaterThan(p.Win[1]));
            Assert.That(p.Win[1], Is.GreaterThan(p.Win[3]));
        }

        [Test]
        public void Probabilities_HandComputedTwoStepRace()
        {
            // 走者0 は1歩、走者1 は2歩で、他の2人は13枚全部が必要（ほぼ勝てない）。
            // 走者1 が勝つのは、♠♥の26枚のうち最初の2枚がどちらも♥のとき: 13/26 × 12/25 = 0.24
            var p = RaceProbabilities.Compute(new[] { 1, 2, 13, 13 }, FullDeck);
            Assert.AreEqual(0.24, p.Win[1], 1e-6);
            Assert.AreEqual(0.76, p.Win[0], 1e-6);
            Assert.That(p.Win[2], Is.LessThan(1e-6));
        }

        [Test]
        public void LiveProbabilities_KnowTheWinner()
        {
            var p = RaceProbabilities.Compute(new[] { 0, 1, 5, 5 }, new[] { 4, 13, 13, 13 }, 0);
            Assert.AreEqual(1.0, p.Win[0]);
            Assert.That(p.Exacta[0, 1], Is.GreaterThan(0.8));
            Assert.AreEqual(1.0, p.Exacta[0, 1] + p.Exacta[0, 2] + p.Exacta[0, 3], 1e-12);
        }

        [Test]
        public void Odds_NeverExceedTargetReturn()
        {
            var rules = DerbyRules.Standard;
            var card = rules.CardFor(9, new[] { 3, 0, 1, 2 });
            for (int t = 0; t < DerbyTickets.Count; t++)
            {
                if (!DerbyTickets.IsValid(t) || card.OddsTenths(t) == 0)
                    continue;
                Assert.That(card.ReturnOf(t), Is.LessThanOrEqualTo(rules.TargetReturn + 1e-9), $"ticket {t}");
                Assert.That(card.ReturnOf(t), Is.GreaterThan(rules.TargetReturn * 0.9), $"ticket {t}");
            }
            Assert.AreEqual(0, rules.OddsTenthsFor(0.95), "1.1倍未満の馬券は発売しない");
            Assert.AreEqual(9999, rules.OddsTenthsFor(1e-6));
            Assert.AreSame(card, rules.CardFor(9, new[] { 3, 0, 1, 2 }), "同じ出走表は使い回す");
        }

        [Test]
        public void Engine_RunsRaceUntilTwoFinish()
        {
            var engine = new DerbyEngine(DerbyRules.Standard, new SeededRandom(8));
            var bets = new BetBoard();
            bets.Add(DerbyTickets.Win(0), 100);
            bets.Add(DerbyTickets.Exacta(1, 2), 10);
            engine.Start(bets);
            int steps = 0;
            RaceStep last = default;
            while (engine.Phase == DerbyPhase.Running)
            {
                last = engine.Step();
                steps++;
                Assert.That(engine.Positions[last.Runner], Is.LessThanOrEqualTo(engine.Race.Distance));
            }
            Assert.AreEqual(2, last.FinishedPlace);
            Assert.AreEqual(steps, engine.CardsDrawn);
            var result = engine.Settle();
            Assert.AreNotEqual(result.First, result.Second);
            Assert.AreEqual(engine.Race.Distance, result.Positions[result.First]);
            Assert.AreEqual(110, result.TotalStake);
            foreach (var bet in result.Bets)
            {
                bool hit = bet.Spot == DerbyTickets.Win(result.First) || bet.Spot == result.WinningExacta;
                Assert.AreEqual(hit ? BetOutcome.Win : BetOutcome.Lose, bet.Outcome);
                if (hit)
                    Assert.AreEqual(bet.Stake * engine.Race.OddsTenths(bet.Spot) / 10, bet.Return);
            }
            engine.BeginBetting();
            Assert.AreEqual(DerbyPhase.Betting, engine.Phase);
            Assert.AreEqual(-1, engine.First);
        }

        [Test]
        public void Engine_RejectsBetsOutsideTheBook()
        {
            var engine = new DerbyEngine(DerbyRules.Standard, new SeededRandom(1));
            var bets = new BetBoard();
            bets.Add(DerbyTickets.Exacta(0, 1), 5_000);
            Assert.Throws<InvalidOperationException>(() => engine.Start(bets), "馬単のマキシマムは 1,000");
            Assert.Throws<InvalidOperationException>(() => engine.Step());
            Assert.Throws<InvalidOperationException>(() => engine.BeginBetting(), "受付中にもう一度は呼べない");
            var session = new WagerSession(engine.Limits, new Bankroll(10_000));
            Assert.AreEqual(ChipError.AboveMaximum, session.TryAddChip(DerbyTickets.Exacta(0, 1), 1_001));
            Assert.AreEqual(ChipError.None, session.TryAddChip(DerbyTickets.Win(3), 10));
        }
    }
}
