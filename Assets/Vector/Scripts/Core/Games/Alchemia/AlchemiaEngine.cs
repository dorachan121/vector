using System;
using System.Collections.Generic;

namespace Vector.Core.Alchemia
{
    /// <summary>
    /// ALCHEMIA の1回のスピン。盤面を埋め、同じ元素が縦横に3つ以上つながった塊を融合させ、
    /// 落下と補充をくり返して連鎖が止まるまでを一度に計算する（結果はスピンの瞬間に確定し、演出はそれをなぞるだけ）。
    /// <para>
    /// 融合: 塊は1つのシンボルになり、塊の中で最も位の高いマスに「最高位 + 1」の位で残る（同じ位なら下、さらに左のマス）。
    /// 賢者の石（位5）を含む塊は大錬成となり、盤面のその元素がすべて消える。
    /// </para>
    /// </summary>
    public sealed class AlchemiaEngine
    {
        readonly AlchemiaRules rules;
        readonly IRandomSource random;

        public AlchemiaEngine(AlchemiaRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public AlchemiaRules Rules => rules;

        public int SpinNumber { get; private set; }

        public SpinResult Spin(BetBoard bets)
        {
            WagerCheck.ThrowIfInvalid(rules, bets);
            SpinNumber++;

            var grid = new Symbol[rules.Cells];
            for (int i = 0; i < grid.Length; i++)
                grid[i] = SpawnSymbol();
            var initial = (Symbol[])grid.Clone();

            var steps = new List<CascadeStep>();
            long total = 0;
            long cap = rules.MaxWinMultiple * 100;
            bool capped = false;
            for (int step = 0; step < rules.MaxCascades; step++)
            {
                var cascade = Step(grid, step);
                if (cascade == null)
                    break;
                steps.Add(cascade);
                total += cascade.PayHundredths;
                if (total >= cap)
                {
                    total = cap;
                    capped = true;
                    break;
                }
            }

            // スロットの慣例どおり、配当の倍率は賭け金を含む払い戻しの倍率（x1.00 なら賭け金がそのまま戻る）
            var results = new List<SpotResult>();
            foreach (var bet in bets.NonZero())
            {
                long paid = checked(bet.Value * total) / 100;
                results.Add(paid > 0
                    ? new SpotResult(bet.Key, bet.Value, BetOutcome.Win, paid)
                    : SpotResult.Lose(bet.Key, bet.Value));
            }
            return new SpinResult(initial, steps, total, capped, results);
        }

        /// <summary>盤面に連鎖の1段（融合 → 大錬成 → 落下 → 補充）を適用する。塊が無ければ何もせず null。</summary>
        public CascadeStep Step(Symbol[] grid, int step)
        {
            if (grid == null || grid.Length != rules.Cells)
                throw new ArgumentException("盤面のマスの数が違います。", nameof(grid));
            var clusters = FindClusters(grid);
            return clusters.Count == 0 ? null : Resolve(grid, clusters, rules.MultiplierFor(step));
        }

        /// <summary>同じ元素が縦横につながった、MinCluster 個以上の塊。</summary>
        public List<List<int>> FindClusters(Symbol[] grid)
        {
            var clusters = new List<List<int>>();
            var seen = new bool[grid.Length];
            var stack = new Stack<int>();
            for (int i = 0; i < grid.Length; i++)
            {
                if (seen[i] || grid[i].IsEmpty)
                    continue;
                var cluster = new List<int>();
                seen[i] = true;
                stack.Push(i);
                while (stack.Count > 0)
                {
                    int cell = stack.Pop();
                    cluster.Add(cell);
                    foreach (int n in Neighbors(cell))
                    {
                        if (!seen[n] && !grid[n].IsEmpty && grid[n].Element == grid[i].Element)
                        {
                            seen[n] = true;
                            stack.Push(n);
                        }
                    }
                }
                if (cluster.Count >= rules.MinCluster)
                {
                    cluster.Sort();
                    clusters.Add(cluster);
                }
            }
            return clusters;
        }

        CascadeStep Resolve(Symbol[] grid, List<List<int>> clusters, int multiplier)
        {
            var merges = new List<Merge>();
            var transmuted = new HashSet<Element>();
            foreach (var cluster in clusters)
            {
                var element = grid[cluster[0]].Element;
                int anchor = AnchorOf(grid, cluster);
                int maxTier = grid[anchor].Tier;
                foreach (int cell in cluster)
                    grid[cell] = Symbol.Empty;
                long pay;
                int newTier;
                if (maxTier >= Symbol.MaxTier)
                {
                    newTier = 0;
                    pay = rules.TransmutationPay * multiplier;
                    transmuted.Add(element);
                }
                else
                {
                    newTier = maxTier + 1;
                    grid[anchor] = new Symbol(element, newTier);
                    pay = rules.SizeScaled(rules.CreationPay(newTier), cluster.Count) * multiplier;
                }
                merges.Add(new Merge(element, cluster, anchor, newTier, pay));
            }

            var dissolved = new List<int>();
            if (transmuted.Count > 0)
            {
                for (int i = 0; i < grid.Length; i++)
                {
                    if (!grid[i].IsEmpty && transmuted.Contains(grid[i].Element))
                    {
                        grid[i] = Symbol.Empty;
                        dissolved.Add(i);
                    }
                }
            }
            var afterMerge = (Symbol[])grid.Clone();

            var falls = new List<Fall>();
            var spawned = new List<int>();
            for (int col = 0; col < rules.Columns; col++)
            {
                int write = rules.Rows - 1;
                for (int row = rules.Rows - 1; row >= 0; row--)
                {
                    int from = row * rules.Columns + col;
                    if (grid[from].IsEmpty)
                        continue;
                    int to = write * rules.Columns + col;
                    if (to != from)
                    {
                        grid[to] = grid[from];
                        grid[from] = Symbol.Empty;
                        falls.Add(new Fall(from, to));
                    }
                    write--;
                }
                for (int row = write; row >= 0; row--)
                {
                    int cell = row * rules.Columns + col;
                    grid[cell] = SpawnSymbol();
                    spawned.Add(cell);
                }
            }
            return new CascadeStep(multiplier, merges, dissolved, afterMerge, falls, spawned, (Symbol[])grid.Clone());
        }

        /// <summary>塊の中で最も位の高いマス。同じ位なら下の行、さらに左の列。</summary>
        int AnchorOf(Symbol[] grid, List<int> cluster)
        {
            int best = cluster[0];
            foreach (int cell in cluster)
            {
                int tier = grid[cell].Tier, bestTier = grid[best].Tier;
                if (tier > bestTier)
                {
                    best = cell;
                    continue;
                }
                if (tier < bestTier)
                    continue;
                int row = cell / rules.Columns, bestRow = best / rules.Columns;
                if (row > bestRow || (row == bestRow && cell % rules.Columns < best % rules.Columns))
                    best = cell;
            }
            return best;
        }

        IEnumerable<int> Neighbors(int cell)
        {
            int row = cell / rules.Columns, col = cell % rules.Columns;
            if (col > 0) yield return cell - 1;
            if (col < rules.Columns - 1) yield return cell + 1;
            if (row > 0) yield return cell - rules.Columns;
            if (row < rules.Rows - 1) yield return cell + rules.Columns;
        }

        Symbol SpawnSymbol() => new Symbol((Element)random.Next(rules.ElementCount), 1);
    }
}
