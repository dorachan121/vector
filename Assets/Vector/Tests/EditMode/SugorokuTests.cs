using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Sugoroku;

namespace Vector.Tests
{
    public class SugorokuTests
    {
        /// <summary>決めた目を順に返す乱数（Next(6) の値なので、目は値 + 1）。</summary>
        sealed class Scripted : IRandomSource
        {
            readonly Queue<int> faces;
            public Scripted(params int[] faces) => this.faces = new Queue<int>(faces);
            public int Next(int maxExclusive) => faces.Dequeue() - 1;
        }

        static BetBoard Bet(long amount)
        {
            var board = new BetBoard();
            board.Add(SugorokuRules.Main, amount);
            return board;
        }

        /// <summary>4マス目が小判、6マス目が凶、7マス目が橋、9マス目が落とし穴の小さな盤（10マスで上がり）。</summary>
        static SugorokuRules Small(int turns = 3) => new SugorokuRules("....C.OB.P", new long[] { 0, 50, 100, 300 }, turns);

        [Test]
        public void Land_AppliesTheSquareWhereThePieceStops()
        {
            var rules = Small();
            Assert.AreEqual((4, 2), rules.Land(0, 1, 4), "小判で段が上がる");
            Assert.AreEqual((4, 3), rules.Land(0, 3, 4), "段は上限で止まる");
            Assert.AreEqual((6, 0), rules.Land(4, 1, 2), "凶で段が下がる");
            Assert.AreEqual((6, 0), rules.Land(4, 0, 2), "段は 0 より下がらない");
            Assert.AreEqual((10, 1), rules.Land(4, 1, 3), "橋で 3 マス先へ（渡った先が上がり）");
            Assert.AreEqual((6, 1), rules.Land(8, 1, 1), "落とし穴で 3 マス戻る（戻った先の凶は効かない）");
            Assert.AreEqual((10, 2), rules.Land(8, 2, 6), "上がりを越えても上がり");
            Assert.AreEqual((5, 1), rules.Land(4, 1, 1), "何もないマス");
        }

        [Test]
        public void Engine_MovesByTheChosenDieAndPaysByTier()
        {
            var rules = Small();
            // 1回目 (4, 6) → 4 で小判（段 0→1）、2回目 (6, 3) → 3 で 7 の橋を渡って上がり
            var engine = new SugorokuEngine(rules, new Scripted(4, 6, 6, 3, 6, 1));
            engine.Start(Bet(100));
            Assert.AreEqual(SugorokuPhase.Choosing, engine.Phase);
            Assert.AreEqual((4, 6), engine.Dice);

            var first = engine.Choose(0);
            Assert.AreEqual(Square.Coin, first.Square);
            Assert.AreEqual(1, engine.Tier);
            Assert.AreEqual((6, 3), engine.Dice);

            engine.Choose(1);
            Assert.AreEqual(7 + 3, engine.Position, "3 で 7 の橋を渡って上がり");
            Assert.AreEqual(SugorokuPhase.Finished, engine.Phase);

            var result = engine.Settle();
            Assert.IsTrue(result.ReachedGoal);
            Assert.AreEqual(1, result.Tier);
            Assert.AreEqual(50, result.TotalReturn, "段 1 は 0.5 倍");
            Assert.AreEqual(2, result.Steps.Count);
        }

        [Test]
        public void Engine_LosesWhenTheTurnsRunOut()
        {
            var engine = new SugorokuEngine(Small(2), new Scripted(1, 1, 1, 2));
            engine.Start(Bet(100));
            engine.Choose(0);
            engine.Choose(1);
            Assert.AreEqual(SugorokuPhase.Finished, engine.Phase);
            var result = engine.Settle();
            Assert.IsFalse(result.ReachedGoal);
            Assert.AreEqual(0, result.TotalReturn);
            Assert.AreEqual(BetOutcome.Lose, result.Bets[0].Outcome);
            Assert.Throws<InvalidOperationException>(() => engine.Choose(0));
        }

        [Test]
        public void Solver_StandardReturnAndChoices()
        {
            var rules = SugorokuRules.Standard;
            var solver = SugorokuSolver.Solve(rules);
            Assert.AreEqual(0.964932, solver.Return, 1e-6);
            for (int a = 1; a <= 6; a++)
            {
                for (int b = 1; b <= 6; b++)
                {
                    int best = solver.Best(rules.Turns, 0, rules.StartTier, a, b);
                    double chosen = solver.ValueOf(rules.Turns, 0, rules.StartTier, best == 0 ? a : b);
                    double other = solver.ValueOf(rules.Turns, 0, rules.StartTier, best == 0 ? b : a);
                    Assert.That(chosen, Is.GreaterThanOrEqualTo(other));
                }
            }
            Assert.AreEqual(rules.PayFor(rules.MaxTier) / 100.0, solver.Value(1, rules.Goal, rules.MaxTier), 1e-12);
            Assert.AreEqual(0, solver.Value(0, 0, rules.StartTier), "回数を使い切ったら没収");
        }

        [Test]
        public void Engine_RejectsInvalidBetsAndChoices()
        {
            var engine = new SugorokuEngine(SugorokuRules.Standard, new SeededRandom(3));
            Assert.Throws<InvalidOperationException>(() => engine.Start(Bet(1)));
            Assert.Throws<InvalidOperationException>(() => engine.Choose(0), "賭ける前は選べない");
            engine.Start(Bet(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => engine.Choose(2));
        }
    }
}
