using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Harpoon;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// HARPOON の画面。上に獲物ごとの倍率と捕獲率の帯、中央に海（泳ぐ獲物）、下に BET・LOCK と発射台。
    /// 獲物を押すとモリを撃つ。進行の判断はしない（HarpoonDirector から呼ばれるだけ）。
    /// </summary>
    public sealed class HarpoonView
    {
        public static readonly Color Water = Theme.Hex("0E4A6E");
        public static readonly Color Deep = Theme.Hex("03111E");
        static readonly Color ButtonColor = new Color(0.06f, 0.2f, 0.3f, 0.97f);
        static readonly Vector2 FieldCenter = new Vector2(0, -45);
        static readonly Vector2 FieldSize = new Vector2(1750, 680);
        const float StripY = 356f;
        const float StripPitch = 192f;

        readonly Image[] stripPlates = new Image[Quarry.Count];
        HarpoonRules rules;
        RectTransform shots;
        TextMeshProUGUI betLabel;
        CasinoButton down;
        CasinoButton up;
        CasinoButton lockButton;

        public CasinoView Casino { get; private set; }
        public Shoal Shoal { get; private set; }
        public LauncherView Launcher { get; private set; }

        public event Action<int> BetStepped;
        public event Action LockClicked;

        public static HarpoonView Build(HarpoonRules rules)
        {
            var view = new HarpoonView { rules = rules };
            var casino = CasinoView.Build("Harpoon", "HARPOON", "FISH HUNT",
                $"BET {Labels.Money(rules.MinFor(Quarry.Sardine))}–{Labels.Money(rules.MaxFor(Quarry.Sardine))}  ·  LEVIATHAN {HarpoonLabels.Multiple(rules, Quarry.Leviathan)}",
                "", Water, Deep);
            view.Casino = casino;
            casino.ShowBettingControls(false);
            ((RectTransform)casino.Status.transform).anchoredPosition = new Vector2(560, CasinoView.BottomRow);

            var field = Ui.Rect(casino.Stage, "Ocean", FieldCenter, FieldSize);
            field.gameObject.AddComponent<RectMask2D>();
            Ui.Image(field, "Depth", Chrome.Gradient(Theme.Hex("3FA9C9").WithAlpha(0.35f), Deep.WithAlpha(0f)), Color.white, Vector2.zero, FieldSize);
            view.Shoal = Shoal.Create(field);
            // モリとロープは海の外（発射台）まで描くので、獲物と同じ座標の切り抜かないレイヤーに置く
            view.shots = Ui.Rect(casino.Stage, "Shots", FieldCenter, FieldSize);
            view.Launcher = LauncherView.Create(view.shots, new Vector2(0, -FieldSize.y / 2 - 30));
            casino.Bank.anchoredPosition = FieldCenter;

            view.BuildStrip(casino.Stage);
            view.BuildControls(casino.Root);
            return view;
        }

        /// <summary>獲物ごとの倍率と捕獲率（すべて RTP 97%）。</summary>
        void BuildStrip(Transform stage)
        {
            for (int q = 0; q < Quarry.Count; q++)
            {
                var at = new Vector2((q - (Quarry.Count - 1) / 2f) * StripPitch, StripY);
                var plate = Ui.Panel(stage, "Quarry" + q, at, new Vector2(StripPitch - 10, 78), new Color(0, 0, 0, 0.5f), 14);
                stripPlates[q] = plate;
                var size = SwimSpec.Of(q).Size;
                float fit = Mathf.Min(70f / size.x, 50f / size.y);
                Ui.Image(plate.transform, "Icon", MarineArt.Of(q), Color.white, new Vector2(-52, 0), size * fit);
                Ui.Text(plate.transform, "Multiple", HarpoonLabels.Multiple(rules, q), Fonts.Display, rules.IsMystery(q) ? 24 : 30, HarpoonLabels.ColorOf(q),
                    new Vector2(34, 12), new Vector2(120, 36));
                Ui.Text(plate.transform, "Chance", HarpoonLabels.Chance(rules, q), Fonts.Label, 16, Theme.Ivory.WithAlpha(0.75f), new Vector2(34, -20), new Vector2(120, 22));
            }
        }

        void BuildControls(Transform root)
        {
            var controls = Ui.Rect(root, "HarpoonControls", new Vector2(0, CasinoView.BottomRow), new Vector2(1400, 110));
            down = Ui.Button(controls, "BetDown", "−", new Vector2(-820, 0), new Vector2(80, 80), ButtonColor, Theme.Ivory, () => BetStepped?.Invoke(-1), 44, 40);
            var betPlate = Ui.Panel(controls, "BetPlate", new Vector2(-660, 0), new Vector2(220, 80), new Color(0, 0, 0, 0.6f), 20);
            Ui.Text(betPlate.transform, "Caption", "BET / SHOT", Fonts.Label, 16, Theme.Muted, new Vector2(0, 22), new Vector2(200, 20));
            betLabel = Ui.Text(betPlate.transform, "Bet", "", Fonts.Display, 38, Theme.GoldBright, new Vector2(0, -8), new Vector2(200, 46));
            up = Ui.Button(controls, "BetUp", "+", new Vector2(-500, 0), new Vector2(80, 80), ButtonColor, Theme.Ivory, () => BetStepped?.Invoke(1), 44, 40);
            lockButton = Ui.Button(controls, "Lock", "LOCK", new Vector2(-310, 0), new Vector2(200, 80), ButtonColor, Theme.GoldBright, () => LockClicked?.Invoke(), 28, 20);
        }

        public void SetBet(long bet, bool canDown, bool canUp)
        {
            betLabel.text = Labels.Money(bet);
            down.Interactable = canDown;
            up.Interactable = canUp;
        }

        /// <param name="locked">LOCK で狙い続けている獲物（-1 ならなし）。</param>
        public void SetLock(int locked)
        {
            lockButton.SetLabel(locked >= 0 ? "LOCK ON" : "LOCK");
            for (int q = 0; q < stripPlates.Length; q++)
                stripPlates[q].color = q == locked ? Palette.Gold.WithAlpha(0.4f) : new Color(0, 0, 0, 0.5f);
        }

        /// <summary>狙った獲物の種類を帯で光らせる。</summary>
        public void Flash(int quarry)
        {
            var plate = stripPlates[quarry];
            var rest = plate.color;
            plate.color = HarpoonLabels.ColorOf(quarry).WithAlpha(0.55f);
            Tween.Tint(plate, rest, 0.5f);
        }

        /// <summary>捕まえた獲物（金のクラゲなら倍率のルーレット）を見せ、発射台まで引き寄せて払い戻しを飛ばす。</summary>
        public IEnumerator Catch(FishView fish, HarpoonResult result)
        {
            fish.Hold();
            var at = fish.Position;
            var color = HarpoonLabels.ColorOf(result.Quarry);
            ParticleBurst.Sparkles(shots, at, color, 16, 320);
            Sfx.Play(SfxKind.Sparkle);
            if (rules.IsMystery(result.Quarry))
                yield return Roulette(at + new Vector2(0, fish.Size.y / 2 + 30), result.Multiple);
            else
                FloatingText.Show(shots, "x" + result.Multiple, color, at + new Vector2(0, fish.Size.y / 2 + 10), 48, 60, 1f);
            yield return Launcher.Reel(fish);
            Shoal.Remove(fish);
            var muzzle = Launcher.Muzzle;
            FloatingText.Show(shots, "+" + Labels.Money(result.TotalReturn), Theme.GoldBright, muzzle + new Vector2(0, 90), 52, 80, 1.2f);
            ChipFlight.Fly(Casino.Fx, result.TotalReturn, shots.TransformPoint(muzzle), Casino.Hud.BalanceWorldPosition);
            Sfx.Play(SfxKind.ChipsCollect);
            if (result.Multiple >= 50)
                FloatingText.Shake(Casino.Stage, result.Multiple >= 200 ? 26f : 16f, 0.5f);
        }

        /// <summary>金のクラゲの倍率を、候補をめくるように見せてから止める。</summary>
        IEnumerator Roulette(Vector2 at, int final)
        {
            var text = Ui.Text(shots, "Roulette", "", Fonts.Display, 50, Palette.GoldLight, at, new Vector2(260, 60));
            Ui.Outline(text, Palette.Ink, 0.25f);
            var prizes = rules.PrizesOf(Quarry.Jellyfish);
            for (int i = 0; i < 12; i++)
            {
                text.text = "x" + prizes[i % prizes.Count].Multiple;
                Sfx.Play(SfxKind.Tick, 0.5f);
                yield return Tween.Wait(0.05f + i * 0.008f);
            }
            text.text = "x" + final;
            text.color = final >= 200 ? Theme.GoldBright : Palette.GoldLight;
            text.rectTransform.localScale = Vector3.one * 1.5f;
            Tween.Scale(text.rectTransform, Vector3.one, 0.3f, Ease.OutBack);
            yield return Tween.Wait(0.45f);
            Tween.Fade(Ui.Group(text.gameObject), 0f, 0.4f, Ease.OutQuad, () => UnityEngine.Object.Destroy(text.gameObject), 0.4f);
        }

        /// <summary>逃げられた（モリをかわして一瞬速く泳ぎ去る）。</summary>
        public IEnumerator Escape(FishView fish)
        {
            var at = fish.Position;
            ParticleBurst.Sparkles(shots, at, Color.white.WithAlpha(0.8f), 8, 200);
            FloatingText.Show(shots, "ESCAPED", Theme.Muted, at + new Vector2(0, fish.Size.y / 2), 26, 40, 0.8f);
            fish.Dart();
            Sfx.Play(SfxKind.Splash, 0.7f);
            yield return Launcher.Reel(null);
        }
    }
}
