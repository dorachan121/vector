using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Orbit;

namespace Vector.Tests
{
    public class OrbitTests
    {
        /// <summary>決めた値を順に返す乱数（彗星の抽選 → 内・中・外の位置の順に引かれる）。</summary>
        sealed class Scripted : IRandomSource
        {
            readonly Queue<int> values;
            public Scripted(params int[] values) => this.values = new Queue<int>(values);
            public int Next(int maxExclusive) => values.Dequeue();
        }

        static BetBoard Bets(params (int spot, long amount)[] bets)
        {
            var board = new BetBoard();
            foreach (var (spot, amount) in bets)
                board.Add(spot, amount);
            return board;
        }

        [Test]
        public void Rules_EveryBetHasTheSameEdge()
        {
            var rules = OrbitRules.Standard;
            for (int spot = 0; spot < OrbitSpots.Count; spot++)
                Assert.AreEqual(36.0 / 37.0, rules.ReturnOf(spot), 1e-12, $"spot {spot}");
            Assert.AreEqual(1.0 / 37.0, rules.ProbabilityOf(OrbitSpots.Comet), 1e-15);
            Assert.AreEqual(36.0 / 37.0 / 288.0, rules.ProbabilityOf(OrbitSpots.Alignment), 1e-15);
            Assert.AreEqual(287, rules.PaysFor(OrbitSpots.Alignment).Win);
            Assert.AreEqual(35, rules.PaysFor(OrbitSpots.Comet).Win);
        }

        [Test]
        public void Spin_QuadrantsAndHalvesFollowTheCompass()
        {
            // 外惑星の位置 0 は北から時計回りに 7.5°（北東・天）
            Assert.AreEqual(0, OrbitSpin.Planets(0, 0, 0).Quadrant);
            Assert.AreEqual(0, OrbitSpin.Planets(0, 0, 0).Half);
            Assert.AreEqual(0, OrbitSpin.Planets(0, 0, 5).Quadrant);
            Assert.AreEqual(1, OrbitSpin.Planets(0, 0, 6).Quadrant, "東を過ぎると南東");
            Assert.AreEqual(1, OrbitSpin.Planets(0, 0, 6).Half, "東より下は地");
            Assert.AreEqual(2, OrbitSpin.Planets(0, 0, 17).Quadrant);
            Assert.AreEqual(1, OrbitSpin.Planets(0, 0, 17).Half);
            Assert.AreEqual(3, OrbitSpin.Planets(0, 0, 18).Quadrant, "西を過ぎると北西");
            Assert.AreEqual(0, OrbitSpin.Planets(0, 0, 18).Half, "西より上は天");
        }

        [Test]
        public void Spin_ConjunctionsAndAlignment()
        {
            var aligned = OrbitSpin.Planets(1, 2, 4);
            Assert.IsTrue(aligned.InnerConjunction && aligned.OuterConjunction && aligned.Alignment);
            Assert.IsTrue(aligned.Wins(OrbitSpots.Alignment));

            var inner = OrbitSpin.Planets(1, 2, 5);
            Assert.IsTrue(inner.InnerConjunction);
            Assert.IsFalse(inner.OuterConjunction);
            Assert.IsFalse(inner.Wins(OrbitSpots.Alignment));

            var outer = OrbitSpin.Planets(0, 3, 6);
            Assert.IsFalse(outer.InnerConjunction);
            Assert.IsTrue(outer.Wins(OrbitSpots.OuterConjunction));
            Assert.IsTrue(outer.Wins(OrbitSpots.Inner(0)));
            Assert.IsFalse(outer.Wins(OrbitSpots.Comet));
            Assert.Throws<ArgumentOutOfRangeException>(() => OrbitSpin.Planets(6, 0, 0));
        }

        [Test]
        public void Spin_CometHidesThePlanets()
        {
            var comet = OrbitSpin.CometPass;
            for (int spot = 0; spot < OrbitSpots.Count; spot++)
                Assert.AreEqual(spot == OrbitSpots.Comet, comet.Wins(spot), $"spot {spot}");
            Assert.IsFalse(comet.Alignment);
        }

        [Test]
        public void Engine_PaysTrueOddsWhenThePlanetsStop()
        {
            // 彗星なし（0 以外）、内 1・中 2・外 4 で惑星直列
            var engine = new OrbitEngine(OrbitRules.Standard, new Scripted(5, 1, 2, 4));
            var spin = engine.Spin(Bets((OrbitSpots.Alignment, 10), (OrbitSpots.Inner(1), 100), (OrbitSpots.Half(1), 50), (OrbitSpots.Comet, 20)));
            Assert.IsTrue(spin.Alignment);
            Assert.AreEqual(OrbitPhase.Spun, engine.Phase);
            var result = engine.Settle();
            Assert.AreEqual(180, result.TotalStake);
            // 直列 10×288 + 内惑星 100×6 （天地は天なので地は負け、彗星も負け）
            Assert.AreEqual(2880 + 600, result.TotalReturn);
            Assert.AreEqual(OrbitPhase.Complete, engine.Phase);
            engine.BeginBetting();
            Assert.AreEqual(OrbitPhase.Betting, engine.Phase);
        }

        [Test]
        public void Engine_CometTakesEverythingButTheCometBet()
        {
            var engine = new OrbitEngine(OrbitRules.Standard, new Scripted(0));
            var spin = engine.Spin(Bets((OrbitSpots.Half(0), 100), (OrbitSpots.Comet, 10)));
            Assert.IsTrue(spin.Comet);
            var result = engine.Settle();
            Assert.AreEqual(360, result.TotalReturn, "彗星 10×36 だけが戻る");
        }

        [Test]
        public void Engine_RejectsInvalidBetsAndOutOfOrderCalls()
        {
            var engine = new OrbitEngine(OrbitRules.Standard, new Scripted(1, 0, 0, 0));
            Assert.Throws<InvalidOperationException>(() => engine.Settle());
            Assert.Throws<InvalidOperationException>(() => engine.Spin(Bets((OrbitSpots.Alignment, 100))), "直列の上限は $50");
            Assert.Throws<InvalidOperationException>(() => engine.Spin(Bets((OrbitSpots.Half(0), 1))), "下限は $5");
            engine.Spin(Bets((OrbitSpots.Half(0), 100)));
            Assert.Throws<InvalidOperationException>(() => engine.Spin(Bets((OrbitSpots.Half(0), 100))));
            engine.Abort();
            Assert.AreEqual(OrbitPhase.Complete, engine.Phase);
        }
    }
}
