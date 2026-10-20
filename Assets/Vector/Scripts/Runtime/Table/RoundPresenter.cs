using System.Collections;
using System.Collections.Generic;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>
    /// ラウンドの状態を画面に映す役。配置中の得点表示、勝負の結果、チップの払い戻し演出、片付けを担当する。
    /// 進行の判断はしない（TableDirector から呼ばれるだけ）。
    /// </summary>
    public sealed class RoundPresenter
    {
        /// <summary>この倍以上の勝ちは大勝ちの効果音にする。</summary>
        const long BigWinMultiple = 5;

        readonly TableView view;
        readonly TableFx fx;

        public RoundPresenter(TableView view, TableFx fx)
        {
            this.view = view;
            this.fx = fx;
        }

        /// <summary>配置中のライン合計と両者の暫定得点を表示する。</summary>
        public void ShowScores(int[] values, LineFamily playerFamily)
        {
            view.Board.UpdateSums(values, playerFamily);
            view.PlayerPlate.SetFamily(Labels.Family(playerFamily));
            view.PlayerPlate.SetScore(Scoring.FamilyScore(values, playerFamily));
            view.DealerPlate.SetFamily(Labels.Family(Scoring.DealerFamily(values, playerFamily)));
            view.DealerPlate.SetScore(Scoring.DealerScore(values, playerFamily));
        }

        /// <summary>勝負を決めたラインを示し、結果を表示してチップを精算する。</summary>
        public IEnumerator ShowResult(RoundResult result, bool hasBonusBet)
        {
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
            view.Result.Show(result, hasBonusBet);
            PlayResultSound(result);
            yield return PayOut(result);
        }

        /// <summary>結果表示を片付け、次のラウンドの状態に戻す。</summary>
        public void Reset()
        {
            view.Result.Hide();
            view.Status.SetVisible(true);
            view.PlayerPlate.Clear();
            view.DealerPlate.Clear();
            view.Paytable.ClearHighlights();
            foreach (var spot in view.Spots.Values)
            {
                spot.ResetOutcome();
                spot.SetAmount(0);
            }
        }

        /// <summary>負けたチップはディーラーへ、勝ちは配当を足してから手元へ飛ばす。</summary>
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

        static HashSet<int> CellsOf(RoundResult result)
        {
            var cells = new HashSet<int>();
            foreach (var lines in new[] { result.PlayerLines, result.DealerLines })
            {
                foreach (var line in lines)
                    cells.UnionWith(new[] { line.A, line.B, line.C });
            }
            return cells;
        }

        static void PlayResultSound(RoundResult result)
        {
            if (result.Net > 0)
                Sfx.Play(result.Net >= result.TotalStake * BigWinMultiple ? SfxKind.BigWin : SfxKind.Win);
            else if (result.Net < 0)
                Sfx.Play(SfxKind.Lose);
            else
                Sfx.Play(SfxKind.Push);
        }
    }
}
