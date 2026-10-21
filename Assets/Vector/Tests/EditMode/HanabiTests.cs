using System;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Hanabi;

namespace Vector.Tests
{
    public class HanabiTests
    {
        /// <summary>いつも同じ値を返す乱数（暴発点を決め打ちする）。</summary>
        sealed class Fixed : IRandomSource
        {
            readonly int value;
            public Fixed(int value) => this.value = value;
            public int Next(int maxExclusive) => value;
        }

        static BetBoard Bet(long amount)
        {
            var board = new BetBoard();
            board.Add(HanabiRules.Main, amount);
            return board;
        }

        [Test]
        public void Rules_CrashPointsMatchTheCounts()
        {
            var rules = HanabiRules.Standard;
            Assert.AreEqual(rules.MaxHundredths, rules.CrashFor(0), "最も小さい乱数は上限で止まる");
            Assert.AreEqual(200, rules.CrashFor(484_999), "97,000,000 / 485,000 = 200");
            Assert.AreEqual(100, rules.CrashFor(HanabiRules.Resolution - 1), "大きい乱数はその場で弾ける");
            // 数え上げと一致する（いくつかの倍率で全ての乱数を確かめる）
            foreach (int m in new[] { 101, 150, 200, 777 })
            {
                long count = 0;
                for (int r = 0; r < HanabiRules.Resolution; r++)
                {
                    if (rules.CrashFor(r) >= m)
                        count++;
                }
                Assert.AreEqual(rules.SurvivingDraws(m), count, $"x{m / 100.0}");
            }
        }

        [Test]
        public void Rules_NoTargetBeatsTheHouse()
        {
            var rules = HanabiRules.Standard;
            for (int m = rules.MinCashOut; m <= rules.MaxHundredths; m += 7)
                Assert.That(rules.ReturnAt(m), Is.LessThanOrEqualTo(0.97 + 1e-12), $"x{m / 100.0}");
            Assert.AreEqual(0.97, rules.ReturnAt(200), 1e-6);
            Assert.AreEqual(0.0396, rules.InstantBurst, 1e-4, "1.01 倍に届かない確率");
        }

        [Test]
        public void Engine_CashOutBeforeTheBurstPays()
        {
            // 乱数 484,999 → 暴発点 2.00 倍
            var engine = new HanabiEngine(HanabiRules.Standard, new Fixed(484_999));
            engine.Launch(Bet(100));
            Assert.IsFalse(engine.HasBurstAt(200), "暴発点ちょうどまでは昇る");
            Assert.IsTrue(engine.HasBurstAt(201));
            Assert.IsTrue(engine.CashOut(155));
            var result = engine.Settle();
            Assert.IsTrue(result.Opened);
            Assert.AreEqual(155, result.TotalReturn);
            Assert.AreEqual(200, result.CrashHundredths);
        }

        [Test]
        public void Engine_TooLateOrBurstLoses()
        {
            var late = new HanabiEngine(HanabiRules.Standard, new Fixed(484_999));
            late.Launch(Bet(100));
            Assert.IsFalse(late.CashOut(201), "暴発点を越えたら開けない");
            Assert.AreEqual(0, late.Settle().TotalReturn);

            var burst = new HanabiEngine(HanabiRules.Standard, new Fixed(484_999));
            burst.Launch(Bet(100));
            burst.Burst();
            var result = burst.Settle();
            Assert.IsFalse(result.Opened);
            Assert.AreEqual(BetOutcome.Lose, result.Bets[0].Outcome);
            Assert.Throws<InvalidOperationException>(() => burst.CashOut(150));
        }

        [Test]
        public void Engine_RejectsInvalidCashOutAndBets()
        {
            var engine = new HanabiEngine(HanabiRules.Standard, new Fixed(0));
            Assert.Throws<InvalidOperationException>(() => engine.Launch(Bet(1)));
            engine.Launch(Bet(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => engine.CashOut(100));
            Assert.Throws<InvalidOperationException>(() => engine.BeginBetting());
        }
    }
}
