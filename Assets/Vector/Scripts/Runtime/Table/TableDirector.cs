using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.Persistence;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Table
{
    /// <summary>
    /// テーブルの進行役。ベット受付 → 配札 → ライン選択 → 交互の配置 → 勝負 → 片付け を繰り返す。
    /// 1ラウンドを GuardedRoutine で包み、想定外の例外が起きたらラウンドを無効にして賭け金を返金する。
    /// </summary>
    public sealed class TableDirector : MonoBehaviour
    {
        const int MaxConsecutiveFaults = 3;
        const float ResultSeconds = 2.8f;

        /// <summary>リアリティチェック（プレイ時間と収支のお知らせ）を出す間隔。</summary>
        static readonly TimeSpan RealityCheckInterval = TimeSpan.FromMinutes(30);

        TableSession session;
        TableView view;
        TableFx fx;
        RoundPresenter presenter;
        ProfileService profiles;
        BettingInput betting;
        IPlayerSeat seat;
        RealityCheck realityCheck;

        TableRules Rules => session.Rules;
        RoundEngine Round => session.Round;
        static bool Paused => Dialog.IsAnyOpen;

        public void Begin(TableSession session, TableView view, ProfileService profiles, PlayerAdvisor advisor)
        {
            this.session = session;
            this.view = view;
            this.profiles = profiles;
            fx = new TableFx(view);
            presenter = new RoundPresenter(view, fx);
            betting = new BettingInput(session, view, fx);
            realityCheck = new RealityCheck(RealityCheckInterval, session.Balance, DateTime.UtcNow);
            seat = session.Mode == GameMode.Player
                ? new HumanSeat(view, advisor, Rules, () => profiles.Profile.settings.showStrategy, () => Paused)
                : (IPlayerSeat)new AiSeat(view, advisor);

            view.Hud.LobbyClicked += GoToLobby;
            view.Hud.HelpClicked += () => HelpDialog.Open(view.Overlay, Rules);
            view.Hud.SettingsClicked += () => SettingsDialog.Open(view.Overlay, profiles, GameContext.ApplySettings);
            betting.End();
            StartCoroutine(MainLoop());
        }

        IEnumerator MainLoop()
        {
            int consecutiveFaults = 0;
            while (true)
            {
                Exception fault = null;
                yield return GuardedRoutine.Run(PlayRound(), e => fault = e);
                if (fault == null)
                {
                    consecutiveFaults = 0;
                    continue;
                }
                yield return Recover(fault);
                if (++consecutiveFaults >= MaxConsecutiveFaults)
                {
                    Log.Error("エラーが続いたためロビーに戻ります。");
                    SceneManager.LoadScene(SceneNames.Lobby);
                    yield break;
                }
            }
        }

        IEnumerator PlayRound()
        {
            yield return Betting();
            yield return Deal();
            yield return ChooseFamily();
            while (Round.Phase != RoundPhase.Showdown)
                yield return Round.Phase == RoundPhase.PlayerTurn ? PlayerTurn() : DealerTurn();
            yield return Showdown();
            yield return Cleanup();
        }

        // ------------------------------------------------------------------
        // ベット受付
        // ------------------------------------------------------------------

        IEnumerator Betting()
        {
            KeepScreenOn(false);
            if (session.BeginBetting())
                yield return Shuffle();
            RefreshTableInfo();
            while (session.IsBroke)
                yield return OfferReload();
            if (realityCheck.IsDue(DateTime.UtcNow))
                yield return ShowRealityCheck();

            view.ShowBettingControls(true);
            view.Hud.SetLobbyEnabled(true);
            view.Status.Show("PLACE YOUR BETS", session.Mode == GameMode.Player
                ? "MAIN にチップを置いて DEAL（BONUS は任意）"
                : "PLAYER / DEALER / TIE にチップを置いて DEAL");
            betting.Begin();

            var timer = new CountdownTimer(Rules.BettingSeconds);
            while (!betting.DealRequested)
            {
                // チップを置くまでは急かさない
                if (session.Bets.IsEmpty)
                {
                    timer.Reset();
                    view.Status.HideTimer();
                }
                else
                {
                    timer.Tick(view.Status, Paused);
                    if (timer.Expired)
                    {
                        var check = session.CheckBets();
                        if (check.IsValid)
                            break;
                        view.Toast.Show(Labels.BetProblem(check), Theme.Negative);
                        timer.Reset();
                    }
                }
                yield return null;
            }
            view.Status.HideTimer();
            view.Hud.SetLobbyEnabled(false);
        }

        IEnumerator Shuffle()
        {
            view.Status.Show("SHUFFLING", "新しいシューを用意しています");
            Sfx.Play(SfxKind.Shuffle);
            view.Scoreboard.Render(session.Scoreboard, Round.Shoe.ShoeNumber);
            yield return Tween.Wait(1.1f);
            view.Toast.Show($"新しいシュー #{Round.Shoe.ShoeNumber}  ·  バーンカード {Labels.CardName(Round.Shoe.BurnedCard)}");
        }

        IEnumerator OfferReload()
        {
            yield return Dialog.Open(view.Overlay, "OUT OF CHIPS",
                    $"手持ちのチップがテーブルミニマム（{Labels.Money(Rules.MinBet)}）を下回りました。\n" +
                    $"遊戯用チップを {Labels.Money(PlayerProfile.StartingBalance)} まで補充できます。", new Vector2(820, 420))
                .AddButton("LOBBY", GoToLobby)
                .AddButton("RELOAD", () =>
                {
                    long added = profiles.Reload(Rules.MinBet);
                    session.Deposit(added);
                    realityCheck.RecordDeposit(added);
                    view.Hud.SetBalance(session.Available, true);
                    Sfx.Play(SfxKind.ChipsCollect);
                }, true)
                .WaitUntilClosed();
        }

        /// <summary>責任あるゲーミングのため、一定時間ごとにプレイ時間とこのセッションの収支を知らせる。</summary>
        IEnumerator ShowRealityCheck()
        {
            var elapsed = realityCheck.Elapsed(DateTime.UtcNow);
            yield return Dialog.Open(view.Overlay, "REALITY CHECK",
                    $"プレイ時間 {(int)elapsed.TotalHours}時間{elapsed.Minutes}分　このセッションの収支 {Labels.SignedMoney(realityCheck.Net(session.Balance))}\n" +
                    "休憩をはさみながら、無理のない範囲でお楽しみください。", new Vector2(860, 400))
                .AddButton("LOBBY", GoToLobby)
                .AddButton("CONTINUE", null, true)
                .WaitUntilClosed();
            realityCheck.Acknowledge(DateTime.UtcNow);
        }

        // ------------------------------------------------------------------
        // ラウンド
        // ------------------------------------------------------------------

        IEnumerator Deal()
        {
            KeepScreenOn(true);
            long stake = session.Bets.Total;
            session.Deal();
            profiles.RecordDeal(session.Balance, stake);
            betting.End();
            view.ShowBettingControls(false);
            view.Hud.SetBalance(session.Balance, true);
            view.Status.Show("NO MORE BETS");
            RefreshTableInfo();
            yield return Tween.Wait(0.4f);

            yield return fx.DealToBoard(Round.Board[Lines.CenterCell].Value, Lines.CenterCell, Seat.House);
            for (int i = 0; i < Round.Hand.Count; i++)
            {
                yield return fx.DealToHand(Round.Hand[i], i);
                yield return Tween.Wait(0.06f);
            }
            RefreshShoe();
            yield return Tween.Wait(0.3f);

            if (Round.Bets[BetSpot.Bonus] > 0 && Rules.BonusPays.ContainsKey(Round.BonusHand))
            {
                view.Paytable.HighlightBonus(Round.BonusHand);
                view.Toast.Show($"BONUS HIT!  {Labels.HandName(Round.BonusHand)}", Theme.GoldBright);
                Sfx.Play(SfxKind.Win, 0.7f);
            }
        }

        IEnumerator ChooseFamily()
        {
            LineFamily family = default;
            yield return seat.ChooseFamily(Round, f => family = f);
            Round.ChooseFamily(family);
            Sfx.Play(SfxKind.Chip);
            UpdateScores();
        }

        IEnumerator PlayerTurn()
        {
            Placement placement = default;
            yield return seat.ChoosePlacement(Round, p => placement = p);
            Round.PlacePlayerCard(placement.HandIndex, placement.Cell);
            yield return fx.MoveFromHand(placement.HandIndex, placement.Cell, Seat.Player);
            UpdateScores();
        }

        IEnumerator DealerTurn()
        {
            view.Status.Show("DEALER'S TURN", "ディーラーがシューから1枚引いて置きます");
            yield return Tween.Wait(0.3f);
            DealerMove move = default;
            var round = Round;
            yield return Background.Run(() => round.PlayDealerCard(), m => move = m);
            yield return fx.DealToBoard(move.Card, move.Cell, Seat.Dealer);
            RefreshShoe();
            UpdateScores();
        }

        IEnumerator Showdown()
        {
            var result = session.Settle();
            profiles.RecordSettlement(session.Balance, result);
            yield return presenter.ShowResult(result, Round.Bets[BetSpot.Bonus] > 0);

            view.Scoreboard.Render(session.Scoreboard, Round.Shoe.ShoeNumber);
            view.Hud.SetBalance(session.Balance, true);
            yield return Tween.Wait(ResultSeconds);
        }

        IEnumerator Cleanup()
        {
            presenter.Reset();
            yield return fx.CollectCards();
        }

        // ------------------------------------------------------------------
        // 障害時の復旧
        // ------------------------------------------------------------------

        IEnumerator Recover(Exception fault)
        {
            Log.Exception(fault);
            long refund = session.VoidRound();
            profiles.RecordVoid(session.Balance);
            betting.End();
            view.Picker.Hide();
            view.Status.HideTimer();
            presenter.Reset();
            yield return fx.CollectCards();
            view.Hud.SetBalance(session.Balance, true);
            view.Toast.Show(refund > 0
                ? $"問題が発生したためラウンドを無効にし、{Labels.Money(refund)} を返金しました"
                : "問題が発生したため、テーブルをリセットしました", Theme.Negative);
            yield return Tween.Wait(1.2f);
        }

        // ------------------------------------------------------------------
        // 補助
        // ------------------------------------------------------------------

        void UpdateScores() => presenter.ShowScores(Round.Board.ToValues(), Round.PlayerFamily);

        void RefreshTableInfo()
        {
            // ベット受付中は、これから配るラウンドの番号を出す
            int round = Round.Phase == RoundPhase.Betting ? Round.RoundNumber + 1 : Round.RoundNumber;
            view.Hud.SetRoundInfo(Round.Shoe.ShoeNumber, round);
            view.Scoreboard.Render(session.Scoreboard, Round.Shoe.ShoeNumber);
            RefreshShoe();
        }

        void RefreshShoe() => view.Shoe.SetRemaining(Round.Shoe.Remaining, Round.Shoe.TotalCards);

        /// <summary>
        /// ラウンド中は画面を消さない（観戦やディーラーの手番では操作が無いまま見続けるため）。
        /// ベット待ちは端末の設定に戻し、席を離れたまま点けっぱなしにならないようにする。
        /// </summary>
        static void KeepScreenOn(bool on) => Screen.sleepTimeout = on ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

        void OnDestroy() => KeepScreenOn(false);

        void GoToLobby()
        {
            // ベット受付中だけ離席できる。置いたチップはまだ引き落としていないのでそのまま手元に戻る。
            if (Round.Phase == RoundPhase.Betting || Round.Phase == RoundPhase.Complete)
                SceneManager.LoadScene(SceneNames.Lobby);
        }
    }
}
