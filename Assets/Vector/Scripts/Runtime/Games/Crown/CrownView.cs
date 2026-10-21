using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vector.Core;
using Vector.Core.Crown;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Crown
{
    /// <summary>
    /// CROWN DICE の画面。上に王冠のダイス、その下にディーラーの5個、手前にプレイヤーの5個。
    /// 進行の判断はしない（CrownDirector から呼ばれるだけ）。
    /// </summary>
    public sealed class CrownView
    {
        const float DieSize = 104f;
        const float DieSpacing = 122f;
        const float DealerRow = 182f;
        const float PlayerRow = 10f;
        const float RollSeconds = 0.65f;

        public static readonly Color Felt = Theme.Hex("173B6B");
        public static readonly Color FeltEdge = Theme.Hex("081429");
        static readonly Color Ivory = Theme.Hex("F7F2E6");
        static readonly Color ButtonColor = new Color(0.1f, 0.2f, 0.38f, 0.97f);

        readonly DieView[] player = new DieView[DiceRank.DiceCount];
        readonly DieView[] dealer = new DieView[DiceRank.DiceCount];
        readonly System.Random jitter = new System.Random();
        DieView crown;
        UnityEngine.UI.Image crownMark;
        TextMeshProUGUI crownNote;
        TextMeshProUGUI playerRank;
        TextMeshProUGUI dealerRank;
        RectTransform controls;
        CasinoButton reroll;
        CasinoButton stand;
        TextMeshProUGUI rerollNote;
        TextMeshProUGUI standNote;

        public CasinoView Casino { get; private set; }
        public PaytablePanel Paytable { get; private set; }

        public event Action<int> DieClicked;
        public event Action RerollClicked;
        public event Action StandClicked;

        public static CrownView Build(CrownRules rules)
        {
            var view = new CrownView();
            var casino = CasinoView.Build("Crown", "CROWN DICE", "VS DEALER",
                $"TABLE LIMITS  {Labels.Money(rules.MinBet)} – {Labels.Money(rules.MaxBet)}",
                $"ANTE PAYS {rules.WinPays}  ·  TIES PUSH  ·  CROWN YACHT BONUS {rules.CrownYachtBonus}", Felt, FeltEdge);
            view.Casino = casino;
            casino.Actions.SetDealLabel("ROLL");
            casino.Bank.anchoredPosition = new Vector2(0, DealerRow);
            view.BuildDice(casino.Stage);
            view.BuildPanels(casino.Stage, rules);
            casino.AddSpot(CrownRules.Ante, new Vector2(-430, -290), new Vector2(170, 170), true, "ANTE", $"PAYS {rules.WinPays}", Theme.Gold);
            casino.AddSpot(CrownRules.Opening, new Vector2(430, -290), new Vector2(140, 140), true, "OPENING", "FIRST ROLL", Theme.GoldBright);
            view.BuildControls(casino.Root);
            view.Reset();
            return view;
        }

        void BuildDice(Transform stage)
        {
            crown = DieView.Create(stage, "CrownDie", new Vector2(0, 322), 84, Theme.Gold, Theme.Ink);
            // 振る前の王冠のダイスは目がないので、王冠の印を出しておく
            crownMark = Ui.Icon(stage, "CrownMark", IconKind.Crown, Theme.Ink.WithAlpha(0.7f), new Vector2(0, 322), 54);
            Ui.Text(stage, "CrownTitle", "CROWN", Fonts.Label, 22, Theme.GoldBright, new Vector2(-110, 322), new Vector2(120, 30));
            crownNote = Ui.Text(stage, "CrownNote", "", Fonts.Body, 18, Theme.Ivory.WithAlpha(0.85f), new Vector2(190, 322), new Vector2(260, 26), TextAlignmentOptions.Left);

            Ui.Text(stage, "DealerTitle", "DEALER", Fonts.Label, 26, Theme.DealerRed, new Vector2(-420, DealerRow), new Vector2(160, 34));
            Ui.Text(stage, "PlayerTitle", "YOU", Fonts.Label, 26, Theme.PlayerBlue, new Vector2(-420, PlayerRow), new Vector2(160, 34));
            for (int i = 0; i < DiceRank.DiceCount; i++)
            {
                float x = (i - 2) * DieSpacing;
                dealer[i] = DieView.Create(stage, "Dealer" + i, new Vector2(x, DealerRow), DieSize * 0.86f, Ivory, Theme.CardRed);
                int index = i;
                player[i] = DieView.Create(stage, "Player" + i, new Vector2(x, PlayerRow), DieSize, Ivory, Theme.CardBlack);
                player[i].Clicked += () => DieClicked?.Invoke(index);
                Ui.Text(stage, "Key" + i, (i + 1).ToString(), Fonts.Label, 16, Theme.Ivory.WithAlpha(0.35f), new Vector2(x, PlayerRow - 84), new Vector2(30, 20));
            }
            dealerRank = Ui.Text(stage, "DealerRank", "", Fonts.Label, 26, Theme.Ivory, new Vector2(0, DealerRow - 76), new Vector2(560, 34));
            playerRank = Ui.Text(stage, "PlayerRank", "", Fonts.Label, 30, Theme.GoldBright, new Vector2(0, PlayerRow - 112), new Vector2(560, 38));
        }

        void BuildPanels(Transform stage, CrownRules rules)
        {
            var order = Ui.Rect(stage, "Ranking", new Vector2(-705, 120), new Vector2(330, 420));
            Ui.Panel(order, "Plate", Vector2.zero, new Vector2(330, 420), Theme.Panel, 20);
            Ui.Outline(order, "Outline", Vector2.zero, new Vector2(330, 420), Theme.PanelLine, 20);
            Ui.Text(order, "Heading", "HAND RANKING", Fonts.Label, 22, Theme.Gold, new Vector2(0, 180), new Vector2(300, 30));
            var kinds = new[] { DiceHandKind.Yacht, DiceHandKind.FourOfAKind, DiceHandKind.FullHouse, DiceHandKind.Straight,
                DiceHandKind.ThreeOfAKind, DiceHandKind.TwoPair, DiceHandKind.OnePair, DiceHandKind.NoHand };
            for (int i = 0; i < kinds.Length; i++)
                Ui.Text(order, "Kind" + i, $"{i + 1}. {CrownLabels.Hand(kinds[i])}", Fonts.Label, 19, Theme.Ivory, new Vector2(0, 142 - i * 30), new Vector2(280, 28), TextAlignmentOptions.Left);
            Ui.Paragraph(order, "Crown", "同じ役なら組の目で比べます。<color=#FFE7A3>王冠の目は 6 より強い</color>（ストレートを除く）", 17, Theme.Muted,
                new Vector2(0, -150), new Vector2(290, 90)).richText = true;

            Paytable = PaytablePanel.Create(stage, new Vector2(705, 120), new Vector2(330, 420));
            Paytable.Heading("ANTE");
            Paytable.Row("win", "BEAT THE DEALER", rules.WinPays.ToString());
            Paytable.Row("tie", "TIE", "PUSH");
            Paytable.Row("bonus", "CROWN YACHT BONUS", rules.CrownYachtBonus.ToString());
            Paytable.Gap();
            Paytable.Heading("OPENING  (FIRST ROLL)");
            foreach (var pair in rules.OpeningPays)
                Paytable.Row(pair.Key.ToString(), CrownLabels.Opening(pair.Key), pair.Value.ToString());
        }

        void BuildControls(Transform root)
        {
            controls = Ui.Rect(root, "DiceControls", new Vector2(0, CasinoView.BottomRow), new Vector2(700, 100));
            reroll = Ui.Button(controls, "Reroll", "REROLL", new Vector2(-180, 0), new Vector2(320, 100), ButtonColor, Theme.GoldBright, () => RerollClicked?.Invoke(), 38, 20);
            stand = Ui.Button(controls, "Stand", "STAND", new Vector2(180, 0), new Vector2(320, 100), Theme.Gold, Theme.Ink, () => StandClicked?.Invoke(), 38, 20);
            reroll.Label.rectTransform.anchoredPosition = stand.Label.rectTransform.anchoredPosition = new Vector2(0, 14);
            rerollNote = Ui.Text(reroll.transform, "Note", "", Fonts.Body, 18, Theme.Ivory, new Vector2(0, -26), new Vector2(300, 24));
            standNote = Ui.Text(stand.transform, "Note", "", Fonts.Body, 18, Theme.Ink, new Vector2(0, -26), new Vector2(300, 24));
            controls.gameObject.SetActive(false);
        }

        public void Reset()
        {
            crown.SetFace(0);
            crownMark.enabled = true;
            crownNote.text = "振った目が 6 より強くなる";
            for (int i = 0; i < DiceRank.DiceCount; i++)
            {
                player[i].SetFace(0);
                player[i].SetHeld(false);
                player[i].SetHint(false);
                player[i].SetCrowned(false);
                player[i].SetDimmed(false);
                dealer[i].SetFace(0);
                dealer[i].SetHeld(false);
                dealer[i].SetCrowned(false);
                dealer[i].SetDimmed(false);
            }
            playerRank.text = dealerRank.text = "";
            Paytable.ClearHighlights();
            ShowControls(false);
        }

        public IEnumerator RollCrown(int face)
        {
            Sfx.Play(SfxKind.DiceRoll);
            crownMark.enabled = false;
            crown.Roll(face, RollSeconds * 1.2f, jitter);
            yield return Tween.Wait(RollSeconds * 1.2f + 0.1f);
            crownNote.text = $"このラウンドは {face} が最強の目";
            Sfx.Play(SfxKind.Sparkle);
        }

        /// <summary>プレイヤーのダイスを振る（held のビットのダイスは残す）。</summary>
        public IEnumerator RollPlayer(IReadOnlyList<int> faces, int heldMask, int crownFace)
            => Roll(player, faces, heldMask, crownFace);

        public IEnumerator RollDealer(IReadOnlyList<int> faces, int heldMask, int crownFace)
            => Roll(dealer, faces, heldMask, crownFace);

        IEnumerator Roll(DieView[] dice, IReadOnlyList<int> faces, int heldMask, int crownFace)
        {
            bool any = false;
            for (int i = 0; i < dice.Length; i++)
            {
                dice[i].SetHint(false);
                if ((heldMask & (1 << i)) != 0)
                    continue;
                dice[i].SetCrowned(false);
                dice[i].Roll(faces[i], RollSeconds + i * 0.05f, jitter);
                any = true;
            }
            if (any)
            {
                Sfx.Play(SfxKind.DiceRoll);
                yield return Tween.Wait(RollSeconds + 0.3f);
            }
            for (int i = 0; i < dice.Length; i++)
                dice[i].SetCrowned(faces[i] == crownFace);
        }

        public void SetPlayerRank(string text) => playerRank.text = text;

        public void SetDealerRank(string text) => dealerRank.text = text;

        /// <summary>プレイヤーの残す目を表示する。</summary>
        public void ShowHolds(int heldMask)
        {
            for (int i = 0; i < player.Length; i++)
                player[i].SetHeld((heldMask & (1 << i)) != 0);
        }

        /// <summary>ディーラーが残した目を示す（振り直す前に一瞬持ち上げる）。</summary>
        public void ShowDealerHolds(int heldMask)
        {
            for (int i = 0; i < dealer.Length; i++)
                dealer[i].SetHeld((heldMask & (1 << i)) != 0);
        }

        public void SetPlayerSelectable(bool selectable)
        {
            foreach (var die in player)
                die.SetSelectable(selectable);
        }

        /// <summary>推奨の残し方を光らせる。-1 で消す。</summary>
        public void ShowHint(int holdMask)
        {
            for (int i = 0; i < player.Length; i++)
                player[i].SetHint(holdMask >= 0 && (holdMask & (1 << i)) != 0);
        }

        public void ShowControls(bool visible, int rerollsLeft = 0, string rerollNoteText = "", string standNoteText = "")
        {
            controls.gameObject.SetActive(visible);
            if (!visible)
                return;
            reroll.SetLabel($"REROLL ({rerollsLeft})");
            rerollNote.text = rerollNoteText;
            standNote.text = standNoteText;
        }

        public void SetRerollEnabled(bool enabled) => reroll.Interactable = enabled;

        /// <summary>勝負の結果に合わせて、負けた側のダイスを暗くする。</summary>
        public void ShowVerdict(RoundWinner winner)
        {
            for (int i = 0; i < player.Length; i++)
            {
                player[i].SetHeld(false);
                player[i].SetDimmed(winner == RoundWinner.Dealer);
                dealer[i].SetHeld(false);
                dealer[i].SetDimmed(winner == RoundWinner.Player);
            }
        }

        /// <summary>キーボード操作: 1〜5 で残す目を切り替え、R で振り直し、S で止める。</summary>
        public void PollKeys()
        {
            if (!controls.gameObject.activeSelf || Dialog.IsAnyOpen)
                return;
            for (int i = 0; i < player.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                    DieClicked?.Invoke(i);
            }
            if (Input.GetKeyDown(KeyCode.R) && reroll.Interactable)
                RerollClicked?.Invoke();
            else if (Input.GetKeyDown(KeyCode.S))
                StandClicked?.Invoke();
        }

        public void Flash(string text, Color color) => FloatingText.Show(Casino.Stage, text, color, new Vector2(0, 96), 60, 60, 1.4f);
    }
}
