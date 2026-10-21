using System;
using System.Collections;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Colosseum;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Colosseum
{
    /// <summary>
    /// COLOSSEUM の進行役。ベット受付 → FIGHT → （予告 → 斬る / 受ける）を決着まで → 精算 を繰り返す。
    /// </summary>
    public sealed class ColosseumDirector : CasinoDirector
    {
        const float ResultSeconds = 2.2f;

        BattleRules rules;
        BattleEngine engine;
        BattleSolver solver;
        ColosseumView view;
        BattleAction? chosen;
        BattleSetup? opponent;
        string beastTitle;
        BattleExchange lastExchange;

        /// <param name="opponent">挑戦で戦う特別な相手（null なら通常の獣）。solver はその相手の最善戦略にすること。</param>
        public void Begin(BattleRules rules, BattleEngine engine, BattleSolver solver, ColosseumView view, WagerSession session, ITableWallet wallet,
            BattleSetup? opponent = null, string beastTitle = "ARENA BEAST")
        {
            this.rules = rules;
            this.engine = engine;
            this.solver = solver;
            this.view = view;
            this.opponent = opponent;
            this.beastTitle = beastTitle;
            view.Reset(opponent ?? rules.Setup, beastTitle);
            view.ActionChosen += a => chosen = a;
            Begin(view.Casino, session, wallet);
        }

        protected override GameId Game => GameId.Colosseum;
        protected override float BettingSeconds => rules.BettingSeconds;
        protected override long TableMinimum => rules.MinBet;
        protected override string BettingHint => "MAIN にチップを置いて FIGHT";
        protected override string SpotName(int spot) => "MAIN";

        protected override IReadOnlyList<(string title, string body)> HelpPages
            => ColosseumHelp.Pages(rules, solver.WinProbability * (1 + rules.WinPays.Ratio));

        void Update() => view?.PollKeys();

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (BattleResult)outcome;
            return result.Winner == RoundWinner.Player && lastExchange.Critical && lastExchange.BeastDefeated
                ? new[] { RoundTags.ColosseumCriticalFinish }
                : new string[0];
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            engine.BeginBetting();
            engine.Deal(inPlay, opponent);
            view.Reset(engine.Setup, beastTitle);
            View.Hud.SetInfo($"BATTLE {engine.RoundNumber}");
            yield return AnnounceTurnOrder(You, new TurnSeat(beastTitle, Theme.DealerRed, Art.BeastHead));
            View.Status.Show("FIGHT!", "獣の予告を見て、斬るか受けるかを選んでください");
            yield return Tween.Wait(0.4f);

            while (engine.Phase == BattlePhase.PlayerTurn)
            {
                yield return view.RevealIntent(engine.Intent, engine.IntentDamage, engine.IsEnraged);
                BattleAction action = default;
                yield return ChooseAction(a => action = a);
                var exchange = engine.Act(action);
                lastExchange = exchange;
                yield return view.PlayExchange(exchange, engine.Setup);
            }

            var result = engine.Settle();
            yield return view.ShowVerdict(result);
            View.Status.Show(result.Winner == RoundWinner.Player ? "YOU WIN" : result.Winner == RoundWinner.Dealer ? "THE BEAST WINS" : "DRAW",
                $"{result.Turns} 手で決着  ·  残り体力 {result.PlayerHp} 対 {result.BeastHp}");
            yield return Settle(result, result.Bets);
            yield return Tween.Wait(ResultSeconds);
        }

        IEnumerator ChooseAction(Action<BattleAction> done)
        {
            int p = engine.PlayerHp, b = engine.BeastHp, intent = engine.Intent.Value;
            double strikeWin = solver.ValueOf(BattleAction.Strike, p, b, intent);
            double guardWin = solver.ValueOf(BattleAction.Guard, p, b, intent);
            bool hints = ShowStrategy;
            View.Status.Show("YOUR MOVE", $"予告 {engine.IntentDamage} ダメージ。STRIKE か GUARD を選んでください");
            chosen = null;
            view.ShowActions(true, hints ? strikeWin : (double?)null, hints ? guardWin : (double?)null);

            var timer = new CountdownTimer(rules.DecisionSeconds);
            while (chosen == null && !timer.Expired)
            {
                timer.Tick(View.Status, Paused);
                yield return null;
            }
            view.ShowActions(false);
            View.Status.HideTimer();
            if (chosen == null)
            {
                chosen = solver.Best(p, b, intent);
                View.Toast.Show($"時間切れ: 推奨の {chosen.Value.ToString().ToUpperInvariant()} を選びました");
            }
            Sfx.Play(SfxKind.Chip);
            done(chosen.Value);
        }

        protected override void AbortRound()
        {
            engine.Abort();
            view.ShowActions(false);
        }

        protected override IEnumerator ResetTable()
        {
            yield return view.Clear(opponent ?? rules.Setup);
        }
    }
}
