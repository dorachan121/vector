using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Alchemia;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Alchemia
{
    /// <summary>
    /// ALCHEMIA の画面。中央に 6×5 の錬成盤、左に配当表と連鎖倍率、右に勝ち額。下に BET・SPIN・AUTO。
    /// 盤面の演出はエンジンが記録した連鎖の各段（融合 → 大錬成 → 落下 → 補充）をそのままなぞる。
    /// </summary>
    public sealed class AlchemiaView
    {
        const float Cell = 108f;
        const float Pitch = 114f;
        static readonly Vector2 GridCenter = new Vector2(0, 70);

        public static readonly Color Felt = Theme.Hex("2B1F5E");
        public static readonly Color FeltEdge = Theme.Hex("0A0720");
        static readonly Color ButtonColor = new Color(0.16f, 0.12f, 0.34f, 0.97f);

        readonly List<Image> ladder = new List<Image>();
        AlchemiaRules rules;
        SymbolView[] cells;
        RectTransform board;
        TextMeshProUGUI winAmount;
        TextMeshProUGUI cascadeText;
        TextMeshProUGUI lastWin;
        TextMeshProUGUI betLabel;
        TextMeshProUGUI banner;
        RectTransform controls;
        CasinoButton spin;
        CasinoButton auto;
        CasinoButton down;
        CasinoButton up;

        public CasinoView Casino { get; private set; }

        public event Action SpinClicked;
        public event Action AutoClicked;
        public event Action<int> BetStepped;

        public static AlchemiaView Build(AlchemiaRules rules)
        {
            var view = new AlchemiaView { rules = rules };
            var casino = CasinoView.Build("Alchemia", "ALCHEMIA", "VIDEO SLOT",
                $"BET {Labels.Money(rules.MinFor(AlchemiaRules.Spin))} – {Labels.Money(rules.MaxFor(AlchemiaRules.Spin))}",
                $"MATCH 3+ OF AN ELEMENT TO TRANSMUTE  ·  MAX WIN {rules.MaxWinMultiple:N0}x", Felt, FeltEdge);
            view.Casino = casino;
            casino.ShowBettingControls(false);
            ((RectTransform)casino.Status.transform).anchoredPosition = new Vector2(0, -262);
            view.BuildBoard(casino.Stage);
            view.BuildPaytable(casino.Stage);
            view.BuildMeter(casino.Stage);
            view.BuildControls(casino.Root);
            return view;
        }

        void BuildBoard(Transform stage)
        {
            var size = new Vector2(Pitch * rules.Columns + 24, Pitch * rules.Rows + 24);
            Ui.Panel(stage, "BoardPlate", GridCenter, size, new Color(0, 0, 0, 0.45f), 26);
            Ui.Outline(stage, "BoardOutline", GridCenter, size, Theme.Gold.WithAlpha(0.7f), 26);
            for (int i = 0; i < rules.Cells; i++)
                Ui.Panel(stage, "Slot" + i, CellPosition(i), Vector2.one * (Cell - 4), new Color(1, 1, 1, 0.05f), 16);
            board = Ui.Rect(stage, "Board", Vector2.zero, Vector2.zero);
            cells = new SymbolView[rules.Cells];
            ShowIdleBoard();
            banner = Ui.Text(stage, "Banner", "", Fonts.Display, 90, Theme.GoldBright, GridCenter, new Vector2(1000, 120));
            banner.outlineWidth = 0.2f;
            banner.outlineColor = new Color(0, 0, 0, 0.9f);
            banner.alpha = 0;
        }

        /// <summary>
        /// 最初のスピンまでの盤面（空のままだと台が止まって見えるため）。同じ元素が隣り合わない並びの素を置き、
        /// 当たりの形に見えないようにする。
        /// </summary>
        void ShowIdleBoard()
        {
            for (int i = 0; i < cells.Length; i++)
            {
                int col = i % rules.Columns, row = i / rules.Columns;
                var element = (Element)((col + 2 * row) % rules.ElementCount);
                cells[i] = SymbolView.Create(board, Cell, PositionOf(i));
                cells[i].Set(new Symbol(element, 1));
            }
        }

        void BuildPaytable(Transform stage)
        {
            var size = new Vector2(330, 580);
            var panel = PaytablePanel.Create(stage, new Vector2(-705, 70), size, "AlchemyTable");
            panel.Heading("TRANSMUTATION PAYS");
            for (int tier = 3; tier <= Symbol.MaxTier; tier++)
                panel.Row("t" + tier, $"{AlchemiaLabels.Tier(tier)}（位{tier}）を作る", AlchemiaLabels.Multiple(rules.CreationPay(tier)));
            panel.Row("grand", "大錬成", AlchemiaLabels.Multiple(rules.TransmutationPay));
            panel.Note("塊が4個で1.5倍、5個で2倍…");
            panel.Gap();
            panel.Heading("ELEMENTS");
            for (int e = 0; e < rules.ElementCount; e++)
            {
                var element = (Element)e;
                string hex = ColorUtility.ToHtmlStringRGB(AlchemiaLabels.ColorOf(element));
                panel.Row("e" + e, $"<color=#{hex}>●</color>  {AlchemiaLabels.Element(element)}  {element.ToString().ToUpperInvariant()}", "");
            }
            panel.Gap(4);
            panel.Heading("TIERS");
            panel.Note("素 → 晶 → 霊薬 → 秘宝 → 賢者の石");
            panel.Note("賢者の石がさらに融合すると大錬成");

            var ladderSize = new Vector2(330, 80);
            var ladderRoot = Ui.Rect(stage, "Ladder", new Vector2(705, -180), ladderSize);
            Ui.Panel(ladderRoot, "Plate", Vector2.zero, ladderSize, Theme.Panel, 18);
            Ui.Text(ladderRoot, "Heading", "CASCADE MULTIPLIER", Fonts.Label, 16, Theme.Gold, new Vector2(0, 26), new Vector2(300, 20));
            var multipliers = rules.CascadeMultipliers;
            for (int i = 0; i < multipliers.Count; i++)
            {
                var box = Ui.Panel(ladderRoot, "Step" + i, new Vector2((i - (multipliers.Count - 1) / 2f) * 50, -8), new Vector2(44, 34), new Color(1, 1, 1, 0.08f), 8);
                Ui.Text(box.transform, "x", "x" + multipliers[i] + (i == multipliers.Count - 1 ? "+" : ""), Fonts.Label, 17, Theme.Ivory, Vector2.zero, new Vector2(44, 34));
                ladder.Add(box);
            }
        }

        void BuildMeter(Transform stage)
        {
            var size = new Vector2(330, 360);
            var panel = Ui.Rect(stage, "Meter", new Vector2(705, 180), size);
            Ui.Panel(panel, "Plate", Vector2.zero, size, Theme.Panel, 20);
            Ui.Outline(panel, "Outline", Vector2.zero, size, Theme.PanelLine, 20);
            Ui.Text(panel, "Heading", "WIN", Fonts.Label, 26, Theme.Gold, new Vector2(0, 140), new Vector2(300, 34));
            winAmount = Ui.Text(panel, "Amount", "", Fonts.Display, 64, Theme.GoldBright, new Vector2(0, 70), new Vector2(310, 84));
            cascadeText = Ui.Text(panel, "Cascade", "", Fonts.Label, 22, Theme.Ivory, new Vector2(0, 0), new Vector2(300, 30));
            lastWin = Ui.Text(panel, "Last", "", Fonts.Label, 18, Theme.Muted, new Vector2(0, -60), new Vector2(300, 26));
            Ui.Paragraph(panel, "Note", "結果はスピンした瞬間に暗号論的乱数で決まり、演出はそれをなぞるだけです。", 15, Theme.Muted,
                new Vector2(0, -130), new Vector2(290, 70), TextAlignmentOptions.Top);
        }

        void BuildControls(Transform root)
        {
            controls = Ui.Rect(root, "SlotControls", new Vector2(0, CasinoView.BottomRow), new Vector2(1400, 110));
            down = Ui.Button(controls, "BetDown", "−", new Vector2(-640, 0), new Vector2(80, 80), ButtonColor, Theme.Ivory, () => BetStepped?.Invoke(-1), 44, 40);
            var betPlate = Ui.Panel(controls, "BetPlate", new Vector2(-480, 0), new Vector2(220, 80), new Color(0, 0, 0, 0.6f), 20);
            Ui.Text(betPlate.transform, "Caption", "BET", Fonts.Label, 16, Theme.Muted, new Vector2(0, 22), new Vector2(200, 20));
            betLabel = Ui.Text(betPlate.transform, "Bet", "", Fonts.Display, 38, Theme.GoldBright, new Vector2(0, -8), new Vector2(200, 46));
            up = Ui.Button(controls, "BetUp", "+", new Vector2(-320, 0), new Vector2(80, 80), ButtonColor, Theme.Ivory, () => BetStepped?.Invoke(1), 44, 40);
            spin = Ui.Button(controls, "Spin", "SPIN", Vector2.zero, new Vector2(320, 110), Theme.Gold, Theme.Ink, () => SpinClicked?.Invoke(), 52, 55);
            auto = Ui.Button(controls, "Auto", "AUTO 10", new Vector2(320, 0), new Vector2(220, 80), ButtonColor, Theme.GoldBright, () => AutoClicked?.Invoke(), 30, 20);
        }

        static Vector2 CellPosition(int cell, int columns = 6, int rows = 5)
        {
            int col = cell % columns, row = cell / columns;
            return GridCenter + new Vector2((col - (columns - 1) / 2f) * Pitch, ((rows - 1) / 2f - row) * Pitch);
        }

        Vector2 PositionOf(int cell) => CellPosition(cell, rules.Columns, rules.Rows);

        public void SetBet(long bet, bool canDown, bool canUp)
        {
            betLabel.text = Labels.Money(bet);
            down.Interactable = canDown;
            up.Interactable = canUp;
        }

        public void SetControls(bool betting, bool autoRunning, int autoLeft)
        {
            spin.Interactable = betting && !autoRunning;
            down.Interactable &= betting && !autoRunning;
            up.Interactable &= betting && !autoRunning;
            auto.SetLabel(autoRunning ? $"STOP ({autoLeft})" : "AUTO 10");
        }

        public void SetWin(long amount, string cascade)
        {
            winAmount.text = amount > 0 ? Labels.Money(amount) : "—";
            cascadeText.text = cascade;
        }

        public void SetLastWin(long amount, double multiple)
            => lastWin.text = amount > 0 ? $"LAST WIN {Labels.Money(amount)}  ({multiple:N2}x)" : "";

        void HighlightLadder(int step)
        {
            for (int i = 0; i < ladder.Count; i++)
            {
                bool on = step >= 0 && Math.Min(step, ladder.Count - 1) == i;
                ladder[i].color = on ? Theme.Gold.WithAlpha(0.7f) : new Color(1, 1, 1, 0.08f);
            }
        }

        /// <summary>前の盤面を片付け、新しい盤面を上から落とす。</summary>
        public IEnumerator DropIn(Symbol[] grid)
        {
            HighlightLadder(-1);
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                    continue;
                var old = cells[i];
                Tween.Move(old.Rect, old.Rect.anchoredPosition - new Vector2(0, 700), 0.25f, Ease.InCubic, () => UnityEngine.Object.Destroy(old.gameObject), (i % rules.Columns) * 0.02f);
                cells[i] = null;
            }
            yield return Tween.Wait(0.2f);
            for (int i = 0; i < grid.Length; i++)
            {
                var view = SymbolView.Create(board, Cell, PositionOf(i) + new Vector2(0, 700));
                view.Set(grid[i]);
                Tween.Move(view.Rect, PositionOf(i), 0.32f, Ease.OutCubic, null, (i % rules.Columns) * 0.04f + (rules.Rows - 1 - i / rules.Columns) * 0.02f);
                cells[i] = view;
            }
            Sfx.Play(SfxKind.CardDeal);
            yield return Tween.Wait(0.32f + rules.Columns * 0.04f + rules.Rows * 0.02f);
        }

        /// <summary>連鎖の1段を演出する。winSoFar はこの段の前までの勝ち額、bet は賭け金。</summary>
        public IEnumerator PlayStep(CascadeStep step, int index, long bet, long winSoFar)
        {
            HighlightLadder(index);
            foreach (var merge in step.Merges)
            {
                foreach (int cell in merge.Cells)
                    cells[cell]?.Highlight();
            }
            Sfx.Play(SfxKind.Sparkle, 0.6f);
            yield return Tween.Wait(0.28f);

            long win = winSoFar;
            foreach (var merge in step.Merges)
            {
                var anchor = PositionOf(merge.Anchor);
                foreach (int cell in merge.Cells)
                {
                    if (cell == merge.Anchor && !merge.IsTransmutation)
                        continue;
                    var view = cells[cell];
                    cells[cell] = null;
                    if (view == null)
                        continue;
                    Tween.Move(view.Rect, anchor, 0.22f, Ease.InCubic, () => UnityEngine.Object.Destroy(view.gameObject));
                    Tween.Scale(view.Rect, Vector3.one * 0.3f, 0.22f, Ease.InCubic);
                }
                if (!merge.IsTransmutation)
                {
                    var target = cells[merge.Anchor];
                    if (target != null)
                    {
                        target.Set(new Symbol(merge.Element, merge.NewTier));
                        target.Pop();
                    }
                }
                long amount = bet * merge.PayHundredths / 100;
                win += amount;
                if (amount > 0)
                    FloatingText.Show(board, "+" + Labels.Money(amount), AlchemiaLabels.ColorOf(merge.Element), anchor + new Vector2(0, 40), 40, 70);
                else if (merge.NewTier == 2)
                    FloatingText.Show(board, AlchemiaLabels.Tier(2), AlchemiaLabels.ColorOf(merge.Element), anchor + new Vector2(0, 30), 28, 40, 0.7f);
            }
            if (step.Merges.Count > 0)
                Sfx.Play(SfxKind.Chip, 0.6f, 1.3f);
            yield return Tween.Wait(0.3f);

            if (step.Dissolved.Count > 0)
            {
                Sfx.Play(SfxKind.BigWin);
                FloatingText.Shake(Casino.Stage, 24f, 0.6f);
                ShowBanner("GRAND TRANSMUTATION", Theme.GoldBright);
                foreach (int cell in step.Dissolved)
                {
                    var view = cells[cell];
                    cells[cell] = null;
                    if (view == null)
                        continue;
                    Tween.Scale(view.Rect, Vector3.one * 1.6f, 0.4f, Ease.OutCubic, () => UnityEngine.Object.Destroy(view.gameObject));
                    Tween.Fade(Ui.Group(view.gameObject), 0f, 0.4f);
                }
                yield return Tween.Wait(0.6f);
            }
            SetWin(win, $"CASCADE {index + 1}  ·  x{step.Multiplier}");

            // 落下（下の行から順に動かすので、移動先が先に空いている）
            foreach (var fall in step.Falls)
            {
                var view = cells[fall.From];
                cells[fall.From] = null;
                cells[fall.To] = view;
                if (view != null)
                    Tween.Move(view.Rect, PositionOf(fall.To), 0.22f, Ease.InCubic);
            }
            foreach (int cell in step.Spawned)
            {
                var view = SymbolView.Create(board, Cell, PositionOf(cell) + new Vector2(0, Pitch * rules.Rows));
                view.Set(step.After[cell]);
                Tween.Move(view.Rect, PositionOf(cell), 0.3f, Ease.OutCubic, null, 0.05f);
                cells[cell] = view;
            }
            yield return Tween.Wait(0.38f);

            // 記録どおりの盤面に揃える（演出のずれを残さない）
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                {
                    cells[i] = SymbolView.Create(board, Cell, PositionOf(i));
                }
                cells[i].Rect.anchoredPosition = PositionOf(i);
                cells[i].Rect.localScale = Vector3.one;
                cells[i].Set(step.After[i]);
            }
        }

        public void ShowBanner(string text, Color color)
        {
            banner.text = text;
            banner.color = color;
            banner.rectTransform.localScale = Vector3.one * 0.6f;
            Tween.Scale(banner.rectTransform, Vector3.one, 0.35f, Ease.OutBack);
            Tween.Run(banner, "fade", 2.2f, Ease.Linear, t => banner.alpha = t < 0.15f ? t / 0.15f : t > 0.8f ? (1 - t) / 0.2f : 1);
        }

        /// <summary>キーボード操作: Space / Enter で SPIN。</summary>
        public void PollKeys()
        {
            if (Dialog.IsAnyOpen || !spin.Interactable)
                return;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SpinClicked?.Invoke();
        }
    }
}
