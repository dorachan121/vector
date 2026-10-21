using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Crown;

namespace Vector.Tests
{
    public class CrownTests
    {
        static DiceRank Rank(int crown, params int[] dice) => DiceRank.Of(dice, crown);

        [Test]
        public void Rank_OrdersHandKinds()
        {
            const int crown = 3;
            Assert.AreEqual(DiceHandKind.NoHand, Rank(crown, 1, 2, 3, 4, 6).Kind);
            Assert.AreEqual(DiceHandKind.OnePair, Rank(crown, 1, 1, 3, 4, 6).Kind);
            Assert.AreEqual(DiceHandKind.TwoPair, Rank(crown, 1, 1, 3, 3, 6).Kind);
            Assert.AreEqual(DiceHandKind.ThreeOfAKind, Rank(crown, 1, 1, 1, 3, 6).Kind);
            Assert.AreEqual(DiceHandKind.Straight, Rank(crown, 5, 4, 3, 2, 1).Kind);
            Assert.AreEqual(DiceHandKind.Straight, Rank(crown, 2, 3, 4, 5, 6).Kind);
            Assert.AreEqual(DiceHandKind.FullHouse, Rank(crown, 2, 2, 2, 5, 5).Kind);
            Assert.AreEqual(DiceHandKind.FourOfAKind, Rank(crown, 6, 6, 6, 6, 1).Kind);
            Assert.AreEqual(DiceHandKind.Yacht, Rank(crown, 4, 4, 4, 4, 4).Kind);

            var ascending = new[]
            {
                Rank(crown, 1, 2, 3, 4, 6), Rank(crown, 1, 1, 3, 4, 6), Rank(crown, 1, 1, 3, 3, 6), Rank(crown, 1, 1, 1, 3, 6),
                Rank(crown, 1, 2, 3, 4, 5), Rank(crown, 2, 3, 4, 5, 6), Rank(crown, 2, 2, 2, 5, 5), Rank(crown, 6, 6, 6, 6, 1),
                Rank(crown, 4, 4, 4, 4, 4),
            };
            for (int i = 1; i < ascending.Length; i++)
                Assert.That(ascending[i] > ascending[i - 1], $"{ascending[i]} は {ascending[i - 1]} より強い");
        }

        [Test]
        public void Rank_CrownFaceBeatsSix()
        {
            Assert.That(Rank(2, 2, 2, 1, 3, 4) > Rank(2, 6, 6, 1, 3, 4), "王冠(2)のペアは6のペアより強い");
            Assert.That(Rank(1, 1, 1, 1, 1, 1) > Rank(1, 6, 6, 6, 6, 6), "王冠(1)のヨットが最強");
            Assert.That(Rank(5, 5, 5, 5, 5, 2) > Rank(5, 6, 6, 6, 6, 2), "王冠(5)のフォーカードは6のフォーカードより強い");
            Assert.That(Rank(5, 1, 1, 1, 1, 1) > Rank(5, 5, 5, 5, 5, 2), "役が違えば役で比べる");
            Assert.That(Rank(4, 6, 6, 6, 4, 4) < Rank(4, 4, 4, 4, 6, 6), "フルハウスは3個の組の強さで比べる");
            Assert.That(Rank(6, 1, 2, 3, 4, 5) < Rank(1, 2, 3, 4, 5, 6), "ストレートは王冠に関係なく 6 ハイが強い");
        }

        [Test]
        public void Rank_ExposesBothGroupsForTwoPairAndFullHouse()
        {
            var twoPair = Rank(3, 6, 6, 5, 5, 2);
            Assert.AreEqual(6, twoPair.MainFace);
            Assert.AreEqual(5, twoPair.SecondFace);
            var crowned = Rank(3, 6, 6, 3, 3, 2);
            Assert.AreEqual(3, crowned.MainFace, "王冠(3)のペアが上の組");
            Assert.AreEqual(6, crowned.SecondFace);
            var fullHouse = Rank(3, 2, 2, 2, 5, 5);
            Assert.AreEqual(2, fullHouse.MainFace);
            Assert.AreEqual(5, fullHouse.SecondFace);
            Assert.AreEqual(0, Rank(3, 1, 1, 1, 5, 6).SecondFace, "スリーカードに2つ目の組はない");
        }

        [Test]
        public void Rank_TiesOnlyForIdenticalDice()
        {
            Assert.That(Rank(3, 1, 2, 2, 5, 6) == Rank(3, 6, 5, 2, 1, 2));
            Assert.That(Rank(3, 1, 2, 2, 5, 6) != Rank(3, 1, 2, 2, 4, 6));
            Assert.That(Rank(3, 1, 2, 3, 4, 5) == Rank(3, 5, 4, 3, 2, 1));
        }

        [Test]
        public void Tables_EnumerateAllHands()
        {
            double sum = 0;
            foreach (double p in DiceTables.InitialProbabilities)
                sum += p;
            Assert.AreEqual(1.0, sum, 1e-12);
            int yacht = DiceTables.HandIndex(new[] { 0, 0, 5, 0, 0, 0 });
            Assert.AreEqual(1.0 / 7776, DiceTables.InitialProbabilities[yacht], 1e-15);
            Assert.AreEqual(32, CountDistinctHoldsOf(new[] { 1, 2, 3, 4, 5 }));
            Assert.AreEqual(6, CountDistinctHoldsOf(new[] { 4, 4, 4, 4, 4 }));
        }

        static int CountDistinctHoldsOf(int[] dice) => DiceTables.HoldsOf(DiceTables.HandIndex(DiceTables.CountsOf(dice))).Count;

        [Test]
        public void Strategy_OptimalRtpMatchesPublishedValue()
        {
            double rtp = new CrownAdvisor(CrownRules.Standard).ExpectedReturn();
            Assert.That(rtp, Is.InRange(0.965, 0.972), "README の 96.85% から変わったら配当表を見直す");
        }

        [Test]
        public void Strategy_StandsOnCrownYachtAndKeepsFourCrowns()
        {
            var strategy = new CrownAdvisor(CrownRules.Standard).For(2);
            Assert.IsTrue(strategy.Advise(new[] { 2, 2, 2, 2, 2 }, 2).Stand);
            var advice = strategy.Advise(new[] { 2, 2, 5, 2, 2 }, 1);
            Assert.AreEqual(0b11011, advice.HoldMask, "王冠の4個を残して1個を振る");
            Assert.That(advice.ExpectedReturn, Is.GreaterThan(advice.StandReturn));
        }

        [Test]
        public void DealerPlan_StopsWhenAlreadyBeatingPlayer()
        {
            var plan = DealerPlan.For(CrownRules.Standard, Rank(4, 1, 1, 2, 3, 5), 4);
            Assert.AreEqual(0b11111, plan.ChooseHold(new[] { 6, 6, 6, 1, 2 }, 2));
            int hold = plan.ChooseHold(new[] { 6, 5, 3, 2, 1 }, 2);
            Assert.AreNotEqual(0b11111, hold, "負けている間は振り直す");
            Assert.That(plan.PlayerReturn, Is.InRange(0.0, 2.0));
        }

        [Test]
        public void Engine_PlaysFullRound()
        {
            var engine = new CrownEngine(CrownRules.Standard, new SeededRandom(3));
            var bets = new BetBoard();
            bets.Add(CrownRules.Ante, 100);
            bets.Add(CrownRules.Opening, 10);
            engine.Deal(bets);
            Assert.AreEqual(CrownPhase.PlayerTurn, engine.Phase);
            Assert.That(engine.Crown, Is.InRange(1, 6));
            Assert.AreEqual(2, engine.PlayerRerollsLeft);
            var held = engine.PlayerDice[0];
            engine.Reroll(0b00001);
            Assert.AreEqual(held, engine.PlayerDice[0]);
            Assert.AreEqual(1, engine.PlayerRerollsLeft);
            engine.Stand();
            Assert.AreEqual(CrownPhase.DealerTurn, engine.Phase);

            int dealerMoves = 0;
            while (engine.Phase == CrownPhase.DealerTurn)
            {
                engine.PlayDealer();
                dealerMoves++;
            }
            Assert.That(dealerMoves, Is.InRange(1, 3));
            var result = engine.Settle();
            Assert.AreEqual(CrownPhase.Complete, engine.Phase);
            Assert.AreEqual(110, result.TotalStake);
            Assert.AreEqual(2, result.Bets.Count);
            var expected = result.PlayerRank > result.DealerRank ? RoundWinner.Player : result.PlayerRank == result.DealerRank ? RoundWinner.Tie : RoundWinner.Dealer;
            Assert.AreEqual(expected, result.Winner);
        }

        [Test]
        public void Engine_RejectsActionsOutOfTurn()
        {
            var engine = new CrownEngine(CrownRules.Standard, new SeededRandom(4));
            Assert.Throws<InvalidOperationException>(() => engine.Stand());
            Assert.Throws<InvalidOperationException>(() => engine.Deal(new BetBoard()));
            var bets = new BetBoard();
            bets.Add(CrownRules.Ante, 10);
            engine.Deal(bets);
            Assert.Throws<ArgumentOutOfRangeException>(() => engine.Reroll(0b11111));
            Assert.Throws<InvalidOperationException>(() => engine.PlayDealer());
            Assert.Throws<InvalidOperationException>(() => engine.Settle());
            engine.Reroll(0);
            engine.Reroll(0);
            Assert.AreEqual(CrownPhase.DealerTurn, engine.Phase, "振り直しを使い切るとディーラーの番");
        }

        [Test]
        public void Engine_PaysCrownYachtBonusEvenOnTie()
        {
            // 毎回同じ目が出る乱数: 王冠もプレイヤーもディーラーも全部「4」
            var engine = new CrownEngine(CrownRules.Standard, new FixedDie(3));
            var bets = new BetBoard();
            bets.Add(CrownRules.Ante, 100);
            bets.Add(CrownRules.Opening, 10);
            engine.Deal(bets);
            Assert.AreEqual(4, engine.Crown);
            Assert.AreEqual(OpeningHand.CrownYacht, engine.Opening);
            engine.Stand();
            while (engine.Phase == CrownPhase.DealerTurn)
                engine.PlayDealer();
            var result = engine.Settle();
            Assert.AreEqual(RoundWinner.Tie, result.Winner);
            Assert.IsTrue(result.CrownYachtBonus);
            Assert.AreEqual(100 + 500, result.Bets[0].Return, "引き分けでもボーナス 5:1");
            Assert.AreEqual(10 + 10_000, result.Bets[1].Return, "オープニング 1000:1");
        }

        [Test]
        public void Opening_IgnoresPlayerChoices()
        {
            var hands = new Dictionary<OpeningHand, int>();
            var dice = new int[5];
            for (int code = 0; code < 7776; code++)
            {
                int x = code;
                for (int i = 0; i < 5; i++)
                {
                    dice[i] = x % 6 + 1;
                    x /= 6;
                }
                var hand = OpeningHands.Of(DiceRank.Of(dice, 6), 6);
                hands.TryGetValue(hand, out int c);
                hands[hand] = c + 1;
            }
            Assert.AreEqual(1, hands[OpeningHand.CrownYacht]);
            Assert.AreEqual(5, hands[OpeningHand.Yacht]);
            Assert.AreEqual(25, hands[OpeningHand.CrownFour]);
            Assert.AreEqual(125, hands[OpeningHand.FourOfAKind]);
            Assert.AreEqual(300, hands[OpeningHand.FullHouse]);
            Assert.AreEqual(240, hands[OpeningHand.Straight]);
            Assert.AreEqual(200, hands[OpeningHand.CrownThree]);
        }

        sealed class FixedDie : IRandomSource
        {
            readonly int value;
            public FixedDie(int value) => this.value = value;
            public int Next(int maxExclusive) => value % maxExclusive;
        }
    }
}
