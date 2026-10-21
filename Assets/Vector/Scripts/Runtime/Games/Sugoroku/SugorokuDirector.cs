using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Core.Sugoroku;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Sugoroku
{
    /// <summary>
    /// SUGOROKU の進行役。ベット受付 → 振出から「2つの目を振る → どちらで進むか選ぶ」を上がるか回数を使い切るまで → 精算 を繰り返す。
    /// 戦略ヒントが ON なら、厳密な最善手（<see cref="SugorokuSolver"/>）で勧める目を光らせる。
    /// </summary>
    public sealed class SugorokuDirector : CasinoDirector
    {
        const float ResultSeconds = 2.2f;

        /// <summary>この段以上で上がったら「大当たりの旅」（クエストの条件）。</summary>
        const int HighTier = 5;

        SugorokuRules rules;
        SugorokuEngine engine;
        SugorokuSolver solver;
        SugorokuView view;
        int chosen = -1;
        bool choosing;

        public void Begin(SugorokuRules rules, SugorokuEngine engine, SugorokuSolver solver, SugorokuView view, WagerSession session, ITableWallet wallet)
        {
            this.rules = rules;
            this.engine = engine;
            this.solver = solver;
            this.view = view;
            view.Chosen += index =>
            {
                if (choosing)
                    chosen = index;
            };
            Begin(view.Casino, session, wallet);
        }

        protected override GameId Game => GameId.Sugoroku;
        protected override float BettingSeconds => rules.BettingSeconds;
        protected override long TableMinimum => rules.MinBet;
        protected override string BettingHint => "チップを置いて START（振出から上りを目指します）";
        protected override string SpotName(int spot) => "BET";
        protected override IReadOnlyList<(string title, string body)> HelpPages => SugorokuHelp.Pages(rules, solver);

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (SugorokuResult)outcome;
            var tags = new List<string>();
            if (result.ReachedGoal)
                tags.Add(RoundTags.SugorokuGoal);
            if (result.ReachedGoal && result.Tier >= HighTier)
                tags.Add(RoundTags.SugorokuHighTier);
            foreach (var step in result.Steps)
            {
                if (step.Square == Square.Treasure)
                {
                    tags.Add(RoundTags.SugorokuTreasure);
                    break;
                }
            }
            return tags;
        }

        void Update()
        {
            if (!choosing || Dialog.IsAnyOpen)
                return;
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.LeftArrow))
                chosen = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.RightArrow))
                chosen = 1;
        }

        protected override IEnumerator BeforeBetting()
        {
            if (engine.Phase == SugorokuPhase.Complete)
                engine.BeginBetting();
            view.ResetJourney();
            View.Hud.SetInfo($"JOURNEY {engine.RoundNumber + 1}");
            yield break;
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            engine.Start(inPlay);
            while (engine.Phase == SugorokuPhase.Choosing)
            {
                view.SetTurns(engine.TurnsLeft);
                View.Status.Show("ROLL", $"残り {engine.TurnsLeft} 回");
                var (first, second) = engine.Dice;
                yield return view.Roll(first, second);

                int hint = ShowStrategy ? solver.Best(engine.TurnsLeft, engine.Position, engine.Tier, first, second) : -1;
                view.ShowChoices(engine.Position, first, second, hint, ShowStrategy ? Advice(first, second) : "");
                View.Status.Show("CHOOSE A DIE", "どちらか1つの目だけ進みます（盤の数字が止まるマス）");
                chosen = -1;
                choosing = true;
                while (chosen < 0)
                    yield return null;
                choosing = false;

                var step = engine.Choose(chosen);
                yield return view.Move(step);
            }
            view.SetTurns(engine.TurnsLeft);

            var result = engine.Settle();
            view.ShowFinish(result);
            View.Status.Show(result.ReachedGoal ? $"上り！  {SugorokuLabels.Pay(rules, result.Tier)}" : "日が暮れた（没収）",
                result.ReachedGoal ? $"段 {result.Tier} で上がりました" : $"{rules.Turns} 回のうちに上りへ着けませんでした");
            yield return Settle(result, result.Bets);
            yield return Tween.Wait(ResultSeconds);
        }

        /// <summary>戦略ヒント: 2つの目それぞれの期待値（賭け金に対する倍率）。</summary>
        string Advice(int first, int second)
        {
            double a = solver.ValueOf(engine.TurnsLeft, engine.Position, engine.Tier, first);
            double b = solver.ValueOf(engine.TurnsLeft, engine.Position, engine.Tier, second);
            return $"HINT  期待値  {first} で進む <color=#FFE7A3>x{a:0.00}</color>  ·  {second} で進む <color=#FFE7A3>x{b:0.00}</color>";
        }

        protected override void AbortRound()
        {
            choosing = false;
            engine.Abort();
        }
    }
}
