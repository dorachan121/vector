using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector.Core;
using Vector.Core.Hanabi;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Hanabi
{
    /// <summary>
    /// HANABI の進行役。ベット受付 → 打ち上げ（暴発点はこの瞬間に決まる）→ 倍率を上げながら昇る玉を「開く」か、自動の倍率で開く
    /// → 開けたら大輪、間に合わなければ暴発 → 精算 を繰り返す。
    /// </summary>
    public sealed class HanabiDirector : CasinoDirector
    {
        /// <summary>倍率の伸び（毎秒）。倍率 = e^(Growth × 秒) で、2倍まで約 4.6 秒、10倍まで約 15 秒。</summary>
        const float Growth = 0.15f;

        /// <summary>自動で開く倍率の選択肢（0 は使わない）。</summary>
        static readonly int[] AutoTargets = { 0, 150, 200, 300, 500, 1_000, 2_000, 5_000, 10_000 };

        const float ResultSeconds = 1.4f;

        HanabiRules rules;
        HanabiEngine engine;
        HanabiView view;
        int autoIndex;
        bool cashRequested;

        public void Begin(HanabiRules rules, HanabiEngine engine, HanabiView view, WagerSession session, ITableWallet wallet)
        {
            this.rules = rules;
            this.engine = engine;
            this.view = view;
            view.CashOutClicked += () => cashRequested = true;
            view.AutoStepped += StepAuto;
            Begin(view.Casino, session, wallet);
            RefreshAuto();
        }

        protected override GameId Game => GameId.Hanabi;
        protected override float BettingSeconds => rules.BettingSeconds;
        protected override long TableMinimum => rules.MinBet;
        protected override string BettingHint => "チップを置いて LAUNCH（昇る花火が弾ける前に「開く」）";
        protected override string SpotName(int spot) => "BET";
        protected override IReadOnlyList<(string title, string body)> HelpPages => HanabiHelp.Pages(rules, AutoTargets);

        int AutoTarget => AutoTargets[autoIndex];

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (HanabiResult)outcome;
            var tags = new List<string>();
            if (!result.Opened)
                return tags;
            if (result.CashedHundredths >= 200)
                tags.Add(RoundTags.HanabiBloom);
            if (result.CashedHundredths >= 1_000)
                tags.Add(RoundTags.HanabiGrand);
            // 暴発点の 9 割以上まで粘って開いた（1.5 倍以上のとき）
            if (result.CashedHundredths >= 150 && result.CashedHundredths * 10L >= result.CrashHundredths * 9L)
                tags.Add(RoundTags.HanabiClose);
            return tags;
        }

        void Update()
        {
            if (view != null && view.CashOutKey)
                cashRequested = true;
        }

        void StepAuto(int delta)
        {
            autoIndex = Mathf.Clamp(autoIndex + delta, 0, AutoTargets.Length - 1);
            Sfx.Play(SfxKind.Chip, 0.6f);
            RefreshAuto();
        }

        void RefreshAuto() => view.SetAuto(AutoTarget, autoIndex > 0, autoIndex < AutoTargets.Length - 1);

        protected override IEnumerator BeforeBetting()
        {
            if (engine.Phase == HanabiPhase.Complete)
                engine.BeginBetting();
            view.ResetShell();
            View.Hud.SetInfo($"LAUNCH {engine.RoundNumber + 1}");
            yield break;
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            long stake = inPlay.Total;
            engine.Launch(inPlay);
            view.Launch();
            cashRequested = false;
            view.SetFlying(true);
            View.Status.Show("RISING", "弾ける前に「開く」（Space）");

            float seconds = 0;
            int shown = HanabiRules.One;
            HanabiResult result = null;
            while (result == null)
            {
                yield return null;
                if (!Paused)
                    seconds += Time.unscaledDeltaTime * Tween.Speed;
                int next = (int)Math.Min(rules.MaxHundredths, HanabiRules.One * Math.Exp(Growth * seconds));
                if (AutoTarget > 0 && AutoTarget > shown)
                    next = Math.Min(next, AutoTarget);
                if (next > shown && engine.HasBurstAt(next))
                {
                    engine.Burst();
                    result = engine.Settle();
                    break;
                }
                shown = Math.Max(shown, next);
                view.SetMultiplier(shown, stake);
                bool wantOpen = cashRequested || (AutoTarget > 0 && shown >= AutoTarget) || shown >= rules.MaxHundredths;
                if (wantOpen && shown >= rules.MinCashOut)
                {
                    engine.CashOut(shown);
                    result = engine.Settle();
                }
            }
            view.SetFlying(false);
            view.AddHistory(result.CrashHundredths);

            if (result.Opened)
            {
                View.Status.Show($"開花！  {HanabiLabels.Multiple(result.CashedHundredths)}", $"暴発点は {HanabiLabels.Multiple(result.CrashHundredths)} でした");
                yield return view.Bloom(result.CashedHundredths, result.TotalReturn);
            }
            else
            {
                View.Status.Show("暴発", $"{HanabiLabels.Multiple(result.CrashHundredths)} で弾けました");
                yield return view.Burst(result.CrashHundredths);
            }
            yield return Settle(result, result.Bets);
            yield return Tween.Wait(ResultSeconds);
        }

        protected override void AbortRound()
        {
            cashRequested = false;
            view.SetFlying(false);
            engine.Abort();
        }
    }
}
