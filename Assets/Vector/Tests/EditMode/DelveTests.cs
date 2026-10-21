using System;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Delve;

namespace Vector.Tests
{
    public class DelveTests
    {
        static BetBoard MainBet(long amount = 100)
        {
            var bets = new BetBoard();
            bets.Add(DelveRules.Main, amount);
            return bets;
        }

        /// <summary>魔物を常に「残っている中で最初の扉」に置く乱数（Next が常に 0）。</summary>
        sealed class FirstDoorMonster : IRandomSource
        {
            public int Next(int maxExclusive) => 0;
        }

        [Test]
        public void Rules_EveryCashOutDepthHasTheSameEdge()
        {
            var rules = DelveRules.Standard;
            for (int d = 1; d <= rules.Depths; d++)
                Assert.That(rules.ReturnIfCashingAt(d), Is.InRange(0.965, rules.TargetReturn + 1e-12), $"depth {d}");
            Assert.AreEqual(129, rules.MultiplierHundredths(1));
            Assert.That(rules.MultiplierHundredths(rules.Depths), Is.GreaterThan(200_000));
            Assert.Throws<ArgumentOutOfRangeException>(() => rules.MultiplierHundredths(0));
        }

        [Test]
        public void Engine_SurvivesSafeDoorsAndCashesOut()
        {
            var engine = new DelveEngine(DelveRules.Standard, new FirstDoorMonster());
            engine.Deal(MainBet());
            Assert.IsFalse(engine.CanCashOut, "1つも抜けていなければ引き返せない");
            Assert.Throws<InvalidOperationException>(() => engine.CashOut());
            var reveal = engine.Open(3);
            Assert.IsTrue(reveal.Survived);
            Assert.IsTrue(reveal.Monsters[0], "魔物は扉0");
            Assert.AreEqual(1, engine.ClearedDepth);
            engine.Open(1);
            engine.CashOut();
            Assert.AreEqual(DelvePhase.Finished, engine.Phase);
            var result = engine.Settle();
            Assert.AreEqual(2, result.ClearedDepth);
            Assert.IsFalse(result.Busted);
            Assert.AreEqual(172, result.TotalReturn, "100 × 1.72");
        }

        [Test]
        public void Engine_MonsterLosesEverything()
        {
            var engine = new DelveEngine(DelveRules.Standard, new FirstDoorMonster());
            engine.Deal(MainBet());
            engine.Open(2);
            var reveal = engine.Open(0);
            Assert.IsFalse(reveal.Survived);
            Assert.AreEqual(DelvePhase.Finished, engine.Phase);
            Assert.Throws<InvalidOperationException>(() => engine.Open(1));
            var result = engine.Settle();
            Assert.IsTrue(result.Busted);
            Assert.AreEqual(0, result.TotalReturn);
            Assert.AreEqual(1, result.ClearedDepth);
        }

        [Test]
        public void Engine_DeepestFloorEndsTheDive()
        {
            var rules = DelveRules.Standard;
            var engine = new DelveEngine(rules, new FirstDoorMonster());
            engine.Deal(MainBet(10));
            for (int d = 0; d < rules.Depths; d++)
                engine.Open(rules.Floors[d].Doors - 1);
            Assert.AreEqual(DelvePhase.Finished, engine.Phase);
            var result = engine.Settle();
            Assert.AreEqual(rules.Depths, result.ClearedDepth);
            Assert.AreEqual(10L * rules.MultiplierHundredths(rules.Depths) / 100, result.TotalReturn);
        }

        [Test]
        public void Engine_PlacesExactlyTheFloorsMonsters()
        {
            var rules = DelveRules.Standard;
            var engine = new DelveEngine(rules, new SeededRandom(5));
            for (int round = 0; round < 50; round++)
            {
                engine.BeginBetting();
                engine.Deal(MainBet());
                while (engine.Phase == DelvePhase.Choosing)
                {
                    var floor = rules.Floors[engine.ClearedDepth];
                    var reveal = engine.Open(0);
                    int monsters = 0;
                    foreach (bool m in reveal.Monsters)
                        monsters += m ? 1 : 0;
                    Assert.AreEqual(floor.Monsters, monsters);
                    Assert.AreEqual(floor.Doors, reveal.Monsters.Count);
                }
                engine.Settle();
            }
        }
    }
}
