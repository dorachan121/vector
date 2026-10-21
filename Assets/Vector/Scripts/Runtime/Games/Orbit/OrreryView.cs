using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Orbit;
using Vector.Game.Audio;
using Vector.Game.UI;

namespace Vector.Game.Orbit
{
    /// <summary>
    /// 天球儀（太陽・3つの軌道・惑星・方角の線）。スピンの結果に向けて惑星を回して止め、合・直列の光線や彗星を見せる。
    /// 角度は北（真上）から時計回りの度数。進行の判断はしない（OrbitDirector から OrbitView 経由で呼ばれる）。
    /// </summary>
    public sealed class OrreryView
    {
        const float InnerRadius = 118f;
        const float MiddleRadius = 200f;
        const float OuterRadius = 282f;

        /// <summary>位置 0 の角度。どの位置も方角の線から半目盛り（7.5°）ずれる。</summary>
        const float FirstAngle = 7.5f;
        const float StepAngle = 360f / OrbitSpin.OuterSlots;

        static readonly float[] Radii = { InnerRadius, MiddleRadius, OuterRadius };
        static readonly float[] Sizes = { 34f, 42f, 50f };

        /// <summary>止まるまでの時間（内惑星から順に止まる）と、その間に回る周回数。</summary>
        static readonly float[] SpinSeconds = { 2.2f, 2.9f, 3.6f };
        static readonly int[] SpinTurns = { 4, 3, 2 };

        readonly RectTransform[] planets = new RectTransform[3];
        readonly float[] angles = new float[3];

        /// <summary>彗星のスピンで惑星を止める位置（結果に関係しない演出用）。</summary>
        readonly System.Random visual = new System.Random();

        RectTransform root;
        CanvasGroup planetGroup;
        Image ray;
        RectTransform comet;
        CanvasGroup cometGroup;

        /// <summary>負けたチップが吸い込まれていく太陽の位置。</summary>
        public RectTransform Sun { get; private set; }

        public static OrreryView Create(Transform parent, Vector2 center)
        {
            var view = new OrreryView();
            view.Build(parent, center);
            return view;
        }

        void Build(Transform parent, Vector2 center)
        {
            float span = OuterRadius * 2;
            root = Ui.Rect(parent, "Orrery", center, Vector2.one * (span + 120));
            Ui.Image(root, "Nebula", Art.Glow, Palette.Haze.WithAlpha(0.22f), Vector2.zero, Vector2.one * (span + 260));
            Ui.Image(root, "Sky", Art.Circle, new Color(0.01f, 0.02f, 0.08f, 0.7f), Vector2.zero, Vector2.one * (span + 70));

            // 方角の線（南北・東西）と、方角・天地の名前
            var line = Theme.FeltPrint.WithAlpha(0.35f);
            Ui.Image(root, "AxisNS", null, line, Vector2.zero, new Vector2(2, span + 50));
            Ui.Image(root, "AxisEW", null, line, Vector2.zero, new Vector2(span + 50, 2));
            for (int q = 0; q < OrbitSpin.Quadrants; q++)
                Ui.Text(root, "Quadrant" + q, OrbitLabels.Quadrant(q), Fonts.Body, 22, Theme.FeltPrint, Polar(OuterRadius + 46, 45 + 90 * q), new Vector2(80, 30));
            Ui.Text(root, "Heaven", OrbitLabels.Half(0), Fonts.Body, 24, Theme.GoldBright.WithAlpha(0.8f), new Vector2(26, OuterRadius + 30), new Vector2(40, 32));
            Ui.Text(root, "Earth", OrbitLabels.Half(1), Fonts.Body, 24, Theme.GoldBright.WithAlpha(0.8f), new Vector2(26, -OuterRadius - 30), new Vector2(40, 32));

            // 軌道と、惑星が止まれる位置の印
            var colors = new[] { OrbitLabels.InnerColor, OrbitLabels.MiddleColor, OrbitLabels.OuterColor };
            for (int p = 0; p < 3; p++)
            {
                float diameter = Radii[p] * 2;
                Ui.Image(root, "Orbit" + p, Art.Ring(5f / diameter), colors[p].WithAlpha(0.45f), Vector2.zero, Vector2.one * diameter);
            }
            for (int k = 0; k < OrbitSpin.OuterSlots; k++)
                Ui.Image(root, "OuterMark", Art.Circle, OrbitLabels.OuterColor.WithAlpha(0.55f), Polar(OuterRadius, AngleOf(k)), Vector2.one * 7);
            for (int j = 0; j < OrbitSpin.MiddleSlots; j++)
                Ui.Image(root, "MiddleMark", Art.Circle, OrbitLabels.MiddleColor.WithAlpha(0.55f), Polar(MiddleRadius, AngleOf(j * 2)), Vector2.one * 7);
            for (int i = 0; i < OrbitSpin.InnerSlots; i++)
            {
                var at = Polar(InnerRadius, AngleOf(i * 4));
                Ui.Image(root, "InnerSeat" + i, Art.Circle, new Color(0.02f, 0.05f, 0.12f, 0.9f), at, Vector2.one * 38);
                Ui.Image(root, "InnerRim" + i, Art.Ring(0.12f), OrbitLabels.InnerColor.WithAlpha(0.6f), at, Vector2.one * 38);
                Ui.Text(root, "InnerNumber" + i, (i + 1).ToString(), Fonts.Label, 20, Theme.Ivory.WithAlpha(0.85f), at, new Vector2(30, 26));
            }

            // 合・直列のときに太陽から伸びる光線（下端を中心に回す）
            ray = Ui.Image(root, "Ray", null, Palette.GoldLight.WithAlpha(0f), Vector2.zero, new Vector2(8, OuterRadius));
            ray.rectTransform.pivot = new Vector2(0.5f, 0);

            // 太陽
            Sun = Ui.Rect(root, "Sun", Vector2.zero, Vector2.one * 80);
            Ui.Image(Sun, "Corona", Art.Glow, Palette.GoldMid.WithAlpha(0.8f), Vector2.zero, Vector2.one * 190);
            var body = Ui.Image(Sun, "Body", Art.Circle, Palette.Gold, Vector2.zero, Vector2.one * 72);
            Pulse.Add(body, 0.05f, 1.6f);

            // 惑星
            var planetLayer = Ui.Rect(root, "Planets", Vector2.zero, Vector2.zero);
            planetGroup = Ui.Group(planetLayer.gameObject);
            for (int p = 0; p < 3; p++)
            {
                var planet = Ui.Rect(planetLayer, "Planet" + p, Vector2.zero, Vector2.one * Sizes[p]);
                Ui.Image(planet, "Glow", Art.Glow, colors[p].WithAlpha(0.7f), Vector2.zero, Vector2.one * Sizes[p] * 2.2f);
                Ui.Image(planet, "Body", Art.Circle, colors[p], Vector2.zero, Vector2.one * Sizes[p]);
                Ui.Image(planet, "Shine", Art.Circle, Color.white.WithAlpha(0.45f), new Vector2(-Sizes[p] * 0.18f, Sizes[p] * 0.18f), Vector2.one * Sizes[p] * 0.32f);
                planets[p] = planet;
            }
            angles[0] = AngleOf(visual.Next(OrbitSpin.InnerSlots) * 4);
            angles[1] = AngleOf(visual.Next(OrbitSpin.MiddleSlots) * 2);
            angles[2] = AngleOf(visual.Next(OrbitSpin.OuterSlots));
            for (int p = 0; p < 3; p++)
                Place(p);

            // 彗星（尾を引く光）
            comet = Ui.Rect(root, "Comet", Vector2.zero, new Vector2(640, 90));
            Ui.Image(comet, "Tail", Art.Glow, OrbitLabels.CometColor.WithAlpha(0.8f), new Vector2(-120, 0), new Vector2(560, 70));
            Ui.Image(comet, "Head", Art.Circle, Palette.GoldLight, new Vector2(170, 0), Vector2.one * 40);
            Ui.Image(comet, "HeadGlow", Art.Glow, Color.white, new Vector2(170, 0), Vector2.one * 120);
            comet.localEulerAngles = new Vector3(0, 0, -32);
            cometGroup = Ui.Group(comet.gameObject);
            cometGroup.alpha = 0;
        }

        /// <summary>外惑星の目盛りの位置 ray の角度。</summary>
        static float AngleOf(int ray) => FirstAngle + StepAngle * ray;

        static Vector2 Polar(float radius, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius);
        }

        void Place(int planet) => planets[planet].anchoredPosition = Polar(Radii[planet], angles[planet]);

        /// <summary>結果の位置へ惑星を回して止める。彗星のスピンは止まった後に彗星が横切り、惑星が隠れる。</summary>
        public IEnumerator Play(OrbitSpin spin)
        {
            var targets = spin.Comet
                ? new[] { visual.Next(OrbitSpin.InnerSlots) * 4, visual.Next(OrbitSpin.MiddleSlots) * 2, visual.Next(OrbitSpin.OuterSlots) }
                : new[] { spin.InnerRay, spin.MiddleRay, spin.Outer };
            Sfx.Play(SfxKind.Shuffle);
            for (int p = 0; p < 3; p++)
            {
                int planet = p;
                float from = angles[p];
                float delta = SpinTurns[p] * 360f + Mathf.Repeat(AngleOf(targets[p]) - from, 360f);
                Tween.Run(planets[p], "orbit", SpinSeconds[p], Ease.OutCubic, t =>
                {
                    angles[planet] = from + delta * t;
                    Place(planet);
                }, () =>
                {
                    angles[planet] = Mathf.Repeat(angles[planet], 360f);
                    Sfx.Play(SfxKind.Tick);
                });
            }
            yield return Tween.Wait(SpinSeconds[2] + 0.15f);

            if (spin.Comet)
            {
                yield return CometPass();
                yield break;
            }
            if (spin.InnerConjunction || spin.OuterConjunction)
                yield return ShowRay(AngleOf(spin.MiddleRay), spin.OuterConjunction ? OuterRadius + 24 : MiddleRadius + 24, spin.Alignment);
        }

        IEnumerator CometPass()
        {
            Sfx.Play(SfxKind.CardDeal);
            // 天球儀の上だけを横切る（右の賭け先の表やボタンにかからないように）
            comet.anchoredPosition = new Vector2(-330, 260);
            cometGroup.alpha = 1;
            Tween.Move(comet, new Vector2(300, -240), 1.1f, Ease.InOutQuad);
            Tween.Fade(planetGroup, 0.12f, 0.6f, Ease.OutQuad, null, 0.35f);
            yield return Tween.Wait(0.55f);
            Sfx.Play(SfxKind.Growl);
            yield return Tween.Wait(0.6f);
            Tween.Fade(cometGroup, 0, 0.3f);
        }

        IEnumerator ShowRay(float angle, float length, bool alignment)
        {
            ray.rectTransform.localEulerAngles = new Vector3(0, 0, -angle);
            ray.rectTransform.sizeDelta = new Vector2(alignment ? 14 : 8, 0);
            ray.color = (alignment ? Palette.GoldLight : Palette.Gold).WithAlpha(0.85f);
            Sfx.Play(SfxKind.Sparkle);
            Tween.Run(ray, "grow", 0.4f, Ease.OutCubic, t => ray.rectTransform.sizeDelta = new Vector2(ray.rectTransform.sizeDelta.x, length * t));
            yield return Tween.Wait(0.45f);
            if (alignment)
            {
                foreach (var planet in planets)
                    Tween.Scale(planet, Vector3.one * 1.35f, 0.35f, Ease.OutBack);
                yield return Tween.Wait(0.4f);
            }
        }

        /// <summary>光線と彗星を片付け、隠れた惑星を元に戻す（次のベット受付の前）。</summary>
        public void Reset()
        {
            Tween.Tint(ray, ray.color.WithAlpha(0f), 0.3f);
            Tween.Fade(planetGroup, 1, 0.4f);
            foreach (var planet in planets)
                Tween.Scale(planet, Vector3.one, 0.3f);
            cometGroup.alpha = 0;
        }
    }
}
