using System.Collections;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Delve;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Delve
{
    /// <summary>
    /// DELVE の進行役。ベット受付 → （扉を選ぶ / 引き返す）を魔物に遭うか引き返すまで → 精算 を繰り返す。
    /// </summary>
    public sealed class DelveDirector : CasinoDirector
    {
        const float ResultSeconds = 2.0f;

        DelveRules rules;
        DelveEngine engine;
        DelveView view;
        IRandomSource random;
        int? door;
        bool cashOut;
        bool choosing;

        public void Begin(DelveRules rules, DelveEngine engine, IRandomSource random, DelveView view, WagerSession session, ITableWallet wallet)
        {
            this.rules = rules;
            this.engine = engine;
            this.random = random;
            this.view = view;
            view.DoorChosen += d =>
            {
                if (choosing)
                    door = d;
            };
            view.CashOutClicked += () => cashOut = choosing && engine.CanCashOut;
            Begin(view.Casino, session, wallet);
        }

        protected override GameId Game => GameId.Delve;
        protected override float BettingSeconds => rules.BettingSeconds;
        protected override long TableMinimum => rules.MinBet;
        protected override string BettingHint => "STAKE にチップを置いて DELVE";
        protected override string SpotName(int spot) => "STAKE";
        protected override IReadOnlyList<(string title, string body)> HelpPages => DelveHelp.Pages(rules);

        void Update() => view?.PollKeys(choosing, choosing && engine.CanCashOut);

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (DelveResult)outcome;
            var tags = new List<string>();
            if (!result.Busted && result.ClearedDepth >= 5)
                tags.Add(RoundTags.DelveDeepReturn);
            if (!result.Busted && result.ClearedDepth >= rules.Depths)
                tags.Add(RoundTags.DelveLegend);
            return tags;
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            engine.BeginBetting();
            engine.Deal(inPlay);
            long stake = inPlay.Total;
            View.Hud.SetInfo($"DIVE {engine.RoundNumber}");

            while (engine.Phase == DelvePhase.Choosing)
            {
                var floor = rules.Floors[engine.ClearedDepth];
                view.ShowFloor(rules, engine.ClearedDepth, false, stake);
                view.LayoutDoors(floor.Doors);
                yield return Tween.Wait(0.3f);
                yield return Choose(stake);
                if (cashOut)
                {
                    engine.CashOut();
                    Sfx.Play(SfxKind.ChipsCollect);
                    break;
                }
                var reveal = engine.Open(door.Value);
                yield return view.Reveal(reveal);
            }

            var result = engine.Settle();
            view.ShowFloor(rules, result.ClearedDepth, result.Busted, stake);
            view.ShowCashOut(false);
            if (result.Busted)
                View.Status.Show("DEFEATED", $"B{result.ClearedDepth + 1} で魔物に遭いました");
            else
                View.Status.Show(result.ClearedDepth >= rules.Depths ? "LEGENDARY RETURN" : "SAFE RETURN",
                    $"B{result.ClearedDepth} から帰還  ·  x{rules.MultiplierHundredths(result.ClearedDepth) / 100.0:N2}");
            yield return Settle(result, result.Bets);
            yield return Tween.Wait(ResultSeconds);
        }

        IEnumerator Choose(long stake)
        {
            door = null;
            cashOut = false;
            choosing = true;
            int cleared = engine.ClearedDepth;
            long purse = cleared > 0 ? stake * rules.MultiplierHundredths(cleared) / 100 : 0;
            view.SetDoorsSelectable(true);
            view.ShowCashOut(true, purse, engine.CanCashOut);
            View.Status.Show("CHOOSE A DOOR", engine.CanCashOut ? "扉を選んで先へ進むか、CASH OUT で宝を持ち帰る" : "扉を1枚選んでください");

            var timer = new CountdownTimer(rules.DecisionSeconds);
            while (door == null && !cashOut && !timer.Expired)
            {
                timer.Tick(View.Status, Paused);
                yield return null;
            }
            choosing = false;
            view.SetDoorsSelectable(false);
            view.ShowCashOut(false);
            View.Status.HideTimer();
            if (door == null && !cashOut)
            {
                // 時間切れ: 抜けていれば持ち帰る。まだなら扉を無作為に選ぶ（どの扉も確率は同じ）
                if (engine.CanCashOut)
                {
                    cashOut = true;
                    View.Toast.Show("時間切れ: CASH OUT しました");
                }
                else
                {
                    door = random.Next(rules.Floors[engine.ClearedDepth].Doors);
                    View.Toast.Show("時間切れ: 扉を無作為に選びました");
                }
            }
            Sfx.Play(SfxKind.Chip);
        }

        protected override void AbortRound()
        {
            engine.Abort();
            choosing = false;
            view.ShowCashOut(false);
            view.SetDoorsSelectable(false);
        }

        protected override IEnumerator ResetTable()
        {
            view.ShowFloor(rules, 0, false);
            view.LayoutDoors(rules.Floors[0].Doors);
            yield return Tween.Wait(0.2f);
        }
    }
}
