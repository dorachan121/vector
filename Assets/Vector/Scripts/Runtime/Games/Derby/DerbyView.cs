using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Derby;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Derby
{
    /// <summary>
    /// FOUR BEASTS DERBY の画面。上にコース（4レーン）と引いたカード、下にオッズ表（単勝4つと馬単の4×4）。
    /// 進行の判断はしない（DerbyDirector から呼ばれるだけ）。
    /// </summary>
    public sealed class DerbyView
    {
        const int Runners = DerbyTickets.Runners;
        const float TrackLeft = -600f;
        const float TrackRight = 560f;
        const float FirstLane = 300f;
        const float LaneStep = 60f;
        const float TokenSize = 50f;
        const int HistoryLines = 6;

        public static readonly Color Turf = Theme.Hex("1F5A2E");
        public static readonly Color TurfEdge = Theme.Hex("081A0D");

        readonly RectTransform[] tokens = new RectTransform[Runners];
        readonly TextMeshProUGUI[] live = new TextMeshProUGUI[Runners];
        readonly Image[] liveBars = new Image[Runners];
        readonly TextMeshProUGUI[] places = new TextMeshProUGUI[Runners];
        readonly TextMeshProUGUI[] gateNotes = new TextMeshProUGUI[Runners];
        readonly List<GameObject> marks = new List<GameObject>();
        readonly Dictionary<int, TicketView> tickets = new Dictionary<int, TicketView>();
        readonly List<string> history = new List<string>();
        RectTransform track;
        TextMeshProUGUI header;
        TextMeshProUGUI historyText;
        CardView drawn;
        int distance;

        public CasinoView Casino { get; private set; }

        public static DerbyView Build(DerbyRules rules)
        {
            var view = new DerbyView();
            var casino = CasinoView.Build("Derby", "DERBY", "FOUR BEASTS",
                $"WIN {Labels.Money(rules.WinMinBet)}–{Labels.Money(rules.WinMaxBet)}  ·  EXACTA {Labels.Money(rules.ExactaMinBet)}–{Labels.Money(rules.ExactaMaxBet)}",
                null, Turf, TurfEdge);
            view.Casino = casino;
            casino.Actions.SetDealLabel("START");
            ((RectTransform)casino.Status.transform).anchoredPosition = new Vector2(0, 58);
            casino.Bank.anchoredPosition = new Vector2(380, 392);
            view.BuildHeader(casino.Stage);
            view.BuildTrack(casino.Stage);
            view.BuildBoard(casino);
            view.BuildPanels(casino.Stage, rules);
            return view;
        }

        void BuildHeader(Transform stage)
        {
            header = Ui.Text(stage, "Header", "", Fonts.Label, 26, Theme.Ivory, new Vector2(-300, 392), new Vector2(800, 34));
            for (int i = 0; i < 3; i++)
                Ui.Image(stage, "Deck" + i, Art.CardBack, Color.white, new Vector2(300 + i * 2, 392 + i * 2), CardView.Size * 0.7f);
            Ui.Text(stage, "DeckLabel", "52 CARDS", Fonts.Label, 14, Theme.FeltPrint, new Vector2(300, 336), new Vector2(120, 18));
        }

        void BuildTrack(Transform stage)
        {
            for (int r = 0; r < Runners; r++)
            {
                float y = FirstLane - r * LaneStep;
                Ui.Panel(stage, "Lane" + r, new Vector2((TrackLeft + TrackRight) / 2, y), new Vector2(TrackRight - TrackLeft + 60, LaneStep - 6),
                    new Color(0, 0, 0, r % 2 == 0 ? 0.28f : 0.18f), 12);
                var badge = Ui.Panel(stage, "Badge" + r, new Vector2(-790, y), new Vector2(54, 46), DerbyLabels.ColorOf(r), 10);
                Ui.Text(badge.transform, "Glyph", DerbyLabels.Glyph(r), Fonts.Body, 30, DerbyLabels.InkOf(r), Vector2.zero, new Vector2(54, 46));
                Ui.Text(stage, "Name" + r, $"{DerbyLabels.Kanji(r)} {DerbyLabels.Suit(r)}", Fonts.Body, 22, Theme.Ivory, new Vector2(-690, y + 9), new Vector2(130, 28));
                gateNotes[r] = Ui.Text(stage, "Gate" + r, "", Fonts.Label, 15, Theme.Muted, new Vector2(-690, y - 15), new Vector2(130, 20));
                live[r] = Ui.Text(stage, "Live" + r, "", Fonts.Label, 22, Theme.Ivory, new Vector2(735, y + 8), new Vector2(150, 28));
                Ui.Panel(stage, "LiveBack" + r, new Vector2(735, y - 16), new Vector2(150, 8), new Color(1, 1, 1, 0.12f), 4);
                liveBars[r] = Ui.Panel(stage, "LiveBar" + r, new Vector2(735, y - 16), new Vector2(150, 8), DerbyLabels.ColorOf(r), 4);
                liveBars[r].type = Image.Type.Filled;
                liveBars[r].fillMethod = Image.FillMethod.Horizontal;
                places[r] = Ui.Text(stage, "Place" + r, "", Fonts.Display, 28, Theme.GoldBright, new Vector2(TrackRight + 54, y), new Vector2(56, 40));
            }
            // 距離の目盛りとゲートの印を置くレイヤー（レーンの上、神獣の下）
            track = Ui.Rect(stage, "Track", Vector2.zero, Vector2.zero);

            // ゴールの市松模様（8段×2列）
            const int rows = 8;
            float height = LaneStep * Runners - 6;
            float square = height / rows;
            float topEdge = FirstLane + LaneStep / 2 - 3;
            for (int k = 0; k < rows; k++)
            {
                for (int c = 0; c < 2; c++)
                {
                    var color = (k + c) % 2 == 0 ? Theme.Ivory : Theme.Ink;
                    Ui.Image(stage, "Finish", null, color, new Vector2(TrackRight + 7 + c * 14, topEdge - square * (k + 0.5f)), new Vector2(14, square));
                }
            }
            Ui.Text(stage, "Goal", "GOAL", Fonts.Label, 16, Theme.Ivory, new Vector2(TrackRight + 14, FirstLane + 42), new Vector2(80, 20));
            for (int r = 0; r < Runners; r++)
            {
                var token = Ui.Rect(stage, "Token" + r, new Vector2(TrackLeft, FirstLane - r * LaneStep), Vector2.one * TokenSize);
                Ui.Image(token, "Glow", Art.Glow, DerbyLabels.ColorOf(r).WithAlpha(0.5f), Vector2.zero, Vector2.one * TokenSize * 1.9f);
                Ui.Image(token, "Body", Art.Circle, DerbyLabels.ColorOf(r), Vector2.zero, Vector2.one * TokenSize);
                Ui.Image(token, "Rim", Art.Ring(0.08f), Theme.Gold, Vector2.zero, Vector2.one * TokenSize);
                Ui.Text(token, "Glyph", DerbyLabels.Glyph(r), Fonts.Body, 28, DerbyLabels.InkOf(r), Vector2.zero, Vector2.one * TokenSize);
                tokens[r] = token;
            }
        }

        void BuildBoard(CasinoView casino)
        {
            var stage = casino.Stage;
            Ui.Text(stage, "WinTitle", "WIN  単勝", Fonts.Label, 20, Theme.Gold, new Vector2(-500, -6), new Vector2(160, 26));
            for (int r = 0; r < Runners; r++)
            {
                int spot = DerbyTickets.Win(r);
                var ticket = TicketView.Create(stage, spot, new Vector2(-300 + r * 200, -8), new Vector2(186, 64),
                    $"{DerbyLabels.Kanji(r)} {DerbyLabels.Suit(r)}", DerbyLabels.ColorOf(r), Theme.Ivory);
                tickets[spot] = ticket;
                casino.AddTarget(ticket);
            }

            const float cellW = 140f, cellH = 44f, top = -102f;
            // 左の説明パネルに重ならないよう、表の左上の空いたマスに2行で置く
            Ui.Text(stage, "ExactaTitle", "EXACTA\n馬単（1着→2着）", Fonts.Label, 17, Theme.Gold, new Vector2(-318, top + 18), new Vector2(150, 48));
            for (int s = 0; s < Runners; s++)
                Ui.Text(stage, "Col" + s, "2着 " + DerbyLabels.Kanji(s), Fonts.Body, 17, DerbyLabels.LabelColorOf(s),
                    new Vector2(-150 + s * (cellW + 6), top + 4), new Vector2(cellW, 22));
            for (int f = 0; f < Runners; f++)
            {
                float y = top - 26 - f * (cellH + 4);
                Ui.Text(stage, "Row" + f, "1着 " + DerbyLabels.Kanji(f), Fonts.Body, 17, DerbyLabels.LabelColorOf(f),
                    new Vector2(-310, y), new Vector2(120, cellH));
                for (int s = 0; s < Runners; s++)
                {
                    if (s == f)
                    {
                        Ui.Panel(stage, $"Diag{f}", new Vector2(-150 + s * (cellW + 6), y), new Vector2(cellW, cellH), new Color(0, 0, 0, 0.2f), 10);
                        continue;
                    }
                    int spot = DerbyTickets.Exacta(f, s);
                    var ticket = TicketView.Create(stage, spot, new Vector2(-150 + s * (cellW + 6), y), new Vector2(cellW, cellH), null, Theme.Gold, Theme.Ivory);
                    tickets[spot] = ticket;
                    casino.AddTarget(ticket);
                }
            }
        }

        void BuildPanels(Transform stage, DerbyRules rules)
        {
            var size = new Vector2(330, 250);
            var info = Ui.Rect(stage, "Info", new Vector2(-705, -175), size);
            Ui.Panel(info, "Plate", Vector2.zero, size, Theme.Panel, 20);
            Ui.Outline(info, "Outline", Vector2.zero, size, Theme.PanelLine, 20);
            Ui.Text(info, "Heading", "HOW IT RUNS", Fonts.Label, 22, Theme.Gold, new Vector2(0, 98), new Vector2(300, 30));
            Ui.Paragraph(info, "Body",
                "1組52枚を1枚ずつ引き、引いたスートの神獣が1歩進みます。距離とゲートはレースごとに抽選。\n" +
                $"オッズは着順の厳密な確率から、払戻率 {rules.TargetReturn:P0} を 0.1倍単位で切り捨てて決めています。",
                16, Theme.Ivory, new Vector2(0, -20), new Vector2(296, 190)).richText = true;

            var results = Ui.Rect(stage, "Results", new Vector2(705, -175), size);
            Ui.Panel(results, "Plate", Vector2.zero, size, Theme.Panel, 20);
            Ui.Outline(results, "Outline", Vector2.zero, size, Theme.PanelLine, 20);
            Ui.Text(results, "Heading", "RESULTS", Fonts.Label, 22, Theme.Gold, new Vector2(0, 98), new Vector2(300, 30));
            historyText = Ui.Paragraph(results, "Lines", "<color=#A7B5AC>レースの結果がここに並びます</color>", 18, Theme.Ivory, new Vector2(0, -20), new Vector2(296, 190));
            historyText.richText = true;
            historyText.lineSpacing = 0;
        }

        static float XOf(int position, int distance) => TrackLeft + (TrackRight - TrackLeft) * position / distance;

        /// <summary>出走表を表示する（ゲート位置・距離・オッズ）。</summary>
        public void ShowRace(RaceCard card, int raceNumber)
        {
            distance = card.Distance;
            header.text = $"RACE {raceNumber}  ·  DISTANCE {card.Distance}";
            foreach (var mark in marks)
                Object.Destroy(mark);
            marks.Clear();
            for (int p = 1; p < card.Distance; p++)
            {
                for (int r = 0; r < Runners; r++)
                    marks.Add(Ui.Image(track, "Tick", Art.Circle, Theme.FeltPrint.WithAlpha(0.35f), new Vector2(XOf(p, distance), FirstLane - r * LaneStep), new Vector2(8, 8)).gameObject);
            }
            for (int r = 0; r < Runners; r++)
            {
                int gate = card.Gates[r];
                gateNotes[r].text = gate > 0 ? $"GATE +{gate}" : "GATE 0";
                if (gate > 0)
                    marks.Add(Ui.Panel(track, "GateBar", new Vector2(XOf(gate, distance) - 28, FirstLane - r * LaneStep), new Vector2(6, LaneStep - 14), Theme.Gold, 3).gameObject);
                tokens[r].localScale = Vector3.one;
                Tween.Move(tokens[r], new Vector2(XOf(gate, distance), FirstLane - r * LaneStep), 0.4f, Ease.OutCubic);
                places[r].text = "";
            }
            ShowLive(card.Probabilities, false);
            foreach (var pair in tickets)
            {
                int tenths = card.OddsTenths(pair.Key);
                pair.Value.SetOdds(DerbyLabels.Odds(tenths), tenths > 0);
                pair.Value.Highlight(false);
            }
            if (drawn != null)
                Object.Destroy(drawn.gameObject);
            drawn = null;
        }

        /// <summary>着順の確率（単勝、1着が決まってからは2着）を各レーンの右に出す。</summary>
        public void ShowLive(RaceProbabilities p, bool secondPlace, int first = -1)
        {
            for (int r = 0; r < Runners; r++)
            {
                double value = secondPlace ? (r == first ? 0 : p.Exacta[first, r]) : p.Win[r];
                live[r].text = secondPlace && r == first ? "1着" : (secondPlace ? "2着 " : "WIN ") + value.ToString("P0");
                liveBars[r].fillAmount = (float)value;
            }
        }

        /// <summary>カードを1枚引いて見せ、その神獣を進める。</summary>
        public IEnumerator Step(RaceStep step, float seconds)
        {
            if (drawn != null)
                Object.Destroy(drawn.gameObject);
            drawn = CardView.Create(Casino.Stage, "Drawn", new Vector2(300, 392));
            drawn.SetCard(step.Card);
            drawn.Rect.localScale = Vector3.one * 0.7f;
            Tween.Move(drawn.Rect, new Vector2(420, 392), seconds * 0.4f);
            drawn.Flip(true, seconds * 0.4f);
            Sfx.Play(SfxKind.CardDeal, 0.5f);
            yield return Tween.Wait(seconds * 0.45f);
            int r = step.Runner;
            if (step.Moved)
            {
                var target = new Vector2(XOf(step.Position, distance), FirstLane - r * LaneStep);
                Tween.Move(tokens[r], target, seconds * 0.5f, Ease.OutBack);
                Tween.Scale(tokens[r], Vector3.one * 1.18f, seconds * 0.25f, Ease.OutQuad, () => Tween.Scale(tokens[r], Vector3.one, seconds * 0.25f));
                Sfx.Play(SfxKind.Chip, 0.35f, 0.8f + 0.1f * r);
            }
            else
            {
                drawn.SetDimmed(true);
            }
            if (step.FinishedPlace > 0)
            {
                places[r].text = step.FinishedPlace == 1 ? "1st" : "2nd";
                Sfx.Play(step.FinishedPlace == 1 ? SfxKind.Win : SfxKind.Sparkle, 0.7f);
                FloatingText.Show(Casino.Stage, $"{step.FinishedPlace}着 {DerbyLabels.Kanji(r)}", DerbyLabels.LabelColorOf(r),
                    new Vector2(TrackRight - 120, FirstLane - r * LaneStep + 20), 46, 50);
            }
            yield return Tween.Wait(seconds * 0.55f);
        }

        /// <summary>的中した馬券（単勝と馬単）を光らせる。</summary>
        public void HighlightWinners(RaceResult result)
        {
            tickets[DerbyTickets.Win(result.First)].Highlight(true);
            tickets[result.WinningExacta].Highlight(true);
        }

        public void AddHistory(RaceResult result, int raceNumber)
        {
            int win = result.Card.OddsTenths(DerbyTickets.Win(result.First));
            int exacta = result.Card.OddsTenths(result.WinningExacta);
            history.Insert(0, $"R{raceNumber}  {DerbyLabels.Kanji(result.First)}→{DerbyLabels.Kanji(result.Second)}  " +
                              $"<color=#FFE7A3>{DerbyLabels.Odds(win)} / {DerbyLabels.Odds(exacta)}</color>");
            if (history.Count > HistoryLines)
                history.RemoveAt(history.Count - 1);
            historyText.text = string.Join("\n", history);
        }
    }
}
