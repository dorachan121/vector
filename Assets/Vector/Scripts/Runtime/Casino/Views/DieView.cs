using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>
    /// 1個のダイス（CROWN DICE・SUGOROKU で共通）。角丸の面に目（ピップ）を描き、残す（HOLD）と持ち上がって金の縁が付く。振ると転がりながら目が変わる。
    /// </summary>
    public sealed class DieView : MonoBehaviour
    {
        /// <summary>3×3 の格子のどこに目を置くか（面ごと）。</summary>
        static readonly int[][] PipLayout =
        {
            new int[0],
            new[] { 4 },
            new[] { 2, 6 },
            new[] { 2, 4, 6 },
            new[] { 0, 2, 6, 8 },
            new[] { 0, 2, 4, 6, 8 },
            new[] { 0, 2, 3, 5, 6, 8 },
        };

        const float HoldLift = 18f;

        readonly Image[] pips = new Image[9];
        Image body;
        Image outline;
        Image glow;
        TextMeshProUGUI holdLabel;
        ClickTarget click;
        Color bodyColor;
        Color pipColor;
        Vector2 home;
        float size;
        bool held;
        bool crowned;

        public int Face { get; private set; }
        public RectTransform Rect => (RectTransform)transform;

        public event Action Clicked;

        public static DieView Create(Transform parent, string name, Vector2 position, float size, Color bodyColor, Color pipColor)
        {
            var rt = Ui.Rect(parent, name, position, new Vector2(size, size));
            var view = rt.gameObject.AddComponent<DieView>();
            view.size = size;
            view.home = position;
            view.bodyColor = bodyColor;
            view.pipColor = pipColor;
            view.glow = Ui.Image(rt, "Glow", Art.Glow, Color.clear, Vector2.zero, Vector2.one * size * 1.8f);
            Ui.Panel(rt, "Shadow", new Vector2(4, -6), Vector2.one * size, new Color(0, 0, 0, 0.4f), size * 0.18f);
            view.body = Ui.Panel(rt, "Body", Vector2.zero, Vector2.one * size, bodyColor, size * 0.18f, true);
            view.outline = Ui.Outline(rt, "Outline", Vector2.zero, Vector2.one * (size + 6), Color.clear, size * 0.2f);
            float step = size * 0.28f;
            for (int i = 0; i < 9; i++)
            {
                var p = new Vector2((i % 3 - 1) * step, (1 - i / 3) * step);
                view.pips[i] = Ui.Image(rt, "Pip" + i, Art.Circle, pipColor, p, Vector2.one * size * 0.19f);
            }
            view.holdLabel = Ui.Text(rt, "Hold", "HOLD", Fonts.Label, size * 0.2f, Theme.GoldBright, new Vector2(0, -size * 0.68f), new Vector2(size, size * 0.3f));
            view.holdLabel.alpha = 0;
            view.click = view.body.gameObject.AddComponent<ClickTarget>();
            view.click.Clicked += () => view.Clicked?.Invoke();
            view.click.Interactable = false;
            view.SetFace(0);
            return view;
        }

        /// <summary>目を表示する。0 なら目のない（まだ振っていない）面。</summary>
        public void SetFace(int face)
        {
            Face = face;
            var on = PipLayout[Mathf.Clamp(face, 0, 6)];
            for (int i = 0; i < pips.Length; i++)
                pips[i].enabled = Array.IndexOf(on, i) >= 0;
            body.color = face == 0 ? bodyColor * 0.6f : bodyColor;
            body.color = new Color(body.color.r, body.color.g, body.color.b, 1);
        }

        public void SetSelectable(bool selectable) => click.Interactable = selectable;

        public void SetHeld(bool value)
        {
            held = value;
            holdLabel.alpha = value ? 1 : 0;
            Tween.Move(Rect, home + new Vector2(0, value ? HoldLift : 0), 0.12f, Ease.OutBack);
            RefreshOutline();
        }

        /// <summary>そのラウンドの王冠の目と同じ目なら金の縁で示す。</summary>
        public void SetCrowned(bool value)
        {
            crowned = value;
            RefreshOutline();
        }

        public void SetHint(bool on) => Tween.Tint(glow, on ? Theme.GoldBright.WithAlpha(0.75f) : Color.clear, 0.2f);

        public void SetDimmed(bool dimmed)
        {
            var c = dimmed ? Color.Lerp(bodyColor, Color.black, 0.45f) : bodyColor;
            body.color = new Color(c.r, c.g, c.b, 1);
            foreach (var pip in pips)
                pip.color = dimmed ? Color.Lerp(pipColor, Color.black, 0.45f) : pipColor;
        }

        /// <summary>転がしてから face の目で止める。</summary>
        public void Roll(int face, float seconds, System.Random jitter)
        {
            float spin = (jitter.Next(2) == 0 ? -1 : 1) * (360f + jitter.Next(180));
            int lastFlip = -1;
            Tween.Run(this, "roll", seconds, Ease.OutCubic, t =>
            {
                // 転がっている間は目をぱらぱら変え、最後に本当の目を出す
                int flip = (int)(t * 9);
                if (t < 0.85f && flip != lastFlip)
                {
                    lastFlip = flip;
                    SetFace(1 + jitter.Next(6));
                }
                Rect.localEulerAngles = new Vector3(0, 0, spin * (1 - t));
                float hop = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3)) * (1 - t) * size * 0.35f;
                Rect.anchoredPosition = home + new Vector2(0, hop + (held ? HoldLift : 0));
            }, () =>
            {
                Rect.localEulerAngles = Vector3.zero;
                Rect.anchoredPosition = home + new Vector2(0, held ? HoldLift : 0);
                SetFace(face);
                Tween.Scale(Rect, Vector3.one * 1.08f, 0.08f, Ease.OutQuad, () => Tween.Scale(Rect, Vector3.one, 0.12f));
            });
        }

        void RefreshOutline()
            => outline.color = held ? Theme.GoldBright : crowned ? Theme.Gold.WithAlpha(0.7f) : Color.clear;
    }
}
