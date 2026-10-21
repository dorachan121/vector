using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector.Core;
using Vector.Core.Harpoon;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// HARPOON の進行役。BET を選んで獲物を押す → 1発撃つ（結果は押した瞬間に決まる）→ 捕まえたか逃げられたかを演出 を繰り返す。
    /// チップラックの代わりに、獲物を押すことが「その獲物に賭ける」操作になる。LOCK なら同じ種類を自動で狙い続ける。
    /// </summary>
    public sealed class HarpoonDirector : CasinoDirector
    {
        /// <summary>LOCK で次の1発を撃つまでの間（連射しすぎないように）。</summary>
        const float LockInterval = 0.25f;

        HarpoonRules rules;
        HarpoonEngine engine;
        HarpoonView view;
        int betIndex = 1;
        int locked = -1;
        int lastQuarry = -1;
        bool aiming;
        FishView pending;
        FishView target;

        public void Begin(HarpoonRules rules, HarpoonEngine engine, HarpoonView view, WagerSession session, ITableWallet wallet)
        {
            this.rules = rules;
            this.engine = engine;
            this.view = view;
            view.Shoal.Tapped += OnTapped;
            view.BetStepped += StepBet;
            view.LockClicked += ToggleLock;
            Begin(view.Casino, session, wallet);
            RefreshControls();
        }

        protected override GameId Game => GameId.Harpoon;
        protected override float BettingSeconds => 0f;
        protected override long TableMinimum => rules.BetLevels[0];
        protected override string BettingHint => "泳いでいる獲物を押すとモリを撃ちます（1発ごとに BET がかかります）";
        protected override string SpotName(int spot) => HarpoonLabels.Name(spot);
        protected override IReadOnlyList<(string title, string body)> HelpPages => HarpoonHelp.Pages(rules);

        long Bet => rules.BetLevels[betIndex];

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (HarpoonResult)outcome;
            var tags = new List<string>();
            if (!result.Caught)
                return tags;
            if (result.Multiple >= 50)
                tags.Add(RoundTags.HarpoonBigCatch);
            if (result.Quarry == Quarry.Leviathan)
                tags.Add(RoundTags.HarpoonLeviathan);
            if (result.Quarry == Quarry.Jellyfish)
                tags.Add(RoundTags.HarpoonJelly);
            return tags;
        }

        void OnTapped(FishView fish)
        {
            if (aiming && pending == null && !Paused)
                pending = fish;
        }

        void StepBet(int delta)
        {
            betIndex = Mathf.Clamp(betIndex + delta, 0, rules.BetLevels.Count - 1);
            Sfx.Play(SfxKind.Chip, 0.6f);
            RefreshControls();
        }

        void ToggleLock()
        {
            if (locked >= 0)
            {
                locked = -1;
                View.Toast.Show("LOCK を解除しました");
            }
            else if (lastQuarry >= 0)
            {
                locked = lastQuarry;
                View.Toast.Show($"LOCK ON  {HarpoonLabels.Name(locked)} が現れたら自動で撃ちます", HarpoonLabels.ColorOf(locked));
            }
            else
            {
                View.Toast.Show("先に獲物を1度押して狙ってください");
            }
            RefreshControls();
            if (aiming)
                ShowAimStatus();
        }

        void RefreshControls()
        {
            // 賭け金は撃つ前ならいつでも変えられる（1発ずつ決まるので、途中で変えても公平さは変わらない）
            view.SetBet(Bet, betIndex > 0, betIndex < rules.BetLevels.Count - 1);
            view.SetLock(locked);
        }

        void ShowAimStatus()
        {
            if (locked >= 0)
                View.Status.Show($"LOCK ON  ·  {HarpoonLabels.Name(locked)}", "同じ種類が来たら自動で撃ちます（LOCK で解除）");
            else
                View.Status.Show("TAKE AIM", "泳いでいる獲物を押してモリを撃つ");
        }

        protected override IEnumerator WaitForBets()
        {
            while (true)
            {
                pending = null;
                aiming = true;
                ShowAimStatus();
                float wait = LockInterval;
                while (pending == null)
                {
                    if (locked >= 0 && !Paused)
                    {
                        wait -= Time.unscaledDeltaTime * Tween.Speed;
                        if (wait <= 0)
                            pending = view.Shoal.Nearest(locked, view.Launcher.Pivot);
                    }
                    yield return null;
                }
                aiming = false;
                // 撃つ前に泳ぎ去っていたら狙い直す
                if (!view.Shoal.IsTargetable(pending))
                    continue;

                Session.ClearBets();
                var error = Session.TryAddChip(pending.Quarry, Bet);
                if (error == ChipError.None)
                    break;
                locked = -1;
                RefreshControls();
                Sfx.Play(SfxKind.Error);
                View.Toast.Show(Labels.ChipProblem(error, "BET", rules.MaxFor(pending.Quarry)), Theme.Negative);
            }
            target = pending;
            target.Hooked = true;
            lastQuarry = target.Quarry;
            if (locked >= 0)
                locked = target.Quarry;
            view.Flash(target.Quarry);
            RefreshControls();
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            var result = engine.Fire(inPlay);
            // 撃った瞬間に結果は確定しているので、演出の前に精算して記録する
            Record(result);
            View.Hud.SetInfo($"SHOT {engine.ShotNumber}");
            View.Status.Show("FIRE!", $"{HarpoonLabels.Name(target.Quarry)}  {HarpoonLabels.Multiple(rules, target.Quarry)}  ·  捕獲率 {HarpoonLabels.Chance(rules, target.Quarry)}");
            yield return view.Launcher.Fire(target);

            if (result.Caught)
            {
                yield return view.Catch(target, result);
                PlayResultSound(result);
                View.Status.Show($"CAUGHT!  x{result.Multiple}", $"{HarpoonLabels.Name(result.Quarry)}を仕留めた  +{Labels.Money(result.TotalReturn)}");
                if (result.Quarry == Quarry.Leviathan)
                    View.Toast.Show("伝説の獲物、リヴァイアサンを仕留めた！", Theme.GoldBright);
                if (result.Multiple >= 50)
                    BigWinBanner.Show(View.Overlay, result.Net);
            }
            else
            {
                yield return view.Escape(target);
                View.Status.Show("ESCAPED", $"{HarpoonLabels.Name(result.Quarry)}に逃げられた");
            }
            View.Hud.SetBalance(Session.Balance, true);
            target = null;
        }

        protected override void AbortRound()
        {
            locked = -1;
            aiming = false;
            if (target != null)
                target.Dart();
            target = null;
            pending = null;
        }
    }
}
