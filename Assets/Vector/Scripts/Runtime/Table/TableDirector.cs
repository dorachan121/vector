using System;
using System.Collections;
using System.Collections.Generic;
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

        TableSession session;
        TableView view;
        TableFx fx;
        ProfileService profiles;
        BettingInput betting;
        IPlayerSeat seat;
        int openDialogs;

        TableRules Rules => session.Rules;
        RoundEngine Round => session.Round;
        bool Paused => openDialogs > 0;

        public void Begin(TableSession session, TableView view, ProfileService profiles, PlayerAdvisor advisor)
        {
            this.session = session;
            this.view = view;
            this.profiles = profiles;
            fx = new TableFx(view);
            betting = new BettingInput(session, view, fx);
            seat = session.Mode == GameMode.Player
                ? new HumanSeat(view, advisor, Rules, () => profiles.Profile.settings.showStrategy, () => Paused)
                : (IPlayerSeat)new AiSeat(view, advisor);

            view.Hud.LobbyClicked += GoToLobby;
            view.Hud.HelpClicked += () => Track(HelpDialog.Open(view.Overlay, Rules));
            view.Hud.SettingsClicked += () => Track(SettingsDialog.Open(view.Overlay, profiles, GameContext.ApplySettings));
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
            if (session.BeginBetting())
                yield return Shuffle();
            RefreshTableInfo();
            while (session.IsBroke)
                yield return OfferReload();

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
            view.Toast.Show($"新しいシュー #{Round.Shoe.ShoeNumber}  ·  バーンカード {Round.Shoe.BurnedCard}");
        }

        IEnumerator OfferReload()
        {
            bool closed = false;
            var dialog = Dialog.Open(view.Overlay, "OUT OF CHIPS",
                    $"手持ちのチップがテーブルミニマム（{Labels.Money(Rules.MinBet)}）を下回りました。\n" +
                    $"遊戯用チップを {Labels.Money(PlayerProfile.StartingBalance)} まで補充できます。", new Vector2(820, 420))
                .AddButton("LOBBY", GoToLobby)
                .AddButton("RELOAD", () =>
                {
                    session.Deposit(profiles.Reload(Rules.MinBet));
                    view.Hud.SetBalance(session.Available, true);
                    Sfx.Play(SfxKind.ChipsCollect);
                }, true);
            dialog.Closed += () => closed = true;
            Track(dialog);
            while (!closed)
                yield return null;
        }

        // ------------------------------------------------------------------
        // ラウンド
        // ------------------------------------------------------------------

        IEnumerator Deal()
        {
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
            view.PlayerPlate.SetFamily(Labels.Family(family));
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
            view.Status.Show("SHOWDOWN");

            view.PlayerPlate.SetScore(result.PlayerScore);
            view.DealerPlate.SetFamily(Labels.Family(result.DealerFamily));
            view.DealerPlate.SetScore(result.DealerScore);
            view.PlayerPlate.SetWinner(result.Winner == RoundWinner.Player);
            view.DealerPlate.SetWinner(result.Winner == RoundWinner.Dealer);
            view.Board.MarkLines(result.PlayerLines, Theme.PlayerBlue);
            view.Board.MarkLines(result.DealerLines, Theme.DealerRed);
            view.Board.DimExcept(CellsOf(result));
            yield return Tween.Wait(0.6f);

            view.Status.SetVisible(false);
            view.Result.Show(result, Round.Bets[BetSpot.Bonus] > 0);
            PlayResultSound(result);
            yield return PayOut(result);

            view.Scoreboard.Render(session.Scoreboard, Round.Shoe.ShoeNumber);
            view.Hud.SetBalance(session.Balance, true);
            yield return Tween.Wait(ResultSeconds);
        }

        IEnumerator PayOut(RoundResult result)
        {
            var bank = view.Shoe.transform.position;
            var wallet = view.Hud.BalanceWorldPosition;
            foreach (var bet in result.Bets)
            {
                var spot = view.Spots[bet.Spot];
                spot.ShowOutcome(bet.Outcome);
                if (bet.Outcome == BetOutcome.Win)
                {
                    view.Paytable.HighlightSpot(bet.Spot);
                    fx.FlyChips(bet.Net, bank, spot.Stack.TopWorldPosition);
                }
                else if (bet.Outcome == BetOutcome.Lose)
                {
                    fx.FlyChips(bet.Stake, spot.Stack.TopWorldPosition, bank);
                    spot.SetAmount(0);
                }
            }
            yield return Tween.Wait(TableFx.ChipFlightSeconds);

            bool anyReturn = false;
            foreach (var bet in result.Bets)
            {
                if (bet.Return <= 0)
                    continue;
                anyReturn = true;
                var spot = view.Spots[bet.Spot];
                spot.SetAmount(bet.Return);
                fx.FlyChips(bet.Return, spot.Stack.TopWorldPosition, wallet, 0.35f);
            }
            if (anyReturn)
            {
                Sfx.Play(SfxKind.ChipsCollect);
                yield return Tween.Wait(0.35f + TableFx.ChipFlightSeconds);
            }
            foreach (var spot in view.Spots.Values)
                spot.SetAmount(0);
        }

        IEnumerator Cleanup()
        {
            view.Result.Hide();
            view.Status.SetVisible(true);
            ResetTable();
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
            view.Result.Hide();
            view.Status.SetVisible(true);
            view.Status.HideTimer();
            ResetTable();
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

        void UpdateScores()
        {
            var values = Round.Board.ToValues();
            var family = Round.PlayerFamily;
            view.Board.UpdateSums(values, family);
            view.PlayerPlate.SetScore(Scoring.FamilyScore(values, family));
            view.DealerPlate.SetFamily(Labels.Family(Scoring.DealerFamily(values, family)));
            view.DealerPlate.SetScore(Scoring.DealerScore(values, family));
        }

        void ResetTable()
        {
            view.PlayerPlate.Clear();
            view.DealerPlate.Clear();
            view.Paytable.ClearHighlights();
            foreach (var spot in view.Spots.Values)
            {
                spot.ResetOutcome();
                spot.SetAmount(0);
            }
        }

        void RefreshTableInfo()
        {
            view.Hud.SetRoundInfo(Round.Shoe.ShoeNumber, Round.RoundNumber);
            view.Scoreboard.Render(session.Scoreboard, Round.Shoe.ShoeNumber);
            RefreshShoe();
        }

        void RefreshShoe() => view.Shoe.SetRemaining(Round.Shoe.Remaining, Round.Shoe.TotalCards);

        static HashSet<int> CellsOf(RoundResult result)
        {
            var cells = new HashSet<int>();
            foreach (var line in result.PlayerLines)
                cells.UnionWith(new[] { line.A, line.B, line.C });
            foreach (var line in result.DealerLines)
                cells.UnionWith(new[] { line.A, line.B, line.C });
            return cells;
        }

        static void PlayResultSound(RoundResult result)
        {
            if (result.Net > 0)
                Sfx.Play(result.Net >= result.TotalStake * 5 ? SfxKind.BigWin : SfxKind.Win);
            else if (result.Net < 0)
                Sfx.Play(SfxKind.Lose);
            else
                Sfx.Play(SfxKind.Push);
        }

        void Track(Dialog dialog)
        {
            openDialogs++;
            dialog.Closed += () => openDialogs--;
        }

        void GoToLobby()
        {
            // ベット受付中だけ離席できる。置いたチップはまだ引き落としていないのでそのまま手元に戻る。
            if (Round.Phase == RoundPhase.Betting || Round.Phase == RoundPhase.Complete)
                SceneManager.LoadScene(SceneNames.Lobby);
        }
    }
}
