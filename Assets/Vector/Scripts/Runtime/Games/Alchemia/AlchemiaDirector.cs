using System.Collections;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Alchemia;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Alchemia
{
    /// <summary>
    /// ALCHEMIA の進行役。BET を選んで SPIN → 結果を即座に精算 → 盤面の連鎖を演出 を繰り返す。
    /// チップラックの代わりに、賭け金の段階と SPIN / AUTO で賭ける。
    /// </summary>
    public sealed class AlchemiaDirector : CasinoDirector
    {
        const int AutoSpins = 10;

        /// <summary>これ以上の当たりで AUTO を止める（賭け金の倍数）。</summary>
        const double AutoStopMultiple = 50;

        AlchemiaRules rules;
        AlchemiaEngine engine;
        AlchemiaView view;
        int betIndex = 2;
        int autoLeft;
        bool spinRequested;
        bool betting;

        public void Begin(AlchemiaRules rules, AlchemiaEngine engine, AlchemiaView view, WagerSession session, ITableWallet wallet)
        {
            this.rules = rules;
            this.engine = engine;
            this.view = view;
            view.SpinClicked += () => spinRequested = betting;
            view.AutoClicked += ToggleAuto;
            view.BetStepped += StepBet;
            Begin(view.Casino, session, wallet);
            RefreshControls();
        }

        protected override GameId Game => GameId.Alchemia;
        protected override float BettingSeconds => rules.SpinSeconds;
        protected override long TableMinimum => rules.BetLevels[0];
        protected override string BettingHint => "BET を選んで SPIN";
        protected override string SpotName(int spot) => "BET";
        protected override IReadOnlyList<(string title, string body)> HelpPages => AlchemiaHelp.Pages(rules);

        long Bet => rules.BetLevels[betIndex];

        void Update() => view?.PollKeys();

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (SpinResult)outcome;
            var tags = new List<string>();
            int highest = result.HighestTierCreated;
            if (highest >= 4)
                tags.Add(RoundTags.AlchemiaRelic);
            if (highest >= Symbol.MaxTier)
                tags.Add(RoundTags.AlchemiaStone);
            if (result.HasTransmutation)
                tags.Add(RoundTags.AlchemiaTransmutation);
            return tags;
        }

        void StepBet(int delta)
        {
            if (!betting || autoLeft > 0)
                return;
            betIndex = System.Math.Max(0, System.Math.Min(rules.BetLevels.Count - 1, betIndex + delta));
            Sfx.Play(SfxKind.Chip, 0.6f);
            RefreshControls();
        }

        void ToggleAuto()
        {
            if (autoLeft > 0)
            {
                autoLeft = 0;
                View.Toast.Show("AUTO を止めました");
            }
            else if (betting)
            {
                autoLeft = AutoSpins;
                spinRequested = true;
            }
            RefreshControls();
        }

        void RefreshControls()
        {
            view.SetBet(Bet, betIndex > 0, betIndex < rules.BetLevels.Count - 1);
            view.SetControls(betting, autoLeft > 0, autoLeft);
        }

        protected override IEnumerator WaitForBets()
        {
            while (true)
            {
                betting = true;
                RefreshControls();
                View.Status.Show(autoLeft > 0 ? $"AUTO  残り {autoLeft} 回" : "PLACE YOUR BET", autoLeft > 0 ? "STOP で自動スピンを止められます" : BettingHint);
                if (autoLeft > 0)
                {
                    yield return Tween.Wait(0.35f);
                    spinRequested = autoLeft > 0;
                }
                while (!spinRequested)
                    yield return null;
                spinRequested = false;
                betting = false;

                Session.ClearBets();
                var error = Session.TryAddChip(AlchemiaRules.Spin, Bet);
                if (error == ChipError.None)
                    break;
                autoLeft = 0;
                Sfx.Play(SfxKind.Error);
                View.Toast.Show(Labels.ChipProblem(error, "BET", rules.MaxFor(AlchemiaRules.Spin)), Theme.Negative);
            }
            if (autoLeft > 0)
                autoLeft--;
            RefreshControls();
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            long bet = inPlay.Total;
            var result = engine.Spin(inPlay);
            // スピンの瞬間に結果は確定しているので、演出の前に精算して記録する
            Record(result);
            View.Hud.SetInfo($"SPIN {engine.SpinNumber}");
            View.Status.Show("TRANSMUTING…", $"BET {Labels.Money(bet)}");
            view.SetWin(0, "");
            yield return view.DropIn(result.Initial);

            long shown = 0;
            for (int i = 0; i < result.Steps.Count; i++)
            {
                yield return view.PlayStep(result.Steps[i], i, bet, shown);
                shown += bet * result.Steps[i].PayHundredths / 100;
            }

            long paid = result.TotalReturn;
            view.SetWin(paid, result.Steps.Count == 0 ? "" : result.Steps.Count == 1 ? "1 CASCADE" : $"{result.Steps.Count} CASCADES");
            double multiple = result.WinMultiple;
            if (result.Capped)
                view.ShowBanner("MAX WIN", Theme.GoldBright);
            else if (multiple >= 200)
                view.ShowBanner("EPIC WIN", Theme.GoldBright);
            else if (multiple >= AutoStopMultiple)
                view.ShowBanner("MEGA WIN", Theme.GoldBright);
            else if (multiple >= 10)
                view.ShowBanner("BIG WIN", Theme.Gold);
            if (paid > 0)
            {
                PlayResultSound(result);
                View.Status.Show($"WIN {Labels.Money(paid)}", $"{multiple:N2} 倍");
                view.SetLastWin(paid, multiple);
            }
            else
            {
                View.Status.Show("NO WIN", "");
            }
            View.Hud.SetBalance(Session.Balance, true);
            if (multiple >= AutoStopMultiple || result.HasTransmutation)
                autoLeft = 0;
            yield return Tween.Wait(paid > 0 ? 1.2f : 0.5f);
        }

        protected override void AbortRound()
        {
            autoLeft = 0;
            betting = false;
        }
    }
}
