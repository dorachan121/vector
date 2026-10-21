using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Delve;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Delve
{
    /// <summary>
    /// DELVE の画面。中央に今の深さの扉、左に深さごとの倍率の梯子、右に引き返したときの払い戻し。
    /// 進行の判断はしない（DelveDirector から呼ばれるだけ）。
    /// </summary>
    public sealed class DelveView
    {
        const float DoorRow = 110f;
        const float DoorSpacing = 230f;
        const int MaxDoors = 4;

        public static readonly Color Stone = Theme.Hex("3B3530");
        public static readonly Color StoneEdge = Theme.Hex("0D0B09");

        readonly DoorView[] doors = new DoorView[MaxDoors];
        readonly List<(Image plate, TextMeshProUGUI label, TextMeshProUGUI pays)> ladder = new List<(Image, TextMeshProUGUI, TextMeshProUGUI)>();
        TextMeshProUGUI depthTitle;
        TextMeshProUGUI depthDetail;
        TextMeshProUGUI multiplier;
        TextMeshProUGUI payout;
        TextMeshProUGUI next;
        CasinoButton cashOut;
        RectTransform controls;
        int doorCount;

        public CasinoView Casino { get; private set; }

        public event Action<int> DoorChosen;
        public event Action CashOutClicked;

        public static DelveView Build(DelveRules rules)
        {
            var view = new DelveView();
            var casino = CasinoView.Build("Delve", "DELVE", "DUNGEON",
                $"TABLE LIMITS  {Labels.Money(rules.MinBet)} – {Labels.Money(rules.MaxBet)}",
                $"CHOOSE A DOOR  ·  CASH OUT ANY TIME  ·  DEPTH {rules.Depths} PAYS x{rules.MultiplierHundredths(rules.Depths) / 100.0:N2}", Stone, StoneEdge);
            view.Casino = casino;
            casino.Actions.SetDealLabel("DELVE");
            casino.Bank.anchoredPosition = new Vector2(0, DoorRow);
            view.BuildTorches(casino.Stage);
            view.BuildDoors(casino.Stage);
            view.BuildLadder(casino.Stage, rules);
            view.BuildPurse(casino.Stage);
            casino.AddSpot(DelveRules.Main, new Vector2(0, -300), new Vector2(170, 170), true, "STAKE", "ANY DEPTH", Theme.Gold);
            view.BuildControls(casino.Root);
            view.ShowFloor(rules, 0, false);
            return view;
        }

        void BuildTorches(Transform stage)
        {
            foreach (float x in new[] { -480f, 480f })
            {
                var torch = Ui.Image(stage, "Torch", Art.Glow, Theme.Hex("FF9A3C").WithAlpha(0.45f), new Vector2(x, 250), new Vector2(240, 240));
                Flicker.Add(torch);
                Ui.Panel(stage, "Sconce", new Vector2(x, 210), new Vector2(26, 60), Theme.Hex("2A2420"), 6);
            }
        }

        void BuildDoors(Transform stage)
        {
            depthTitle = Ui.Text(stage, "DepthTitle", "", Fonts.Display, 46, Theme.GoldBright, new Vector2(0, 318), new Vector2(700, 56));
            depthDetail = Ui.Text(stage, "DepthDetail", "", Fonts.Body, 20, Theme.Ivory.WithAlpha(0.85f), new Vector2(0, 274), new Vector2(700, 28));
            for (int i = 0; i < MaxDoors; i++)
            {
                int index = i;
                doors[i] = DoorView.Create(stage, "Door" + i, new Vector2(0, DoorRow));
                doors[i].Clicked += () => DoorChosen?.Invoke(index);
                Ui.Text(doors[i].transform, "Key", (i + 1).ToString(), Fonts.Label, 18, Theme.Ivory.WithAlpha(0.4f), new Vector2(0, -DoorView.Size.y / 2 - 14), new Vector2(30, 22));
            }
        }

        void BuildLadder(Transform stage, DelveRules rules)
        {
            var size = new Vector2(330, 520);
            var panel = Ui.Rect(stage, "Ladder", new Vector2(-705, 60), size);
            Ui.Panel(panel, "Plate", Vector2.zero, size, Theme.Panel, 20);
            Ui.Outline(panel, "Outline", Vector2.zero, size, Theme.PanelLine, 20);
            Ui.Text(panel, "Heading", "DEPTH  ·  DOORS  ·  PAYS", Fonts.Label, 20, Theme.Gold, new Vector2(0, size.y / 2 - 28), new Vector2(300, 28));
            const float row = 44f;
            for (int d = rules.Depths; d >= 1; d--)
            {
                float y = size.y / 2 - 70 - (rules.Depths - d) * row;
                var floor = rules.Floors[d - 1];
                var plate = Ui.Panel(panel, "Row" + d, new Vector2(0, y), new Vector2(size.x - 24, row - 6), Color.clear, 10);
                var label = Ui.Text(plate.transform, "Label", $"B{d}   {floor.Doors} 扉 / 魔物 {floor.Monsters}", Fonts.Label, 19, Theme.Ivory,
                    new Vector2(-50, 0), new Vector2(200, row), TextAlignmentOptions.Left);
                var pays = Ui.Text(plate.transform, "Pays", $"x{rules.MultiplierHundredths(d) / 100.0:N2}", Fonts.Label, 20, Theme.GoldBright,
                    new Vector2(80, 0), new Vector2(130, row), TextAlignmentOptions.Right);
                ladder.Add((plate, label, pays));
            }
        }

        void BuildPurse(Transform stage)
        {
            var size = new Vector2(330, 520);
            var panel = Ui.Rect(stage, "Purse", new Vector2(705, 60), size);
            Ui.Panel(panel, "Plate", Vector2.zero, size, Theme.Panel, 20);
            Ui.Outline(panel, "Outline", Vector2.zero, size, Theme.PanelLine, 20);
            Ui.Text(panel, "Heading", "TREASURE", Fonts.Label, 24, Theme.Gold, new Vector2(0, size.y / 2 - 30), new Vector2(300, 30));
            multiplier = Ui.Text(panel, "Multiplier", "", Fonts.Display, 72, Theme.GoldBright, new Vector2(0, 150), new Vector2(300, 90));
            payout = Ui.Text(panel, "Payout", "", Fonts.Display, 40, Theme.Ivory, new Vector2(0, 80), new Vector2(300, 50));
            next = Ui.Paragraph(panel, "Next", "", 19, Theme.Ivory.WithAlpha(0.85f), new Vector2(0, -60), new Vector2(290, 180), TextAlignmentOptions.Top);
            next.richText = true;
            Ui.Paragraph(panel, "Fair", "どの深さで引き返しても RTP は同じ。違うのは、持ち帰る宝の大きさだけ。", 16, Theme.Muted,
                new Vector2(0, -205), new Vector2(290, 70), TextAlignmentOptions.Top);
        }

        void BuildControls(Transform root)
        {
            controls = Ui.Rect(root, "DelveControls", new Vector2(0, CasinoView.BottomRow), new Vector2(700, 100));
            cashOut = Ui.Button(controls, "CashOut", "CASH OUT", Vector2.zero, new Vector2(440, 100), Theme.Gold, Theme.Ink, () => CashOutClicked?.Invoke(), 40, 20);
            controls.gameObject.SetActive(false);
        }

        /// <summary>深さ cleared まで抜けた状態の表示（梯子・扉・宝の額）。</summary>
        public void ShowFloor(DelveRules rules, int cleared, bool busted, long stake = 0)
        {
            int depth = Math.Min(cleared + 1, rules.Depths);
            var floor = rules.Floors[depth - 1];
            depthTitle.text = busted ? "DEFEATED" : cleared >= rules.Depths ? "DEEPEST TREASURE" : $"DEPTH B{depth}";
            depthDetail.text = busted || cleared >= rules.Depths ? "" : $"扉 {floor.Doors} 枚のうち、魔物は {floor.Monsters} 匹  ·  生き残る確率 {floor.Survival:P0}";
            for (int i = 0; i < ladder.Count; i++)
            {
                int d = rules.Depths - i;
                var (plate, label, pays) = ladder[i];
                bool current = !busted && d == cleared + 1;
                bool done = d <= cleared;
                plate.color = current ? Theme.Gold.WithAlpha(0.28f) : done ? Theme.TieGreen.WithAlpha(0.16f) : busted && d == cleared + 1 ? Theme.DealerRed.WithAlpha(0.3f) : Color.clear;
                label.color = done ? Theme.Positive : Theme.Ivory;
                pays.color = done || current ? Theme.GoldBright : Theme.Gold.WithAlpha(0.6f);
            }
            if (cleared == 0 || busted)
            {
                multiplier.text = busted ? "x0" : "x1.00";
                payout.text = busted || stake == 0 ? "" : Labels.Money(stake);
            }
            else
            {
                int m = rules.MultiplierHundredths(cleared);
                multiplier.text = $"x{m / 100.0:N2}";
                payout.text = stake > 0 ? Labels.Money(stake * m / 100) : "";
            }
            if (busted || cleared >= rules.Depths)
                next.text = "";
            else
                next.text = $"次の扉を抜けると <color=#FFE7A3>x{rules.MultiplierHundredths(depth) / 100.0:N2}</color>" +
                            (stake > 0 ? $"（{Labels.Money(stake * rules.MultiplierHundredths(depth) / 100)}）" : "") +
                            $"\n生き残る確率 {floor.Survival:P0}\n最深部まで生き残る確率 {rules.SurvivalTo(rules.Depths) / rules.SurvivalTo(cleared):P3}";
        }

        /// <summary>扉を並べ直す（閉じた状態）。</summary>
        public void LayoutDoors(int count)
        {
            doorCount = count;
            for (int i = 0; i < MaxDoors; i++)
            {
                bool used = i < count;
                doors[i].gameObject.SetActive(used);
                doors[i].Close();
                doors[i].Rect.anchoredPosition = new Vector2((i - (count - 1) / 2f) * DoorSpacing, DoorRow);
                if (used)
                {
                    doors[i].Rect.localScale = Vector3.one * 0.9f;
                    Tween.Scale(doors[i].Rect, Vector3.one, 0.25f, Ease.OutBack, null, i * 0.05f);
                }
            }
        }

        public void SetDoorsSelectable(bool selectable)
        {
            for (int i = 0; i < doorCount; i++)
                doors[i].SetSelectable(selectable);
        }

        public void ShowCashOut(bool visible, long amount = 0, bool enabled = true)
        {
            controls.gameObject.SetActive(visible);
            if (!visible)
                return;
            cashOut.SetLabel(amount > 0 ? $"CASH OUT  {Labels.Money(amount)}" : "CASH OUT");
            cashOut.Interactable = enabled;
        }

        /// <summary>選んだ扉を開け、続いて他の扉の中身も見せる。</summary>
        public IEnumerator Reveal(DoorReveal reveal)
        {
            Sfx.Play(SfxKind.DoorOpen);
            doors[reveal.Chosen].Open(reveal.Monsters[reveal.Chosen], true, 0f);
            yield return Tween.Wait(0.45f);
            if (reveal.Survived)
            {
                Sfx.Play(SfxKind.Sparkle);
                FloatingText.Show(Casino.Stage, "SAFE", Theme.GoldBright, doors[reveal.Chosen].Rect.anchoredPosition + new Vector2(0, 160), 56);
            }
            else
            {
                Sfx.Play(SfxKind.Growl);
                FloatingText.Shake(Casino.Stage, 20f);
                FloatingText.Show(Casino.Stage, "MONSTER!", Theme.Negative, doors[reveal.Chosen].Rect.anchoredPosition + new Vector2(0, 160), 64);
            }
            for (int i = 0; i < doorCount; i++)
            {
                if (i != reveal.Chosen)
                    doors[i].Open(reveal.Monsters[i], false, 0.15f + i * 0.08f);
            }
            yield return Tween.Wait(0.9f);
        }

        /// <summary>キーボード操作: 1〜4 で扉を選び、C で引き返す。</summary>
        public void PollKeys(bool doorsOpen, bool cashOutOpen)
        {
            if (Dialog.IsAnyOpen)
                return;
            if (doorsOpen)
            {
                for (int i = 0; i < doorCount; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                        DoorChosen?.Invoke(i);
                }
            }
            if (cashOutOpen && cashOut.Interactable && Input.GetKeyDown(KeyCode.C))
                CashOutClicked?.Invoke();
        }
    }
}
