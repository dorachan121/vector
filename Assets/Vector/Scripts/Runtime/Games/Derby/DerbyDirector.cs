using System.Collections;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Derby;
using Vector.Core.Rpg;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Derby
{
    /// <summary>
    /// FOUR BEASTS DERBY の進行役。出走表とオッズの発表 → 馬券の受付 → レース → 精算 を繰り返す。
    /// </summary>
    public sealed class DerbyDirector : CasinoDirector
    {
        const float StepSeconds = 0.34f;
        const float ResultSeconds = 2.6f;

        DerbyRules rules;
        DerbyEngine engine;
        DerbyView view;
        int raceNumber;

        public void Begin(DerbyRules rules, DerbyEngine engine, DerbyView view, WagerSession session, ITableWallet wallet)
        {
            this.rules = rules;
            this.engine = engine;
            this.view = view;
            Begin(view.Casino, session, wallet);
        }

        protected override GameId Game => GameId.Derby;
        protected override float BettingSeconds => rules.BettingSeconds;
        protected override long TableMinimum => rules.ExactaMinBet;
        protected override string BettingHint => "オッズ表のマスにチップを置いて START";
        protected override string SpotName(int spot) => DerbyLabels.Ticket(spot);
        protected override IReadOnlyList<(string title, string body)> HelpPages => DerbyHelp.Pages(rules);

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (RaceResult)outcome;
            var tags = new List<string>();
            foreach (var bet in result.Bets)
            {
                if (bet.Outcome != BetOutcome.Win)
                    continue;
                if (DerbyTickets.IsExacta(bet.Spot) && !tags.Contains(RoundTags.DerbyExacta))
                    tags.Add(RoundTags.DerbyExacta);
                if (result.Card.OddsTenths(bet.Spot) >= 100 && !tags.Contains(RoundTags.DerbyLongshot))
                    tags.Add(RoundTags.DerbyLongshot);
            }
            return tags;
        }

        protected override IEnumerator BeforeBetting()
        {
            if (engine.Phase == DerbyPhase.Complete)
                engine.BeginBetting();
            raceNumber++;
            view.ShowRace(engine.Race, raceNumber);
            View.Hud.SetInfo($"RACE {raceNumber}  ·  DISTANCE {engine.Race.Distance}");
            yield return Tween.Wait(0.4f);
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            engine.Start(inPlay);
            View.Status.Show("AND THEY'RE OFF!", "カードのスートの神獣が1歩ずつ進みます");
            int leader = -1;
            while (engine.Phase == DerbyPhase.Running)
            {
                var step = engine.Step();
                yield return view.Step(step, StepSeconds);
                if (engine.Phase == DerbyPhase.Running)
                {
                    view.ShowLive(engine.LiveProbabilities(), engine.First >= 0, engine.First);
                    Commentate(step, ref leader);
                }
            }

            var result = engine.Settle();
            view.HighlightWinners(result);
            view.AddHistory(result, raceNumber);
            View.Status.Show($"1着 {DerbyLabels.Kanji(result.First)}  ·  2着 {DerbyLabels.Kanji(result.Second)}",
                $"WIN {DerbyLabels.Odds(result.Card.OddsTenths(DerbyTickets.Win(result.First)))}倍  ·  " +
                $"EXACTA {DerbyLabels.Odds(result.Card.OddsTenths(result.WinningExacta))}倍  ·  {result.CardsDrawn} 枚");
            yield return Settle(result, result.Bets);
            yield return Tween.Wait(ResultSeconds);
        }

        /// <summary>実況（先頭が替わった・ゴール目前）をトーストで流す。</summary>
        void Commentate(RaceStep step, ref int leader)
        {
            if (step.FinishedPlace == 1)
            {
                View.Toast.Show($"1着は {DerbyLabels.Kanji(step.Runner)}！ 2着争いは…", Theme.GoldBright);
                return;
            }
            if (engine.First >= 0)
                return;
            int best = 0;
            for (int r = 1; r < DerbyTickets.Runners; r++)
            {
                if (engine.Positions[r] > engine.Positions[best])
                    best = r;
            }
            bool tied = false;
            for (int r = 0; r < DerbyTickets.Runners; r++)
                tied |= r != best && engine.Positions[r] == engine.Positions[best];
            if (!tied && best != leader)
            {
                leader = best;
                if (engine.Race.Distance - engine.Positions[best] <= 2)
                    View.Toast.Show($"{DerbyLabels.Kanji(best)}、ゴール目前！");
                else if (engine.CardsDrawn > 4)
                    View.Toast.Show($"{DerbyLabels.Kanji(best)} が先頭に立った！");
            }
        }

        protected override void AbortRound() => engine.Abort();
    }
}
