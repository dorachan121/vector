using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;
using Vector.Game.World;

namespace Vector.Game.Casino
{
    /// <summary>
    /// 全ゲーム共通のテーブルの進行役。ベット受付 → 勝負（ゲームごと） → 精算 → 片付け を繰り返す。
    /// 1ラウンドを GuardedRoutine で包み、想定外の例外が起きたらラウンドを無効にして賭け金を返金する。
    /// 勝負の中身は派生クラスが <see cref="PlayRound"/> で実装し、決着したら <see cref="Settle"/> を呼ぶ。
    /// </summary>
    public abstract class CasinoDirector : MonoBehaviour
    {
        const int MaxConsecutiveFaults = 3;

        /// <summary>この倍以上の勝ちは大勝ち（効果音と演出）にする。</summary>
        const long BigWinMultiple = 5;

        /// <summary>リアリティチェック（プレイ時間と収支のお知らせ）を出す間隔。</summary>
        static readonly TimeSpan RealityCheckInterval = TimeSpan.FromMinutes(30);

        static readonly string[] NoTags = new string[0];

        WagerInput betting;
        RealityCheck realityCheck;
        bool settled;
        bool leaving;
        bool turnOrderShown;

        protected CasinoView View { get; private set; }
        protected WagerSession Session { get; private set; }
        protected ITableWallet Wallet { get; private set; }

        /// <summary>勝負中（ベット受付と片付けの間）。この間は離席できない。</summary>
        protected bool InRound { get; private set; }

        protected static bool Paused => Dialog.IsAnyOpen;

        protected static bool ShowStrategy => GameContext.Profiles.Profile.settings.showStrategy;

        protected abstract GameId Game { get; }
        protected abstract float BettingSeconds { get; }

        /// <summary>これ未満の手持ちでは賭けられない（補充の判断に使う）。</summary>
        protected abstract long TableMinimum { get; }

        /// <summary>ベット受付中の説明。</summary>
        protected abstract string BettingHint { get; }

        /// <summary>トーストなどに出すベットエリアの名前。</summary>
        protected abstract string SpotName(int spot);

        protected abstract IReadOnlyList<(string title, string body)> HelpPages { get; }

        /// <summary>引き落とした後の勝負の中身。決着したら <see cref="Settle"/> か <see cref="Record"/> を呼ぶこと。</summary>
        protected abstract IEnumerator PlayRound(BetBoard inPlay);

        /// <summary>障害時に、進行中の勝負をエンジン側でも無効にする。</summary>
        protected abstract void AbortRound();

        /// <summary>結果表示を片付けて、次のベット受付の状態に戻す。</summary>
        protected virtual IEnumerator ResetTable()
        {
            yield break;
        }

        /// <summary>ベット受付を始める前の準備（新しい出走表を出すなど）。</summary>
        protected virtual IEnumerator BeforeBetting()
        {
            yield break;
        }

        /// <summary>精算したラウンドの出来事（クエストの条件用、RoundTags）。</summary>
        protected virtual IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome) => NoTags;

        /// <summary>フロアの主の挑戦中なら、その挑戦。</summary>
        protected ChallengeRun Challenge => (Wallet as ChallengeWallet)?.Run;

        /// <summary>先攻・後攻の演出に出す、この席のプレイヤー。</summary>
        protected static TurnSeat You => new TurnSeat("YOU", Palette.Sapphire, Cast.Player);

        /// <summary>先攻・後攻の演出に出すディーラー（主の挑戦中は、その主がディーラーを務める）。</summary>
        protected TurnSeat Dealer => Challenge != null
            ? new TurnSeat(StoryNames.Person(Challenge.Spec.Master), Palette.Ruby, Cast.Of(Challenge.Spec.Master).Look)
            : new TurnSeat("DEALER", Palette.Ruby, Cast.Dealer);

        /// <summary>
        /// 先攻・後攻のあるゲームのラウンドの始まりに、順番をカジノ風の演出で見せる。テーブルの最初のラウンドは大きく、以降は短く。
        /// </summary>
        protected IEnumerator AnnounceTurnOrder(TurnSeat first, TurnSeat second)
        {
            View.Status.Show("TURN ORDER", $"先攻 {first.Name}  ·  後攻 {second.Name}");
            yield return TurnOrderIntro.Play(View.Overlay, first, second, !turnOrderShown);
            turnOrderShown = true;
        }

        /// <summary>派生クラスが画面と賭け金の管理を組み立ててから呼ぶ。</summary>
        protected void Begin(CasinoView view, WagerSession session, ITableWallet wallet)
        {
            View = view;
            Session = session;
            Wallet = wallet;
            betting = new WagerInput(session, view, SpotName);
            realityCheck = new RealityCheck(RealityCheckInterval, session.Balance, DateTime.UtcNow);
            view.Hud.LobbyClicked += GoToLobby;
            view.Hud.HelpClicked += () => HelpDialog.Open(view.Overlay, HelpPages);
            view.Hud.SettingsClicked += () => SettingsDialog.Open(view.Overlay, GameContext.Profiles, GameContext.ApplySettings);
            betting.End();
            view.Hud.SetBalance(session.Balance, false);
            if (Challenge != null)
                ChallengeFlow.ShowHud(view.Hud, Challenge);
            StartCoroutine(MainLoop());
        }

        IEnumerator MainLoop()
        {
            int consecutiveFaults = 0;
            while (!leaving)
            {
                Exception fault = null;
                yield return GuardedRoutine.Run(Round(), e => fault = e);
                if (fault == null)
                {
                    consecutiveFaults = 0;
                    continue;
                }
                yield return Recover(fault);
                if (++consecutiveFaults >= MaxConsecutiveFaults)
                {
                    Log.Error("エラーが続いたためロビーに戻ります。");
                    ChallengeFlow.CancelAfterFault();
                    SceneFlow.Load(GameContext.ReturnScene);
                    yield break;
                }
            }
        }

        IEnumerator Round()
        {
            var challenge = Challenge;
            if (challenge != null)
            {
                challenge.CheckBroke(TableMinimum);
                if (challenge.IsOver)
                {
                    leaving = true;
                    yield return ChallengeFlow.Finish(View.Overlay, challenge);
                    yield break;
                }
            }
            yield return BeforeBetting();
            yield return Betting();

            KeepScreenOn(true);
            InRound = true;
            settled = false;
            long stake = Session.Bets.Total;
            var inPlay = Session.Commit();
            Wallet.RecordDeal(Session.Balance, stake);
            betting.End();
            View.ShowBettingControls(false);
            View.Hud.SetBalance(Session.Balance, true);
            yield return PlayRound(inPlay);
            if (!settled)
                throw new InvalidOperationException("勝負が精算されないまま終わりました。");

            yield return ResetTable();
            foreach (var target in View.Targets.Values)
            {
                target.ResetOutcome();
                target.SetAmount(0);
            }
            InRound = false;
        }

        IEnumerator Betting()
        {
            KeepScreenOn(false);
            while (Session.IsBrokeFor(TableMinimum))
                yield return OfferReload();
            if (realityCheck.IsDue(DateTime.UtcNow))
                yield return ShowRealityCheck();

            View.Hud.SetLobbyEnabled(true);
            yield return WaitForBets();
            View.Hud.SetLobbyEnabled(false);
        }

        /// <summary>
        /// チップを置いて開始ボタンを押すまで待つ（チップラックとベットエリアを使う標準の受付）。
        /// スロットのように独自の操作で賭けるゲームは上書きし、Session にベットを置いてから戻る。
        /// </summary>
        protected virtual IEnumerator WaitForBets()
        {
            View.ShowBettingControls(true);
            View.Status.Show("PLACE YOUR BETS", BettingHint);
            betting.Begin();

            var timer = new CountdownTimer(BettingSeconds);
            while (!betting.DealRequested)
            {
                // チップを置くまでは急かさない
                if (Session.Bets.IsEmpty)
                {
                    timer.Reset();
                    View.Status.HideTimer();
                }
                else
                {
                    timer.Tick(View.Status, Paused);
                    if (timer.Expired)
                    {
                        var check = Session.CheckBets();
                        if (check.IsValid)
                            break;
                        View.Toast.Show(Labels.WagerProblem(check, SpotName), Theme.Negative);
                        timer.Reset();
                    }
                }
                yield return null;
            }
            View.Status.HideTimer();
        }

        IEnumerator OfferReload()
        {
            if (!Wallet.CanReload)
            {
                yield return Dialog.Open(View.Overlay, "OUT OF CHIPS", "手持ちのチップがミニマムを下回りました。", new Vector2(760, 340))
                    .AddButton("LEAVE", GoToLobby, true)
                    .WaitUntilClosed();
                yield break;
            }
            yield return Dialog.Open(View.Overlay, "OUT OF CHIPS",
                    $"手持ちのチップがテーブルミニマム（{Labels.Money(TableMinimum)}）を下回りました。\n" +
                    $"遊戯用チップを {Labels.Money(Wallet.ReloadTarget)} まで補充できます。", new Vector2(820, 420))
                .AddButton("LOBBY", GoToLobby)
                .AddButton("RELOAD", () =>
                {
                    long added = Wallet.Reload(TableMinimum);
                    Session.Deposit(added);
                    realityCheck.RecordDeposit(added);
                    View.Hud.SetBalance(Session.Available, true);
                    Sfx.Play(SfxKind.ChipsCollect);
                }, true)
                .WaitUntilClosed();
        }

        /// <summary>責任あるゲーミングのため、一定時間ごとにプレイ時間とこのセッションの収支を知らせる。</summary>
        IEnumerator ShowRealityCheck()
        {
            var elapsed = realityCheck.Elapsed(DateTime.UtcNow);
            yield return Dialog.Open(View.Overlay, "REALITY CHECK",
                    $"プレイ時間 {(int)elapsed.TotalHours}時間{elapsed.Minutes}分　このセッションの収支 {Labels.SignedMoney(realityCheck.Net(Session.Balance))}\n" +
                    "休憩をはさみながら、無理のない範囲でお楽しみください。", new Vector2(860, 400))
                .AddButton("LOBBY", GoToLobby)
                .AddButton("CONTINUE", null, true)
                .WaitUntilClosed();
            realityCheck.Acknowledge(DateTime.UtcNow);
        }

        // ------------------------------------------------------------------
        // 精算
        // ------------------------------------------------------------------

        /// <summary>払い戻しを手持ちに戻して記録する（演出はしない）。スロットのように結果が先に確定するゲームで使う。</summary>
        protected void Record(IRoundOutcome outcome)
        {
            if (settled)
                throw new InvalidOperationException("この勝負はもう精算しています。");
            Session.Settle(outcome.TotalReturn);
            Wallet.RecordSettlement(Session.Balance, outcome, TagsFor(outcome));
            settled = true;
            if (Wallet is ProfileWallet profile)
                ChallengeFlow.AnnounceReport(View.Toast, profile.LastReport);
            else if (Challenge != null)
                ChallengeFlow.ShowHud(View.Hud, Challenge);
        }

        /// <summary>精算して記録し、チップの払い戻しを演出する。</summary>
        protected IEnumerator Settle(IRoundOutcome outcome, IReadOnlyList<SpotResult> bets)
        {
            Record(outcome);
            CelebrateResult(outcome);
            yield return PayOut(bets);
            View.Hud.SetBalance(Session.Balance, true);
        }

        /// <summary>負けたチップは親へ、勝ちは配当を足してから手元へ飛ばす。</summary>
        protected IEnumerator PayOut(IReadOnlyList<SpotResult> bets)
        {
            var bank = View.Bank.position;
            var wallet = View.Hud.BalanceWorldPosition;
            foreach (var bet in bets)
            {
                if (!View.Targets.TryGetValue(bet.Spot, out var target))
                    continue;
                target.ShowOutcome(bet.Outcome);
                if (bet.Outcome == BetOutcome.Win)
                {
                    ChipFlight.Fly(View.Fx, bet.Net, bank, target.ChipWorldPosition);
                }
                else if (bet.Outcome == BetOutcome.Lose)
                {
                    ChipFlight.Fly(View.Fx, bet.Stake, target.ChipWorldPosition, bank);
                    target.SetAmount(0);
                }
            }
            yield return Tween.Wait(ChipFlight.Seconds);

            bool anyReturn = false;
            foreach (var bet in bets)
            {
                if (bet.Return <= 0 || !View.Targets.TryGetValue(bet.Spot, out var target))
                    continue;
                anyReturn = true;
                target.SetAmount(bet.Return);
                ChipFlight.Fly(View.Fx, bet.Return, target.ChipWorldPosition, wallet, 0.35f);
            }
            if (anyReturn)
            {
                Sfx.Play(SfxKind.ChipsCollect);
                yield return Tween.Wait(0.35f + ChipFlight.Seconds);
            }
            foreach (var target in View.Targets.Values)
                target.SetAmount(0);
        }

        /// <summary>結果の効果音と、賭け金の <see cref="BigWinMultiple"/> 倍以上勝ったときの大勝ちの演出。</summary>
        protected void CelebrateResult(IRoundOutcome outcome)
        {
            PlayResultSound(outcome);
            if (IsBigWin(outcome))
                BigWinBanner.Show(View.Overlay, outcome.TotalReturn - outcome.TotalStake);
        }

        /// <summary>結果の効果音だけ（独自の勝ちの演出を持つゲーム用）。</summary>
        protected static void PlayResultSound(IRoundOutcome outcome)
        {
            long net = outcome.TotalReturn - outcome.TotalStake;
            if (net > 0)
                Sfx.Play(IsBigWin(outcome) ? SfxKind.BigWin : SfxKind.Win);
            else if (net < 0)
                Sfx.Play(SfxKind.Lose);
            else
                Sfx.Play(SfxKind.Push);
        }

        static bool IsBigWin(IRoundOutcome outcome) => outcome.TotalReturn - outcome.TotalStake >= Math.Max(1, outcome.TotalStake * BigWinMultiple);

        // ------------------------------------------------------------------
        // 障害時の復旧
        // ------------------------------------------------------------------

        IEnumerator Recover(Exception fault)
        {
            Log.Exception(fault);
            AbortRound();
            long refund = Session.Void();
            Wallet.RecordVoid(Session.Balance);
            betting.End();
            View.Status.HideTimer();
            Exception cleanupFault = null;
            yield return GuardedRoutine.Run(ResetTable(), e => cleanupFault = e);
            if (cleanupFault != null)
                Log.Exception(cleanupFault);
            foreach (var target in View.Targets.Values)
            {
                target.ResetOutcome();
                target.SetAmount(0);
            }
            InRound = false;
            View.Hud.SetBalance(Session.Balance, true);
            View.Toast.Show(refund > 0
                ? $"問題が発生したためラウンドを無効にし、{Labels.Money(refund)} を返金しました"
                : "問題が発生したため、テーブルをリセットしました", Theme.Negative);
            yield return Tween.Wait(1.2f);
        }

        // ------------------------------------------------------------------
        // 補助
        // ------------------------------------------------------------------

        /// <summary>
        /// ラウンド中は画面を消さない（演出を見続けるため）。ベット待ちは端末の設定に戻す。
        /// </summary>
        static void KeepScreenOn(bool on) => Screen.sleepTimeout = on ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

        protected virtual void OnDestroy() => KeepScreenOn(false);

        protected void GoToLobby()
        {
            // ベット受付中だけ離席できる。置いたチップはまだ引き落としていないのでそのまま手元に戻る。
            if (InRound || leaving)
                return;
            if (Challenge != null)
                ChallengeFlow.ConfirmForfeit(View.Overlay, Challenge);
            else
                SceneFlow.Load(GameContext.ReturnScene);
        }
    }
}
