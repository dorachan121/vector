using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>
    /// 出目表（ビーズロード）。バカラの罫線と同じく上から下、左から右へ勝者の色の玉を並べる。
    /// </summary>
    public sealed class ScoreboardView : MonoBehaviour
    {
        const int Rows = 6;
        const int Columns = 9;
        const float Bead = 34f;

        readonly List<Image> beads = new List<Image>();
        readonly List<TextMeshProUGUI> letters = new List<TextMeshProUGUI>();
        TextMeshProUGUI title;
        TextMeshProUGUI counts;

        public static ScoreboardView Create(Transform parent, Vector2 position, Vector2 size)
        {
            var rt = Ui.Rect(parent, "Scoreboard", position, size);
            var view = rt.gameObject.AddComponent<ScoreboardView>();
            Ui.Panel(rt, "Plate", Vector2.zero, size, Theme.Panel, 20);
            Ui.Outline(rt, "Outline", Vector2.zero, size, Theme.PanelLine, 20);
            view.title = Ui.Text(rt, "Title", "", Fonts.Label, 24, Theme.Gold, new Vector2(0, size.y / 2 - 30), new Vector2(size.x, 32));

            var grid = Ui.Rect(rt, "Grid", new Vector2(0, size.y / 2 - 70 - Rows * Bead / 2), new Vector2(Columns * Bead, Rows * Bead));
            for (int c = 0; c < Columns; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    var p = new Vector2((c - (Columns - 1) / 2f) * Bead, ((Rows - 1) / 2f - r) * Bead);
                    Ui.Image(grid, "Cell", Art.RoundedOutline(32, 3f), Theme.PanelLine.WithAlpha(0.2f), p, Vector2.one * Bead).pixelsPerUnitMultiplier = 32f / 4f;
                    var bead = Ui.Image(grid, "Bead", Art.Circle, Color.clear, p, Vector2.one * (Bead - 6));
                    view.beads.Add(bead);
                    view.letters.Add(Ui.Text(bead.transform, "L", "", Fonts.Label, 16, Color.white, Vector2.zero, Vector2.one * Bead));
                }
            }
            view.counts = Ui.Text(rt, "Counts", "", Fonts.Label, 20, Theme.Ivory, new Vector2(0, grid.anchoredPosition.y - Rows * Bead / 2 - 26), new Vector2(size.x, 30));
            view.counts.richText = true;
            return view;
        }

        public void Render(Scoreboard scoreboard, int shoeNumber)
        {
            title.text = $"SCOREBOARD  ·  SHOE #{shoeNumber}";
            var entries = scoreboard.Entries;
            int capacity = beads.Count;
            int start = entries.Count > capacity ? (entries.Count - capacity + Rows - 1) / Rows * Rows : 0;
            for (int i = 0; i < capacity; i++)
            {
                int index = start + i;
                bool filled = index < entries.Count;
                beads[i].color = filled ? Theme.ColorFor(entries[index]) : Color.clear;
                letters[i].text = filled ? Letter(entries[index]) : "";
            }
            counts.text = $"<color=#{Hex(Theme.PlayerBlue)}>PLAYER {scoreboard.Count(RoundWinner.Player)}</color>    " +
                          $"<color=#{Hex(Theme.DealerRed)}>DEALER {scoreboard.Count(RoundWinner.Dealer)}</color>    " +
                          $"<color=#{Hex(Theme.TieGreen)}>TIE {scoreboard.Count(RoundWinner.Tie)}</color>";
        }

        static string Letter(RoundWinner winner) => winner == RoundWinner.Player ? "P" : winner == RoundWinner.Dealer ? "D" : "T";

        static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
    }
}
