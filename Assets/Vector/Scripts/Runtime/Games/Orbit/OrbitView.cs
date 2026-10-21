using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vector.Core.Orbit;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Orbit
{
    /// <summary>
    /// ORBIT の画面。左に天球儀、右に賭け先の表（内惑星の位置・外惑星の方角と天地・惑星の並びと彗星）と直近の結果。
    /// 進行の判断はしない（OrbitDirector から呼ばれるだけ）。
    /// </summary>
    public sealed class OrbitView
    {
        const int HistoryCount = 8;
        const float BoardX = 480f;
        const float CellPitch = 128f;
        static readonly Vector2 CellSize = new Vector2(116, 80);
        static readonly Vector2 WideCellSize = new Vector2(180, 84);
        static readonly Vector2 OrreryCenter = new Vector2(-450, 20);
        public static readonly Color SkyCenter = Theme.Hex("1B2152");
        public static readonly Color SkyEdge = Theme.Hex("05060F");

        readonly Dictionary<int, TicketView> tickets = new Dictionary<int, TicketView>();
        readonly List<string> history = new List<string>();
        TextMeshProUGUI historyText;

        public CasinoView Casino { get; private set; }
        public OrreryView Orrery { get; private set; }

        public static OrbitView Build(OrbitRules rules)
        {
            var view = new OrbitView();
            var casino = CasinoView.Build("Orbit", "ORBIT", "ORRERY ROULETTE",
                $"BET {Labels.Money(rules.MinBet)}–{Labels.Money(rules.MaxFor(OrbitSpots.Half(0)))}  ·  ALIGNMENT {OrbitLabels.Odds(rules, OrbitSpots.Alignment)}",
                $"ALL BETS PAY TRUE ODDS  ·  A COMET CROSSES 1 IN {OrbitRules.CometOneIn}", SkyCenter, SkyEdge);
            view.Casino = casino;
            casino.Actions.SetDealLabel("SPIN");
            ((RectTransform)casino.Status.transform).anchoredPosition = new Vector2(BoardX, -236);
            view.Orrery = OrreryView.Create(casino.Stage, OrreryCenter);
            // 負けたチップは太陽へ吸い込まれる
            casino.Bank.anchoredPosition = OrreryCenter;
            view.BuildBoard(casino, rules);
            view.BuildHistory(casino.Stage);
            return view;
        }

        void BuildBoard(CasinoView casino, OrbitRules rules)
        {
            var stage = casino.Stage;
            Heading(stage, 318, "INNER PLANET", "内惑星が止まる位置");
            for (int i = 0; i < OrbitSpin.InnerSlots; i++)
                AddTicket(casino, rules, OrbitSpots.Inner(i), Cell(i, 258), CellSize, $"内 {i + 1}", OrbitLabels.InnerColor);

            Heading(stage, 172, "OUTER PLANET", "外惑星の方角と天地");
            for (int q = 0; q < OrbitSpin.Quadrants; q++)
                AddTicket(casino, rules, OrbitSpots.Quadrant(q), Cell(q, 112), CellSize, OrbitLabels.Quadrant(q), OrbitLabels.OuterColor);
            AddTicket(casino, rules, OrbitSpots.Half(0), Cell(4, 112), CellSize, OrbitLabels.Half(0), Palette.GoldLight);
            AddTicket(casino, rules, OrbitSpots.Half(1), Cell(5, 112), CellSize, OrbitLabels.Half(1), Palette.Slate);

            Heading(stage, 26, "ALIGNMENT", "惑星の並びと彗星");
            var specials = new[]
            {
                (OrbitSpots.InnerConjunction, "内合", OrbitLabels.InnerColor),
                (OrbitSpots.OuterConjunction, "外合", OrbitLabels.MiddleColor),
                (OrbitSpots.Alignment, "惑星直列", Palette.Gold),
                (OrbitSpots.Comet, "彗星", OrbitLabels.CometColor),
            };
            for (int s = 0; s < specials.Length; s++)
            {
                var (spot, label, accent) = specials[s];
                var at = new Vector2(BoardX + (s - 1.5f) * (WideCellSize.x + 12), -36);
                AddTicket(casino, rules, spot, at, WideCellSize, label, accent);
            }
        }

        static Vector2 Cell(int column, float y) => new Vector2(BoardX + (column - 2.5f) * CellPitch, y);

        static void Heading(Transform stage, float y, string title, string note)
        {
            Ui.Text(stage, "Heading", title, Fonts.Label, 20, Theme.Gold, new Vector2(BoardX - 280, y), new Vector2(200, 26), TextAlignmentOptions.Left);
            Ui.Text(stage, "Note", note, Fonts.Body, 18, Theme.Muted, new Vector2(BoardX + 40, y), new Vector2(440, 26), TextAlignmentOptions.Left);
        }

        void AddTicket(CasinoView casino, OrbitRules rules, int spot, Vector2 position, Vector2 size, string label, Color accent)
        {
            var ticket = TicketView.Create(casino.Stage, spot, position, size, label, accent, Theme.Ivory);
            ticket.SetOdds(OrbitLabels.Odds(rules, spot), true);
            tickets[spot] = ticket;
            casino.AddTarget(ticket);
        }

        void BuildHistory(Transform stage)
        {
            Ui.Text(stage, "HistoryTitle", "LAST SPINS", Fonts.Label, 18, Theme.Gold, new Vector2(BoardX - 330, -132), new Vector2(140, 24), TextAlignmentOptions.Left);
            historyText = Ui.Text(stage, "History", "<color=#A7B5AC>スピンの結果がここに並びます</color>", Fonts.Body, 19, Theme.Ivory,
                new Vector2(BoardX + 70, -132), new Vector2(660, 26), TextAlignmentOptions.Left);
            historyText.richText = true;
        }

        /// <summary>当たったマスを（賭けていなくても）光らせる。</summary>
        public void ShowWinners(OrbitSpin spin)
        {
            foreach (var pair in tickets)
                pair.Value.Highlight(spin.Wins(pair.Key));
        }

        public void ClearWinners()
        {
            foreach (var ticket in tickets.Values)
                ticket.Highlight(false);
        }

        /// <summary>直近の結果を左から新しい順に並べる。</summary>
        public void AddHistory(OrbitSpin spin)
        {
            history.Insert(0, OrbitLabels.Short(spin));
            if (history.Count > HistoryCount)
                history.RemoveAt(history.Count - 1);
            historyText.text = string.Join("  ·  ", history);
        }
    }
}
