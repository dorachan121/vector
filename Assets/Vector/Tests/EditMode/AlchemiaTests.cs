using System;
using System.Linq;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Alchemia;

namespace Vector.Tests
{
    public class AlchemiaTests
    {
        static readonly AlchemiaRules Rules = AlchemiaRules.Standard;

        /// <summary>補充は常に「光の素」（Next が常に 4）。</summary>
        sealed class AlwaysLight : IRandomSource
        {
            public int Next(int maxExclusive) => (int)Element.Light % maxExclusive;
        }

        /// <summary>
        /// 文字列から盤面を作る。1マス2文字（元素 F/W/A/E/L と位 1〜5、"--" は空き）、6列×5行。
        /// 同じ元素が並ばないように、使わないマスは火と水を市松に置いて埋める想定で書く。
        /// </summary>
        static Symbol[] Grid(params string[] rows)
        {
            var grid = new Symbol[Rules.Cells];
            for (int r = 0; r < rows.Length; r++)
            {
                var cells = rows[r].Split(' ');
                for (int c = 0; c < cells.Length; c++)
                {
                    if (cells[c] == "--")
                        continue;
                    var element = (Element)"FWAEL".IndexOf(cells[c][0]);
                    grid[r * Rules.Columns + c] = new Symbol(element, cells[c][1] - '0');
                }
            }
            return grid;
        }

        // 塊にならない背景（水と風と土の市松）
        const string Bg0 = "W1 A1 E1 W1 A1 E1";
        const string Bg1 = "A1 E1 W1 A1 E1 W1";

        [Test]
        public void FindClusters_NeedsThreeConnectedOfOneElement()
        {
            var engine = new AlchemiaEngine(Rules, new AlwaysLight());
            var grid = Grid(Bg0, Bg1, "F1 F1 W1 A1 E1 W1", "F1 E1 A1 F1 W1 E1", Bg1);
            var clusters = engine.FindClusters(grid);
            Assert.AreEqual(1, clusters.Count);
            CollectionAssert.AreEquivalent(new[] { 12, 13, 18 }, clusters[0]);
            Assert.AreEqual(0, engine.FindClusters(Grid(Bg0, Bg1, Bg0, Bg1, Bg0)).Count);
        }

        [Test]
        public void Merge_GrowsAtHighestTierCellAndPaysFromTierThree()
        {
            var engine = new AlchemiaEngine(Rules, new AlwaysLight());
            // 下段に 火2 火1 火1: 火2 の位置に 火3 ができ、位3なので 1倍 × 連鎖倍率1
            var grid = Grid(Bg0, Bg1, Bg0, Bg1, "F1 F2 F1 A1 E1 W1");
            var step = engine.Step(grid, 0);
            Assert.AreEqual(1, step.Merges.Count);
            var merge = step.Merges[0];
            Assert.AreEqual(25, merge.Anchor);
            Assert.AreEqual(3, merge.NewTier);
            Assert.AreEqual(Rules.CreationPay(3), merge.PayHundredths);
            Assert.AreEqual(new Symbol(Element.Fire, 3), step.AfterMerge[25]);
            Assert.IsTrue(step.AfterMerge[24].IsEmpty && step.AfterMerge[26].IsEmpty);
        }

        [Test]
        public void Merge_TieBreaksToBottomThenLeftAndScalesWithSize()
        {
            var engine = new AlchemiaEngine(Rules, new AlwaysLight());
            // 5個の 火1 の塊（縦3 + 下段2）。最も下の行の左のマス (24) に 火2。位2は配当なし
            var grid = Grid(Bg0, Bg1, "F1 E1 W1 A1 E1 W1", "F1 W1 A1 E1 W1 A1", "F1 F1 F1 E1 A1 W1");
            var step = engine.Step(grid, 3);
            Assert.AreEqual(24, step.Merges[0].Anchor);
            Assert.AreEqual(2, step.Merges[0].NewTier);
            Assert.AreEqual(0, step.Merges[0].PayHundredths);
            Assert.AreEqual(Rules.MultiplierFor(3), step.Multiplier);
            Assert.AreEqual(Rules.CreationPay(4) * 2, Rules.SizeScaled(Rules.CreationPay(4), 5), "5個は2倍");
        }

        [Test]
        public void Gravity_DropsSymbolsAndRefillsFromTop()
        {
            var engine = new AlchemiaEngine(Rules, new AlwaysLight());
            var grid = Grid(Bg0, Bg1, Bg0, "F1 E1 W1 A1 E1 W1", "F1 F1 W1 A1 E1 A1");
            var step = engine.Step(grid, 0);
            // 塊 {18, 24, 25} → 24 に 火2 が残り、18 と 25 が空く。0列目は1マス、1列目も1マス落ちる
            Assert.AreEqual(2, step.Spawned.Count);
            Assert.That(step.Falls.All(f => f.From % Rules.Columns == f.To % Rules.Columns && f.To > f.From));
            Assert.AreEqual(new Symbol(Element.Fire, 2), step.After[24]);
            Assert.AreEqual(Element.Light, step.After[0].Element);
            Assert.AreEqual(Element.Light, step.After[1].Element);
            Assert.IsFalse(step.After.Any(s => s.IsEmpty));
        }

        [Test]
        public void Transmutation_DissolvesTheWholeElement()
        {
            var engine = new AlchemiaEngine(Rules, new AlwaysLight());
            var grid = Grid("F1 W1 A1 E1 W1 A1", Bg1, Bg0, "W1 E1 W1 A1 E1 W1", "F5 F1 F1 A1 E1 W1");
            var step = engine.Step(grid, 0);
            var transmutation = step.Merges.Single(m => m.Element == Element.Fire);
            Assert.IsTrue(transmutation.IsTransmutation);
            Assert.AreEqual(Rules.TransmutationPay, transmutation.PayHundredths);
            CollectionAssert.Contains(step.Dissolved, 0, "離れた火も消える");
            Assert.IsFalse(step.AfterMerge.Any(s => !s.IsEmpty && s.Element == Element.Fire));
        }

        [Test]
        public void Spin_PaysFlooredMultipleOfBetAndRecordsSteps()
        {
            var engine = new AlchemiaEngine(Rules, new SeededRandom(42));
            var bets = new BetBoard();
            bets.Add(AlchemiaRules.Spin, 50);
            for (int i = 0; i < 200; i++)
            {
                var result = engine.Spin(bets);
                long pay = result.Steps.Sum(s => s.PayHundredths);
                Assert.AreEqual(Math.Min(pay, Rules.MaxWinMultiple * 100), result.PayHundredths);
                long expected = 50 * result.PayHundredths / 100;
                Assert.AreEqual(expected, result.TotalReturn);
                Assert.AreEqual(50, result.TotalStake);
                if (result.Steps.Count > 0)
                    Assert.AreEqual(0, engine.FindClusters(result.Steps[result.Steps.Count - 1].After).Count, "連鎖は止まるまで続く");
            }
            Assert.AreEqual(200, engine.SpinNumber);
        }

        [Test]
        public void Spin_RejectsBetsOutsideLimits()
        {
            var engine = new AlchemiaEngine(Rules, new SeededRandom(1));
            var bets = new BetBoard();
            bets.Add(AlchemiaRules.Spin, 5);
            Assert.Throws<InvalidOperationException>(() => engine.Spin(bets));
        }
    }
}
