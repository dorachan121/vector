using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core;
using Vector.Core.Colosseum;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Colosseum
{
    /// <summary>
    /// COLOSSEUM の画面。左に自分（盾）、右に獣、間に両者のカード。獣の予告・一手のやり取り・決着を演出する。
    /// 進行の判断はしない（ColosseumDirector から呼ばれるだけ）。
    /// </summary>
    public sealed class ColosseumView
    {
        static readonly Vector2 PlayerCenter = new Vector2(-560, 170);
        static readonly Vector2 BeastCenter = new Vector2(560, 180);
        static readonly Vector2 PlayerCardPos = new Vector2(-220, 170);
        static readonly Vector2 IntentCardPos = new Vector2(220, 170);
        static readonly Vector2 DeckPos = new Vector2(0, 298);
        const float CardScale = 1.25f;
        const int LogLines = 5;

        public static readonly Color Sand = Theme.Hex("8C5A34");
        public static readonly Color SandEdge = Theme.Hex("2E1A0E");
        static readonly Color BeastColor = Theme.Hex("5A1A1A");
        static readonly Color StrikeColor = Theme.Hex("8E2A2A");
        static readonly Color GuardColor = Theme.Hex("1F4F8A");

        readonly List<string> log = new List<string>();

        RectTransform playerSide;
        RectTransform beastSide;
        Image shieldGlow;
        Image beastGlow;
        Image[] eyes;
        HpBar playerHp;
        HpBar beastHp;
        TextMeshProUGUI beastName;
        TextMeshProUGUI intentLabel;
        TextMeshProUGUI intentDamage;
        TextMeshProUGUI playerLabel;
        TextMeshProUGUI center;
        TextMeshProUGUI logText;
        CardView intentCard;
        CardView playerCard;
        CasinoButton strike;
        CasinoButton guard;
        TextMeshProUGUI strikeNote;
        TextMeshProUGUI guardNote;
        RectTransform actions;

        public CasinoView Casino { get; private set; }

        public event Action<BattleAction> ActionChosen;

        public static ColosseumView Build(BattleRules rules)
        {
            var view = new ColosseumView();
            var casino = CasinoView.Build("Colosseum", "COLOSSEUM", "ARENA",
                $"TABLE LIMITS  {Labels.Money(rules.MinBet)} – {Labels.Money(rules.MaxBet)}",
                $"DEFEAT THE BEAST PAYS {rules.WinPays}  ·  K = CRITICAL x2  ·  INFINITE DECK", Sand, SandEdge);
            view.Casino = casino;
            casino.Actions.SetDealLabel("FIGHT");
            casino.Bank.anchoredPosition = BeastCenter;
            view.BuildFighters(casino.Stage);
            view.BuildCards(casino.Stage);
            view.BuildPanels(casino.Stage);
            casino.AddSpot(BattleRules.Main, new Vector2(0, -300), new Vector2(170, 170), true, "MAIN", $"PAYS {rules.WinPays}", Theme.Gold);
            view.BuildActions(casino.Root);
            view.Reset(rules.Setup);
            return view;
        }

        void BuildFighters(Transform stage)
        {
            playerSide = Ui.Rect(stage, "Player", PlayerCenter, new Vector2(300, 300));
            shieldGlow = Ui.Image(playerSide, "Glow", Art.Glow, Theme.PlayerBlue.WithAlpha(0.25f), Vector2.zero, new Vector2(420, 420));
            Ui.Image(playerSide, "Rim", Art.Shield, Theme.Gold, Vector2.zero, new Vector2(232, 252));
            Ui.Image(playerSide, "Shield", Art.Shield, Theme.Hex("173A66"), Vector2.zero, new Vector2(212, 232));
            Ui.Image(playerSide, "Star", Art.Gem(4), Theme.GoldBright, new Vector2(0, 18), new Vector2(96, 96));
            Ui.Text(stage, "PlayerTitle", "YOU", Fonts.Display, 40, Theme.PlayerBlue, PlayerCenter + new Vector2(0, 152), new Vector2(300, 50));
            playerHp = HpBar.Create(stage, "PlayerHp", new Vector2(PlayerCenter.x, 0), 380, Theme.PlayerBlue);

            beastSide = Ui.Rect(stage, "Beast", BeastCenter, new Vector2(320, 320));
            beastGlow = Ui.Image(beastSide, "Glow", Art.Glow, Theme.DealerRed.WithAlpha(0.25f), Vector2.zero, new Vector2(460, 460));
            Ui.Image(beastSide, "Rim", Art.BeastHead, Theme.Hex("C79A4B"), Vector2.zero, new Vector2(316, 316));
            Ui.Image(beastSide, "Head", Art.BeastHead, BeastColor, Vector2.zero, new Vector2(300, 300));
            eyes = new[]
            {
                Ui.Image(beastSide, "EyeL", Art.Glow, Theme.GoldBright, new Vector2(-38, -6), new Vector2(46, 46)),
                Ui.Image(beastSide, "EyeR", Art.Glow, Theme.GoldBright, new Vector2(38, -6), new Vector2(46, 46)),
            };
            beastName = Ui.Text(stage, "BeastTitle", "ARENA BEAST", Fonts.Display, 40, Theme.DealerRed, BeastCenter + new Vector2(0, 152), new Vector2(420, 50));
            beastHp = HpBar.Create(stage, "BeastHp", new Vector2(BeastCenter.x, 0), 380, Theme.DealerRed);
        }

        void BuildCards(Transform stage)
        {
            // 無限デッキ（毎回 52 枚から1枚を引く）の山
            for (int i = 0; i < 3; i++)
                Ui.Image(stage, "Deck" + i, Art.CardBack, Color.white, DeckPos + new Vector2(i * 3, i * 3), CardView.Size * 0.8f);
            Ui.Text(stage, "DeckLabel", "INFINITE DECK", Fonts.Label, 16, Theme.FeltPrint, DeckPos + new Vector2(0, -72), new Vector2(200, 22));

            intentLabel = Ui.Text(stage, "IntentLabel", "NEXT ATTACK", Fonts.Label, 22, Theme.DealerRed, IntentCardPos + new Vector2(0, 112), new Vector2(240, 30));
            intentDamage = Ui.Text(stage, "IntentDamage", "", Fonts.Display, 40, Theme.Ivory, IntentCardPos + new Vector2(0, -116), new Vector2(300, 50));
            playerLabel = Ui.Text(stage, "PlayerLabel", "YOUR DRAW", Fonts.Label, 22, Theme.PlayerBlue, PlayerCardPos + new Vector2(0, 112), new Vector2(240, 30));
            center = Ui.Text(stage, "Center", "VS", Fonts.Display, 54, Theme.GoldBright, new Vector2(0, 170), new Vector2(260, 70));
        }

        void BuildPanels(Transform stage)
        {
            var rulesPanel = Ui.Rect(stage, "Rules", new Vector2(-705, -250), new Vector2(330, 200));
            Ui.Panel(rulesPanel, "Plate", Vector2.zero, new Vector2(330, 200), Theme.Panel, 20);
            Ui.Outline(rulesPanel, "Outline", Vector2.zero, new Vector2(330, 200), Theme.PanelLine, 20);
            Ui.Text(rulesPanel, "Heading", "HOW TO FIGHT", Fonts.Label, 22, Theme.Gold, new Vector2(0, 74), new Vector2(300, 30));
            Ui.Paragraph(rulesPanel, "Body",
                "<b>STRIKE</b>  引いた点数のダメージ（K は2倍）。倒せなければ予告どおり受ける\n" +
                "<b>GUARD</b>  引いた点数で受け止め、上回れば反撃、下回れば差を受ける",
                17, Theme.Ivory, new Vector2(0, -16), new Vector2(300, 140)).richText = true;

            var logPanel = Ui.Rect(stage, "Log", new Vector2(705, -250), new Vector2(330, 200));
            Ui.Panel(logPanel, "Plate", Vector2.zero, new Vector2(330, 200), Theme.Panel, 20);
            Ui.Outline(logPanel, "Outline", Vector2.zero, new Vector2(330, 200), Theme.PanelLine, 20);
            Ui.Text(logPanel, "Heading", "BATTLE LOG", Fonts.Label, 22, Theme.Gold, new Vector2(0, 74), new Vector2(300, 30));
            logText = Ui.Paragraph(logPanel, "Lines", "", 17, Theme.Ivory, new Vector2(0, -16), new Vector2(300, 140));
            logText.richText = true;
            logText.lineSpacing = 0;
        }

        void BuildActions(Transform root)
        {
            actions = Ui.Rect(root, "BattleActions", new Vector2(0, CasinoView.BottomRow), new Vector2(700, 100));
            strike = Ui.Button(actions, "Strike", "STRIKE", new Vector2(-180, 0), new Vector2(320, 100), StrikeColor, Theme.Ivory, () => ActionChosen?.Invoke(BattleAction.Strike), 40, 20);
            guard = Ui.Button(actions, "Guard", "GUARD", new Vector2(180, 0), new Vector2(320, 100), GuardColor, Theme.Ivory, () => ActionChosen?.Invoke(BattleAction.Guard), 40, 20);
            foreach (var b in new[] { strike, guard })
                b.Label.rectTransform.anchoredPosition = new Vector2(0, 14);
            strikeNote = Ui.Text(strike.transform, "Note", "", Fonts.Body, 18, Theme.Ivory, new Vector2(0, -26), new Vector2(300, 24));
            guardNote = Ui.Text(guard.transform, "Note", "", Fonts.Body, 18, Theme.Ivory, new Vector2(0, -26), new Vector2(300, 24));
            Ui.Text(strike.transform, "Key", "1", Fonts.Label, 16, Theme.Ivory.WithAlpha(0.5f), new Vector2(-140, 34), new Vector2(30, 20));
            Ui.Text(guard.transform, "Key", "2", Fonts.Label, 16, Theme.Ivory.WithAlpha(0.5f), new Vector2(-140, 34), new Vector2(30, 20));
            actions.gameObject.SetActive(false);
        }

        /// <summary>対戦の初期状態（体力満タン、カードなし）。</summary>
        public void Reset(BattleSetup setup, string beastTitle = "ARENA BEAST")
        {
            beastName.text = beastTitle;
            playerHp.Set(setup.PlayerHp, setup.PlayerHp, false);
            beastHp.Set(setup.BeastHp, setup.BeastHp, false);
            SetRage(false);
            intentDamage.text = "";
            intentLabel.alpha = 0.4f;
            playerLabel.alpha = 0.4f;
            center.text = "VS";
            center.color = Theme.GoldBright;
            log.Clear();
            logText.text = "<color=#A7B5AC>勝負が始まると、ここに記録が残ります</color>";
            playerSide.anchoredPosition = PlayerCenter;
            beastSide.anchoredPosition = BeastCenter;
            playerSide.localScale = beastSide.localScale = Vector3.one;
        }

        /// <summary>手を選ぶボタンを出す。hints が null なら勝率を出さない。</summary>
        public void ShowActions(bool visible, double? strikeWin = null, double? guardWin = null)
        {
            actions.gameObject.SetActive(visible);
            if (!visible)
                return;
            strike.Interactable = guard.Interactable = true;
            bool hints = strikeWin.HasValue && guardWin.HasValue;
            bool strikeBest = hints && strikeWin.Value >= guardWin.Value;
            strikeNote.text = hints ? $"勝率 {strikeWin.Value:P1}" + (strikeBest ? "  ★" : "") : "点数ぶん斬る";
            guardNote.text = hints ? $"勝率 {guardWin.Value:P1}" + (!strikeBest ? "  ★" : "") : "受け止めて反撃";
            strikeNote.color = hints && strikeBest ? Theme.GoldBright : Theme.Ivory;
            guardNote.color = hints && !strikeBest ? Theme.GoldBright : Theme.Ivory;
        }

        /// <summary>キーボード操作: 1 / 2 で STRIKE / GUARD。</summary>
        public void PollKeys()
        {
            if (!actions.gameObject.activeSelf || Dialog.IsAnyOpen)
                return;
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                ActionChosen?.Invoke(BattleAction.Strike);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                ActionChosen?.Invoke(BattleAction.Guard);
        }

        /// <summary>獣が次の一撃の予告カードを見せる。</summary>
        public IEnumerator RevealIntent(Card intent, int damage, bool enraged)
        {
            ClearCard(ref intentCard);
            intentCard = DealCard(intent, IntentCardPos);
            Sfx.Play(SfxKind.CardDeal);
            yield return Tween.Wait(0.3f);
            intentCard.Flip(true);
            Sfx.Play(SfxKind.CardFlip);
            intentLabel.alpha = 1;
            intentDamage.text = enraged ? $"{damage} DMG  (RAGE +{damage - intent.Value})" : $"{damage} DMG";
            intentDamage.color = damage >= 10 ? Theme.Negative : Theme.Ivory;
            SetRage(enraged);
            Tween.Scale(beastSide, Vector3.one * 1.06f, 0.18f, Ease.OutQuad, () => Tween.Scale(beastSide, Vector3.one, 0.25f));
            yield return Tween.Wait(0.3f);
        }

        /// <summary>一手のやり取りを演出する（プレイヤーのカード → ダメージ → 体力）。</summary>
        public IEnumerator PlayExchange(BattleExchange ex, BattleSetup setup)
        {
            ClearCard(ref playerCard);
            playerCard = DealCard(ex.PlayerCard, PlayerCardPos);
            playerLabel.alpha = 1;
            Sfx.Play(SfxKind.CardDeal);
            yield return Tween.Wait(0.32f);
            playerCard.Flip(true);
            Sfx.Play(SfxKind.CardFlip);
            yield return Tween.Wait(0.35f);

            var stage = Casino.Stage;
            if (ex.Action == BattleAction.Strike)
            {
                center.text = ex.Critical ? "CRITICAL!" : "STRIKE";
                center.color = ex.Critical ? Theme.GoldBright : Theme.Ivory;
                yield return Lunge(playerSide, PlayerCenter, new Vector2(140, 0));
                Hit(beastSide, ex.DamageToBeast, ex.Critical);
                beastHp.Set(ex.BeastHp, setup.BeastHp, true);
                if (ex.Critical)
                    FloatingText.Shake(stage, 22f);
                yield return Tween.Wait(0.45f);
                if (ex.DamageToPlayer > 0)
                {
                    yield return Lunge(beastSide, BeastCenter, new Vector2(-140, 0));
                    Hit(playerSide, ex.DamageToPlayer, false);
                    playerHp.Set(ex.PlayerHp, setup.PlayerHp, true);
                    yield return Tween.Wait(0.45f);
                }
            }
            else
            {
                Tween.Tint(shieldGlow, Theme.PlayerBlue.WithAlpha(0.7f), 0.2f);
                yield return Lunge(beastSide, BeastCenter, new Vector2(-140, 0));
                if (ex.Clash)
                {
                    center.text = "CLASH";
                    center.color = Theme.TieGreen;
                    Sfx.Play(SfxKind.Push);
                    FloatingText.Show(stage, "CLASH", Theme.TieGreen, new Vector2(0, 230), 56);
                }
                else if (ex.DamageToBeast > 0)
                {
                    center.text = "COUNTER";
                    center.color = Theme.PlayerBlue;
                    yield return Lunge(playerSide, PlayerCenter, new Vector2(110, 0));
                    Hit(beastSide, ex.DamageToBeast, false);
                    beastHp.Set(ex.BeastHp, setup.BeastHp, true);
                }
                else
                {
                    center.text = "GUARD";
                    center.color = Theme.Ivory;
                    Hit(playerSide, ex.DamageToPlayer, false);
                    playerHp.Set(ex.PlayerHp, setup.PlayerHp, true);
                }
                yield return Tween.Wait(0.45f);
                Tween.Tint(shieldGlow, Theme.PlayerBlue.WithAlpha(0.25f), 0.3f);
            }
            AddLog(ex);
        }

        /// <summary>決着を見せる（勝ちなら獣が崩れ、負けなら盾が沈む）。</summary>
        public IEnumerator ShowVerdict(BattleResult result)
        {
            ShowActions(false);
            switch (result.Winner)
            {
                case RoundWinner.Player:
                    center.text = "VICTORY";
                    center.color = Theme.GoldBright;
                    Tween.Scale(beastSide, new Vector3(1.2f, 0.05f, 1), 0.5f, Ease.InCubic);
                    break;
                case RoundWinner.Dealer:
                    center.text = "DEFEAT";
                    center.color = Theme.Negative;
                    Tween.Scale(playerSide, new Vector3(1.1f, 0.05f, 1), 0.5f, Ease.InCubic);
                    break;
                default:
                    center.text = "DRAW";
                    center.color = Theme.TieGreen;
                    break;
            }
            Tween.Scale(center.rectTransform, Vector3.one * 1.25f, 0.3f, Ease.OutBack);
            yield return Tween.Wait(0.7f);
        }

        /// <summary>カードを片付けて初期状態に戻す。</summary>
        public IEnumerator Clear(BattleSetup setup)
        {
            ShowActions(false);
            foreach (var card in new[] { intentCard, playerCard })
            {
                if (card == null)
                    continue;
                card.Flip(false, 0.2f);
                Tween.Move(card.Rect, (Vector2)Casino.Stage.InverseTransformPoint(Casino.Bank.position), 0.35f, Ease.InCubic, () => UnityEngine.Object.Destroy(card.gameObject));
            }
            intentCard = playerCard = null;
            Tween.Scale(center.rectTransform, Vector3.one, 0.2f);
            yield return Tween.Wait(0.4f);
            Reset(setup, beastName.text);
        }

        public string BeastTitle => beastName.text;

        CardView DealCard(Card card, Vector2 target)
        {
            var view = CardView.Create(Casino.Stage, "Card " + card, DeckPos);
            view.SetCard(card);
            view.SetOwner(Seat.None);
            view.Rect.localScale = Vector3.one * CardScale;
            Tween.Move(view.Rect, target, 0.3f);
            return view;
        }

        static void ClearCard(ref CardView card)
        {
            if (card != null)
                UnityEngine.Object.Destroy(card.gameObject);
            card = null;
        }

        IEnumerator Lunge(RectTransform side, Vector2 home, Vector2 offset)
        {
            Sfx.Play(SfxKind.CardDeal, 0.8f, 0.7f);
            Tween.Move(side, home + offset, 0.12f, Ease.OutQuad, () => Tween.Move(side, home, 0.25f, Ease.OutCubic));
            yield return Tween.Wait(0.12f);
        }

        void Hit(RectTransform side, int damage, bool critical)
        {
            if (damage <= 0)
            {
                FloatingText.Show(Casino.Stage, "BLOCK", Theme.PlayerBlue, side.anchoredPosition + new Vector2(0, 60), 52);
                Sfx.Play(SfxKind.Chip, 0.8f, 1.4f);
                return;
            }
            Sfx.Play(critical ? SfxKind.BigWin : SfxKind.Lose, critical ? 0.6f : 0.5f, critical ? 1.2f : 1.6f);
            FloatingText.Show(Casino.Stage, "-" + damage, critical ? Theme.GoldBright : Theme.Negative, side.anchoredPosition + new Vector2(0, 60), critical ? 92 : 72);
            var group = Ui.Group(side.gameObject);
            Tween.Run(group, "hit", 0.3f, Ease.Linear, t => group.alpha = 0.35f + 0.65f * Mathf.Abs(Mathf.Cos(t * Mathf.PI * 3)), () => group.alpha = 1);
        }

        void SetRage(bool enraged)
        {
            foreach (var eye in eyes)
                Tween.Tint(eye, enraged ? Theme.DealerRed : Theme.GoldBright, 0.3f);
            Tween.Tint(beastGlow, Theme.DealerRed.WithAlpha(enraged ? 0.6f : 0.25f), 0.3f);
        }

        void AddLog(BattleExchange ex)
        {
            string line;
            string you = Labels.CardName(ex.PlayerCard), it = Labels.CardName(ex.Intent);
            if (ex.Action == BattleAction.Strike)
                line = $"<color=#FF7B7B>STRIKE</color> {you} → 獣に {ex.DamageToBeast}" + (ex.Critical ? " 会心!" : "") + (ex.DamageToPlayer > 0 ? $" / 受けた {ex.DamageToPlayer}" : "");
            else if (ex.Clash)
                line = $"<color=#4E9BFF>GUARD</color> {you} vs {it} 相殺";
            else if (ex.DamageToBeast > 0)
                line = $"<color=#4E9BFF>GUARD</color> {you} vs {it} 反撃 {ex.DamageToBeast}";
            else
                line = $"<color=#4E9BFF>GUARD</color> {you} vs {it} 受けた {ex.DamageToPlayer}";
            log.Add(line);
            if (log.Count > LogLines)
                log.RemoveAt(0);
            logText.text = string.Join("\n", log);
        }
    }
}
