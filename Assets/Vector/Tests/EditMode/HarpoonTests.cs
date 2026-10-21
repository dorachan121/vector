using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Harpoon;

namespace Vector.Tests
{
    public class HarpoonTests
    {
        /// <summary>決めた値を順に返す乱数（捕まえたかどうか → 金のクラゲなら倍率の順に引かれる）。</summary>
        sealed class Scripted : IRandomSource
        {
            readonly Queue<int> values;
            public Scripted(params int[] values) => this.values = new Queue<int>(values);
            public int Next(int maxExclusive)
            {
                int value = values.Dequeue();
                Assert.That(value, Is.InRange(0, maxExclusive - 1));
                return value;
            }
        }

        static BetBoard Bet(int quarry, long amount)
        {
            var board = new BetBoard();
            board.Add(quarry, amount);
            return board;
        }

        [Test]
        public void Rules_EveryQuarryHasTheSameReturn()
        {
            var rules = HarpoonRules.Standard;
            for (int q = 0; q < Quarry.Count; q++)
            {
                Assert.AreEqual(0.97, rules.ReturnOf(q), 1e-12, $"quarry {q}");
                Assert.That(rules.MinMultiple(q), Is.GreaterThanOrEqualTo(2), "捕まえたら必ず賭け金より多く戻る");
                var (hits, outOf) = rules.CatchOdds(q);
                Assert.That(hits, Is.GreaterThan(0).And.LessThan(outOf));
            }
            Assert.AreEqual(0.485, rules.CatchProbability(Quarry.Sardine), 1e-15);
            Assert.AreEqual(0.97 / 300, rules.CatchProbability(Quarry.Leviathan), 1e-15);
            Assert.AreEqual(26.75, rules.ExpectedMultiple(Quarry.Jellyfish), 1e-12);
            Assert.IsTrue(rules.IsMystery(Quarry.Jellyfish));
            Assert.IsFalse(rules.IsMystery(Quarry.Whale));
            Assert.AreEqual(500, rules.MaxMultiple(Quarry.Jellyfish));
        }

        [Test]
        public void Fire_CatchesBelowTheOddsAndMissesAtThem()
        {
            var rules = HarpoonRules.Standard;
            var (hits, _) = rules.CatchOdds(Quarry.Shark);
            var engine = new HarpoonEngine(rules, new Scripted(hits - 1, hits));

            var caught = engine.Fire(Bet(Quarry.Shark, 20));
            Assert.IsTrue(caught.Caught);
            Assert.AreEqual(50, caught.Multiple);
            Assert.AreEqual(1_000, caught.TotalReturn, "賭け金を含めて 50 倍");
            Assert.AreEqual(BetOutcome.Win, caught.Bets[0].Outcome);

            var missed = engine.Fire(Bet(Quarry.Shark, 20));
            Assert.IsFalse(missed.Caught);
            Assert.AreEqual(0, missed.TotalReturn);
            Assert.AreEqual(20, missed.TotalStake);
            Assert.AreEqual(2, engine.ShotNumber);
        }

        [Test]
        public void Fire_JellyfishDrawsItsMultipleByWeight()
        {
            // 重み 40, 30, 15, 10, 4, 1（合計 100）。99 は最後の 500 倍、40 は 2つ目の 10 倍
            var engine = new HarpoonEngine(HarpoonRules.Standard, new Scripted(0, 99, 0, 40, 0, 0));
            Assert.AreEqual(500, engine.Fire(Bet(Quarry.Jellyfish, 10)).Multiple);
            Assert.AreEqual(10, engine.Fire(Bet(Quarry.Jellyfish, 10)).Multiple);
            Assert.AreEqual(5, engine.Fire(Bet(Quarry.Jellyfish, 10)).Multiple);
        }

        [Test]
        public void Fire_RejectsInvalidBets()
        {
            var engine = new HarpoonEngine(HarpoonRules.Standard, new SeededRandom(1));
            var two = Bet(Quarry.Sardine, 10);
            two.Add(Quarry.Shark, 10);
            Assert.Throws<InvalidOperationException>(() => engine.Fire(two), "1発で2つは狙えない");
            Assert.Throws<InvalidOperationException>(() => engine.Fire(new BetBoard()));
            Assert.Throws<InvalidOperationException>(() => engine.Fire(Bet(Quarry.Sardine, 1)), "最低額未満");
            Assert.Throws<InvalidOperationException>(() => engine.Fire(Bet(Quarry.Sardine, 1_000)), "上限超え");
            Assert.Throws<InvalidOperationException>(() => engine.Fire(Bet(Quarry.Count, 10)));
            Assert.AreEqual(0, engine.ShotNumber);
        }

        [Test]
        public void Fire_LongRunReturnMatchesTheRules()
        {
            var engine = new HarpoonEngine(HarpoonRules.Standard, new SeededRandom(20261022));
            long staked = 0, returned = 0;
            for (int i = 0; i < 200_000; i++)
            {
                var result = engine.Fire(Bet(Quarry.Sardine, 10));
                staked += result.TotalStake;
                returned += result.TotalReturn;
            }
            Assert.AreEqual(0.97, returned / (double)staked, 0.01);
        }
    }
}
