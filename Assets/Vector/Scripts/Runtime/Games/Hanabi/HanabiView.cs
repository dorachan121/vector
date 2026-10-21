using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Hanabi;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Hanabi
{
    /// <summary>
    /// HANABI の画面。左は夜の川辺の空（月・星・町並み・川面）と、昇っていく花火の玉と倍率。右は直近の暴発点・自動で開く倍率・「開く」ボタンと賭け金の場所。
    /// 進行の判断はしない（HanabiDirector から呼ばれるだけ）。
    /// </summary>
    public sealed class HanabiView
    {
        public static readonly Color Night = Theme.Hex("0B1030");
        public static readonly Color NightEdge = Theme.Hex("02030A");

        const float PanelX = 660f;
        const int HistoryCount = 10;
        static readonly Vector2 LaunchAt = new Vector2(-520, -290);
        const float SkyTop = 300f;
        static readonly Color ButtonColor = new Color(0.12f, 0.1f, 0.3f, 0.97f);

        readonly List<int> history = new List<int>();
        RectTransform shell;
        Image trail;
        TextMeshProUGUI multiplier;
        TextMeshProUGUI payout;
        TextMeshProUGUI historyText;
        TextMeshProUGUI autoLabel;
        CasinoButton cashOut;
        CasinoButton autoDown;
        CasinoButton autoUp;

        public CasinoView Casino { get; private set; }

        public event Action CashOutClicked;
        public event Action<int> AutoStepped;

        public static HanabiView Build(HanabiRules rules)
        {
            var view = new HanabiView();
            var casino = CasinoView.Build("Hanabi", "HANABI", "FIREWORK CRASH",
                $"{Labels.Money(rules.MinBet)}–{Labels.Money(rules.MaxBet)}  ·  RTP {HanabiRules.ReturnPercent}%  ·  UP TO x{rules.MaxHundredths / HanabiRules.One:N0}",
                "", Night, NightEdge);
            view.Casino = casino;
            casino.Actions.SetDealLabel("LAUNCH");
            ((RectTransform)casino.Status.transform).anchoredPosition = new Vector2(-260, 330);
            view.BuildSky(casino.Stage);
            view.BuildPanel(casino.Stage);
            casino.AddSpot(HanabiRules.Main, new Vector2(PanelX, -226), new Vector2(136, 136), true, "BET", "CASH OUT ANYTIME", Theme.Gold);
            casino.Bank.anchoredPosition = LaunchAt;
            view.ResetShell();
            return view;
        }

        void BuildSky(Transform stage)
        {
            Ui.Image(stage, "MoonGlow", Art.Glow, Palette.GoldLight.WithAlpha(0.25f), new Vector2(-740, 250), Vector2.one * 260);
            Ui.Image(stage, "Moon", Art.Circle, Theme.Hex("FFF3D6"), new Vector2(-740, 250), Vector2.one * 86);
            var stars = Ui.Rect(stage, "Stars", new Vector2(-260, 130), new Vector2(1240, 420));
            Twinkle.Add(stars, 26, Palette.Ivory, 22);

            // 川向こうの町並み（屋根の高さを変えた影絵）と、灯りの窓
            var rng = new System.Random(7);
            for (int i = 0; i < 18; i++)
            {
                float width = 60 + rng.Next(60);
                float height = 60 + rng.Next(120);
                float x = -830 + i * 66;
                Ui.Panel(stage, "House", new Vector2(x, -338 + height / 2), new Vector2(width, height), Theme.Hex("05071A"), 6);
                if (rng.Next(3) > 0)
                    Ui.Panel(stage, "Light", new Vector2(x + rng.Next(-12, 13), -338 + height * 0.55f), new Vector2(10, 14), Palette.GoldLight.WithAlpha(0.55f), 2);
            }
            var river = Ui.Image(stage, "River", Chrome.Gradient(Theme.Hex("1B2A5A"), Theme.Hex("060A1C")), Color.white, new Vector2(-230, -362), new Vector2(1150, 48));
            river.raycastTarget = false;
            Ui.Image(stage, "Reflection", Art.Glow, Palette.GoldLight.WithAlpha(0.12f), new Vector2(-740, -362), new Vector2(160, 30));

            // 打ち上げ筒と、昇る玉
            Ui.Panel(stage, "Tube", LaunchAt + new Vector2(0, -18), new Vector2(34, 56), Theme.Hex("3A2414"), 6);
            Ui.Panel(stage, "TubeBand", LaunchAt + new Vector2(0, -2), new Vector2(40, 8), Palette.GoldDark, 3);
            trail = Ui.Image(stage, "Trail", Chrome.Gradient(Palette.GoldLight.WithAlpha(0.8f), Palette.GoldLight.WithAlpha(0f)), Color.white, LaunchAt, new Vector2(10, 0));
            trail.rectTransform.pivot = new Vector2(0.5f, 1f);
            shell = Ui.Rect(stage, "Shell", LaunchAt, new Vector2(40, 40));
            Ui.Image(shell, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.8f), Vector2.zero, Vector2.one * 90);
            Ui.Image(shell, "Core", Art.Circle, Theme.Hex("FFF6E0"), Vector2.zero, Vector2.one * 18);

            multiplier = Ui.Text(stage, "Multiplier", "", Fonts.Display, 150, Theme.Ivory, new Vector2(80, 120), new Vector2(700, 170));
            Ui.Outline(multiplier, Palette.Ink, 0.2f);
            payout = Ui.Text(stage, "Payout", "", Fonts.Label, 34, Palette.GoldLight, new Vector2(80, 20), new Vector2(700, 44));
        }

        void BuildPanel(Transform stage)
        {
            var plateSize = new Vector2(420, 640);
            Ui.Panel(stage, "PanelPlate", new Vector2(PanelX, 0), plateSize, Theme.Panel, 22);
            Ui.Outline(stage, "PanelRim", new Vector2(PanelX, 0), plateSize, Theme.PanelLine, 22);
            Ui.Text(stage, "HistoryTitle", "これまでの暴発点", Fonts.Body, 20, Palette.GoldLight, new Vector2(PanelX, 280), new Vector2(400, 28));
            historyText = Ui.Paragraph(stage, "History", "", 22, Theme.Ivory, new Vector2(PanelX, 216), new Vector2(380, 84), TextAlignmentOptions.Top);
            historyText.richText = true;

            Ui.Text(stage, "AutoTitle", "自動で開く倍率", Fonts.Body, 20, Palette.GoldLight, new Vector2(PanelX, 140), new Vector2(400, 28));
            autoDown = Ui.Button(stage, "AutoDown", "−", new Vector2(PanelX - 140, 84), new Vector2(76, 70), ButtonColor, Theme.Ivory, () => AutoStepped?.Invoke(-1), 40, 30);
            var plate = Ui.Panel(stage, "AutoPlate", new Vector2(PanelX, 84), new Vector2(180, 70), new Color(0, 0, 0, 0.6f), 18);
            autoLabel = Ui.Text(plate.transform, "Auto", "", Fonts.Display, 34, Theme.GoldBright, Vector2.zero, new Vector2(170, 60));
            autoUp = Ui.Button(stage, "AutoUp", "+", new Vector2(PanelX + 140, 84), new Vector2(76, 70), ButtonColor, Theme.Ivory, () => AutoStepped?.Invoke(1), 40, 30);

            cashOut = Ui.Button(stage, "CashOut", "開く", new Vector2(PanelX, -40), new Vector2(360, 130), Palette.Gold, Palette.Ink, () => CashOutClicked?.Invoke(), 48, 40);
            cashOut.Label.font = Fonts.Body;
            Ui.Text(stage, "CashOutNote", "弾ける前に押すと、その倍率で開きます", Fonts.Body, 16, Theme.Muted, new Vector2(PanelX, -122), new Vector2(400, 24));
            SetFlying(false);
        }

        /// <summary>自動で開く倍率（0 なら使わない）。</summary>
        public void SetAuto(int hundredths, bool canDown, bool canUp)
        {
            autoLabel.text = hundredths > 0 ? HanabiLabels.Multiple(hundredths) : "OFF";
            autoDown.Interactable = canDown;
            autoUp.Interactable = canUp;
        }

        /// <summary>玉が昇っている間だけ「開く」ボタンを押せるようにする。</summary>
        public void SetFlying(bool flying)
        {
            cashOut.gameObject.SetActive(flying);
            cashOut.Interactable = flying;
        }

        /// <summary>玉を筒に戻し、倍率の表示を消す。</summary>
        public void ResetShell()
        {
            shell.anchoredPosition = LaunchAt;
            shell.localScale = Vector3.one;
            Ui.Group(shell.gameObject).alpha = 1;
            trail.rectTransform.sizeDelta = new Vector2(10, 0);
            multiplier.text = "";
            multiplier.color = Theme.Ivory;
            payout.text = "";
        }

        public void Launch()
        {
            ResetShell();
            Sfx.Play(SfxKind.Launch);
            ParticleBurst.Sparkles(Casino.Stage, LaunchAt, Palette.GoldLight, 10, 180);
        }

        /// <summary>
        /// 倍率に合わせて玉を昇らせる。高さは 1 − 1/倍率 で空の上端に近づいていく（2 倍で半分、10 倍で 9 割）。
        /// </summary>
        public void SetMultiplier(int hundredths, long stake)
        {
            float m = hundredths / (float)HanabiRules.One;
            float height = (SkyTop - LaunchAt.y) * (1 - 1 / m);
            var at = LaunchAt + new Vector2(Mathf.Sin(m * 2.1f) * 18 * (1 - 1 / m), height);
            shell.anchoredPosition = at;
            trail.rectTransform.anchoredPosition = at;
            trail.rectTransform.sizeDelta = new Vector2(10, Mathf.Min(220, height));
            multiplier.text = HanabiLabels.Multiple(hundredths);
            multiplier.color = HanabiLabels.ColorOf(hundredths);
            payout.text = $"開けば {Labels.Money(stake * hundredths / HanabiRules.One)}";
        }

        /// <summary>開いた（大輪の花火が咲く）。</summary>
        public IEnumerator Bloom(int hundredths, long amount)
        {
            var at = shell.anchoredPosition;
            var color = HanabiLabels.ColorOf(hundredths);
            Sfx.Play(SfxKind.Boom);
            // 倍率が大きいほど大輪にする（空の上端からははみ出さない大きさまで）
            float radius = Mathf.Min(170 + 55 * Mathf.Log(hundredths / (float)HanabiRules.One, 2), SkyTop + 130 - at.y);
            Flower(at, color, 36, radius, 1.9f);
            Flower(at, color == Palette.GoldLight || color == Palette.Gold ? Palette.Cyan : Palette.GoldLight, 24, radius * 0.62f, 1.6f);
            Flower(at, Color.white, 12, radius * 0.3f, 1.2f);
            Tween.Fade(Ui.Group(shell.gameObject), 0, 0.3f);
            trail.rectTransform.sizeDelta = new Vector2(10, 0);
            multiplier.color = color;
            payout.text = $"開花！  +{Labels.Money(amount)}";
            if (hundredths >= 1_000)
                FloatingText.Shake(Casino.Stage, 18f, 0.5f);
            yield return Tween.Wait(0.9f);
        }

        /// <summary>花びら（光の粒）を放射状に広げ、少し垂れながら消える一重の輪。</summary>
        void Flower(Vector2 at, Color color, int petals, float radius, float seconds)
        {
            for (int i = 0; i < petals; i++)
            {
                float angle = (i + 0.5f * (petals % 2)) * Mathf.PI * 2 / petals;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var petal = Ui.Image(Casino.Fx, "Petal", Art.Glow, color, at, Vector2.one * 30);
                var spark = Ui.Image(petal.rectTransform, "Spark", Art.Circle, Color.white, Vector2.zero, Vector2.one * 8);
                Tween.Run(petal, "bloom", seconds, Ease.Linear, t =>
                {
                    float spread = 1 - (1 - t) * (1 - t) * (1 - t);
                    petal.rectTransform.anchoredPosition = at + direction * radius * spread + new Vector2(0, -80 * t * t);
                    petal.color = color.WithAlpha(1 - t * t);
                    spark.color = Color.white.WithAlpha(1 - t);
                }, () => UnityEngine.Object.Destroy(petal.gameObject));
            }
        }

        /// <summary>開く前に弾けた（小さな煙を残して落ちる）。</summary>
        public IEnumerator Burst(int crashHundredths)
        {
            var at = shell.anchoredPosition;
            Sfx.Play(SfxKind.Lose, 0.7f);
            ParticleBurst.Sparkles(Casino.Stage, at, Theme.Hex("FF5A36"), 14, 220);
            ParticleBurst.Sparkles(Casino.Stage, at, Theme.Hex("7A7A86"), 10, 120);
            multiplier.text = HanabiLabels.Multiple(crashHundredths);
            multiplier.color = Theme.Negative;
            payout.text = crashHundredths <= HanabiRules.One ? "その場で暴発…" : "暴発…";
            trail.rectTransform.sizeDelta = new Vector2(10, 0);
            Tween.Move(shell, at + new Vector2(0, -120), 0.6f, Ease.InCubic);
            Tween.Fade(Ui.Group(shell.gameObject), 0, 0.6f);
            yield return Tween.Wait(0.8f);
        }

        /// <summary>暴発点の履歴を新しい順に並べる（色で大きさが分かるように）。</summary>
        public void AddHistory(int crashHundredths)
        {
            history.Insert(0, crashHundredths);
            if (history.Count > HistoryCount)
                history.RemoveAt(history.Count - 1);
            var parts = new string[history.Count];
            for (int i = 0; i < history.Count; i++)
                parts[i] = $"<color=#{ColorUtility.ToHtmlStringRGB(HanabiLabels.ColorOf(history[i]))}>{HanabiLabels.Multiple(history[i])}</color>";
            historyText.text = string.Join("  ", parts);
        }

        /// <summary>キーボード操作: Space / Enter で開く。</summary>
        public bool CashOutKey => cashOut.gameObject.activeSelf && !Dialog.IsAnyOpen &&
                                  (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
    }
}
