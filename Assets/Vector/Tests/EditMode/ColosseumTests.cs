using System;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Colosseum;

namespace Vector.Tests
{
    public class ColosseumTests
    {
        static BetBoard MainBet(long amount = 100)
        {
            var bets = new BetBoard();
            bets.Add(BattleRules.Main, amount);
            return bets;
        }

        /// <summary>決まった順番のカードを返す乱数（Card.FromId の番号。ランク r のスペードは r - 1）。</summary>
        sealed class Script : IRandomSource
        {
            readonly int[] ranks;
            int next;
            public Script(params int[] ranks) => this.ranks = ranks;
            public int Next(int maxExclusive) => (ranks[next++ % ranks.Length] - 1) % maxExclusive;
        }

        [Test]
        public void Solver_OptimalRtpMatchesPublishedValue()
        {
            var solver = BattleSolver.Solve(BattleRules.Standard.Setup);
            double rtp = solver.WinProbability * (1 + BattleRules.Standard.WinPays.Ratio);
            Assert.That(rtp, Is.InRange(0.964, 0.970), "README の 96.71% から変わったら体力を見直す");
        }

        [Test]
        public void Solver_FinishesWhenStrikeCannotFail()
        {
            var solver = BattleSolver.Solve(BattleRules.Standard.Setup);
            // 獣の体力1なら、どのカードで斬っても倒せる
            Assert.AreEqual(1.0, solver.ValueOf(BattleAction.Strike, 1, 1, 13), 1e-12);
            Assert.AreEqual(BattleAction.Strike, solver.Best(1, 1, 13));
            // 体力が十分で予告が K なら受ける方が良い
            Assert.AreEqual(BattleAction.Guard, solver.Best(14, 42, 13));
            Assert.That(solver.WinProbabilityAt(30, 1), Is.EqualTo(1.0).Within(1e-12));
            Assert.AreEqual(0.0, solver.WinProbabilityAt(0, 10));
        }

        [Test]
        public void Engine_StrikeThatKillsAvoidsCounterAttack()
        {
            // 予告 K（13）、プレイヤーは K を引いて会心の 26。体力 20 の獣は倒れ、一撃は受けない
            var rules = new BattleRules(new BattleSetup(10, 20));
            var engine = new BattleEngine(rules, new Script(13, 13));
            engine.Deal(MainBet());
            Assert.AreEqual(13, engine.IntentDamage);
            var exchange = engine.Act(BattleAction.Strike);
            Assert.IsTrue(exchange.Critical);
            Assert.AreEqual(26, exchange.DamageToBeast);
            Assert.AreEqual(0, exchange.DamageToPlayer);
            Assert.AreEqual(BattlePhase.Showdown, engine.Phase);
            var result = engine.Settle();
            Assert.AreEqual(RoundWinner.Player, result.Winner);
            Assert.AreEqual(200, result.TotalReturn);
        }

        [Test]
        public void Engine_GuardCountersBlocksAndClashes()
        {
            var engine = new BattleEngine(BattleRules.Standard, new Script(5, 9, 7, 3, 4, 4, 1));
            engine.Deal(MainBet());
            var counter = engine.Act(BattleAction.Guard);
            Assert.AreEqual(4, counter.DamageToBeast, "9 で 5 を受けて 4 の反撃");
            Assert.AreEqual(0, counter.DamageToPlayer);
            var blocked = engine.Act(BattleAction.Guard);
            Assert.AreEqual(4, blocked.DamageToPlayer, "3 で 7 を受けて 4 のダメージ");
            var clash = engine.Act(BattleAction.Guard);
            Assert.IsTrue(clash.Clash);
            Assert.AreEqual(26, engine.PlayerHp);
            Assert.AreEqual(38, engine.BeastHp);
            Assert.AreEqual(3, engine.Turn);
        }

        [Test]
        public void Engine_LosesWhenBeastSurvivesAndHitsLast()
        {
            var engine = new BattleEngine(new BattleRules(new BattleSetup(5, 40)), new Script(13, 2));
            engine.Deal(MainBet());
            var exchange = engine.Act(BattleAction.Strike);
            Assert.IsTrue(exchange.PlayerDefeated);
            var result = engine.Settle();
            Assert.AreEqual(RoundWinner.Dealer, result.Winner);
            Assert.AreEqual(0, result.TotalReturn);
            Assert.AreEqual(0, result.PlayerHp);
        }

        [Test]
        public void Setup_RageAddsDamageAtOneThird()
        {
            var setup = new BattleSetup(30, 30, 3);
            Assert.IsFalse(setup.IsEnraged(11));
            Assert.IsTrue(setup.IsEnraged(10));
            Assert.AreEqual(8, setup.IntentDamage(5, 10));
            Assert.AreEqual(5, setup.IntentDamage(5, 11));
        }

        [Test]
        public void Engine_RejectsActionsOutOfTurn()
        {
            var engine = new BattleEngine(BattleRules.Standard, new SeededRandom(2));
            Assert.Throws<InvalidOperationException>(() => engine.Act(BattleAction.Strike));
            Assert.Throws<InvalidOperationException>(() => engine.Settle());
            engine.Deal(MainBet());
            Assert.Throws<InvalidOperationException>(() => engine.Settle());
            Assert.Throws<InvalidOperationException>(() => engine.BeginBetting());
            engine.Abort();
            Assert.AreEqual(BattlePhase.Complete, engine.Phase);
            engine.BeginBetting();
        }
    }
}
