using System.Collections;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Orbit;
using Vector.Core.Rpg;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Orbit
{
    /// <summary>
    /// ORBIT の進行役。ベット受付 → スピン（結果は押した瞬間に決まる）→ 惑星が止まる演出 → 精算 を繰り返す。
    /// </summary>
    public sealed class OrbitDirector : CasinoDirector
    {
        const float ResultSeconds = 2.4f;

        OrbitRules rules;
        OrbitEngine engine;
        OrbitView view;

        public void Begin(OrbitRules rules, OrbitEngine engine, OrbitView view, WagerSession session, ITableWallet wallet)
        {
            this.rules = rules;
            this.engine = engine;
            this.view = view;
            Begin(view.Casino, session, wallet);
        }

        protected override GameId Game => GameId.Orbit;
        protected override float BettingSeconds => rules.BettingSeconds;
        protected override long TableMinimum => rules.MinBet;
        protected override string BettingHint => "賭け先のマスにチップを置いて SPIN（いくつでも同時に賭けられます）";
        protected override string SpotName(int spot) => OrbitLabels.Spot(spot);
        protected override IReadOnlyList<(string title, string body)> HelpPages => OrbitHelp.Pages(rules);

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (OrbitResult)outcome;
            var tags = new List<string>();
            foreach (var bet in result.Bets)
            {
                if (bet.Outcome != BetOutcome.Win)
                    continue;
                if (bet.Spot == OrbitSpots.InnerConjunction || bet.Spot == OrbitSpots.OuterConjunction)
                    tags.Add(RoundTags.OrbitConjunction);
                else if (bet.Spot == OrbitSpots.Alignment)
                    tags.Add(RoundTags.OrbitAlignment);
                else if (bet.Spot == OrbitSpots.Comet)
                    tags.Add(RoundTags.OrbitComet);
            }
            return tags;
        }

        protected override IEnumerator BeforeBetting()
        {
            if (engine.Phase == OrbitPhase.Complete)
                engine.BeginBetting();
            View.Hud.SetInfo($"SPIN {engine.RoundNumber + 1}");
            yield break;
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            var spin = engine.Spin(inPlay);
            View.Status.Show("THE PLANETS ARE MOVING", "内惑星・中惑星・外惑星の順に止まります");
            yield return view.Orrery.Play(spin);

            var result = engine.Settle();
            view.ShowWinners(spin);
            view.AddHistory(spin);
            View.Status.Show(OrbitLabels.Headline(spin), OrbitLabels.Describe(spin));
            if (spin.Comet)
                View.Toast.Show("彗星が横切った！ 彗星への賭けだけが当たりです", OrbitLabels.CometColor);
            else if (spin.Alignment)
                View.Toast.Show("惑星直列！ 3つの惑星が一直線に並んだ", Theme.GoldBright);
            yield return Settle(result, result.Bets);
            yield return Tween.Wait(ResultSeconds);
        }

        protected override IEnumerator ResetTable()
        {
            view.ClearWinners();
            view.Orrery.Reset();
            yield return Tween.Wait(0.3f);
        }

        protected override void AbortRound() => engine.Abort();
    }
}
