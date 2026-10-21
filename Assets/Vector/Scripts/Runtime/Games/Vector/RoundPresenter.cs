using System.Collections;
using System.Collections.Generic;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>
    /// ラウンドの状態を画面に映す役。配置中の得点表示、勝負の結果（勝ったライン・得点・結果の帯・配当表の強調）、片付けを担当する。
    /// 精算とチップの演出は CasinoDirector、進行の判断は TableDirector が受け持つ。
    /// </summary>
    public sealed class RoundPresenter
    {
        readonly TableView view;

        public RoundPresenter(TableView view) => this.view = view;

        /// <summary>配置中のライン合計と両者の暫定得点を表示する。</summary>
        public void ShowScores(int[] values, LineFamily playerFamily)
        {
            view.Board.UpdateSums(values, playerFamily);
            view.PlayerPlate.SetFamily(Labels.Family(playerFamily));
            view.PlayerPlate.SetScore(Scoring.FamilyScore(values, playerFamily));
            view.DealerPlate.SetFamily(Labels.Family(Scoring.DealerFamily(values, playerFamily)));
            view.DealerPlate.SetScore(Scoring.DealerScore(values, playerFamily));
        }

        /// <summary>勝負を決めたラインを示し、結果の帯と勝ったベットの配当を表示する。</summary>
        public IEnumerator ShowResult(RoundResult result, bool hasBonusBet)
        {
            view.Casino.Status.Show("SHOWDOWN");
            view.PlayerPlate.SetScore(result.PlayerScore);
            view.DealerPlate.SetFamily(Labels.Family(result.DealerFamily));
            view.DealerPlate.SetScore(result.DealerScore);
            view.PlayerPlate.SetWinner(result.Winner == RoundWinner.Player);
            view.DealerPlate.SetWinner(result.Winner == RoundWinner.Dealer);
            view.Board.MarkLines(result.PlayerLines, Theme.PlayerBlue);
            view.Board.MarkLines(result.DealerLines, Theme.DealerRed);
            view.Board.DimExcept(CellsOf(result));
            yield return Tween.Wait(0.6f);

            view.Casino.Status.SetVisible(false);
            view.Result.Show(result, hasBonusBet);
            foreach (var bet in result.Bets)
            {
                if (bet.Outcome == BetOutcome.Win)
                    view.Paytable.Highlight(VectorPaytable.Key((BetSpot)bet.Spot));
            }
        }

        /// <summary>結果表示を片付け、次のラウンドの状態に戻す。</summary>
        public void Reset()
        {
            view.Result.Hide();
            view.Picker.Hide();
            view.Casino.Status.SetVisible(true);
            view.PlayerPlate.Clear();
            view.DealerPlate.Clear();
            view.Paytable.ClearHighlights();
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
    }
}
