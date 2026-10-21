using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// プレイアウトで期待値を見積もる戦略アドバイザー。画面の「ストラテジー」表示、
    /// 制限時間切れの自動進行、観戦モードのAIプレイヤーに使う。
    /// </summary>
    public sealed class PlayerAdvisor
    {
        /// <summary>同点のとき1手先評価の候補を優先させるための加点（試行1回分未満）。</summary>
        const double TieBreakBonus = 0.5;

        readonly IRandomSource random;
        readonly TableRules rules;
        readonly int familyPlayouts;
        readonly int movePlayouts;

        public PlayerAdvisor(IRandomSource random, TableRules rules, int familyPlayouts = 120, int movePlayouts = 60)
        {
            if (familyPlayouts <= 0)
                throw new ArgumentOutOfRangeException(nameof(familyPlayouts));
            if (movePlayouts < 0)
                throw new ArgumentOutOfRangeException(nameof(movePlayouts));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.familyPlayouts = familyPlayouts;
            this.movePlayouts = movePlayouts;
        }

        /// <summary>手札を見たうえで、各ラインの種類を選んだ場合の勝率と期待値を見積もる。</summary>
        public FamilyEstimate[] EstimateFamilies(int[] values, IReadOnlyList<int> hand)
        {
            var result = new FamilyEstimate[Lines.Families.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var family = Lines.Families[i];
                var tally = new OutcomeTally();
                for (int n = 0; n < familyPlayouts; n++)
                    tally.Add(Playout.Run(values, hand, family, true, random));
                result[i] = new FamilyEstimate(family, tally.WinRate, tally.ExpectedValue(rules.MainPays.Ratio));
            }
            return result;
        }

        /// <summary>期待値が最も高いラインの種類。</summary>
        public static LineFamily Best(IReadOnlyList<FamilyEstimate> estimates)
        {
            var best = estimates[0];
            foreach (var e in estimates)
            {
                if (e.ExpectedValue > best.ExpectedValue)
                    best = e;
            }
            return best.Family;
        }

        /// <summary>候補手それぞれをプレイアウトで評価し、最も勝ちやすい置き方を返す。</summary>
        public Placement SuggestPlacement(int[] values, IReadOnlyList<int> hand, LineFamily playerFamily)
        {
            var greedyPick = GreedyPolicy.ChoosePlacement(values, hand, playerFamily);
            if (movePlayouts == 0 || Scoring.EmptyCount(values) <= 1)
                return greedyPick;

            var best = greedyPick;
            double bestScore = double.MinValue;
            var v = (int[])values.Clone();
            var rest = new List<int>(hand.Count);
            for (int h = 0; h < hand.Count; h++)
            {
                if (GreedyPolicy.HasSameValueBefore(hand, h))
                    continue;
                rest.Clear();
                for (int k = 0; k < hand.Count; k++)
                {
                    if (k != h)
                        rest.Add(hand[k]);
                }
                for (int cell = 0; cell < v.Length; cell++)
                {
                    if (v[cell] != 0)
                        continue;
                    v[cell] = hand[h];
                    double score = h == greedyPick.HandIndex && cell == greedyPick.Cell ? TieBreakBonus : 0;
                    for (int n = 0; n < movePlayouts; n++)
                        score += Playout.Run(v, rest, playerFamily, false, random);
                    v[cell] = 0;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = new Placement(h, cell);
                    }
                }
            }
            return best;
        }
    }
}
