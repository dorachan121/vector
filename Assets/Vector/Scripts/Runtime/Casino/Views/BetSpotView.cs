using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>
    /// フェルトに印刷されたベットエリア。クリックでチップを置き、精算時は勝ち負けを光り方で示す。
    /// ベットエリアは番号で表す（番号の意味はゲームごとに決める。VECTOR は BetSpot の値）。
    /// </summary>
    public sealed class BetSpotView : MonoBehaviour, IBetTarget
    {
        Image area;
        Image outline;
        Image glow;
        TextMeshProUGUI title;
        TextMeshProUGUI odds;
        Color accent;
        ChipStackView stack;
        ClickTarget click;
        bool hovered;

        public int Id { get; private set; }
        public ChipStackView Stack => stack;
        public Vector3 ChipWorldPosition => stack.TopWorldPosition;

        public event Action<int> Clicked;

        public static BetSpotView Create(Transform parent, int id, Vector2 position, Vector2 size, bool round, string titleText, string oddsText, Color accent)
        {
            var rt = Ui.Rect(parent, "Spot" + titleText, position, size);
            var view = rt.gameObject.AddComponent<BetSpotView>();
            view.Id = id;
            view.accent = accent;

            view.glow = Ui.Image(rt, "Glow", Art.Glow, accent.WithAlpha(0f), Vector2.zero, size * 1.6f);
            if (round)
            {
                view.area = Ui.Image(rt, "Area", Art.Circle, accent.WithAlpha(0.10f), Vector2.zero, size, true);
                view.outline = Ui.Image(rt, "Outline", Art.Ring(0.03f), accent.WithAlpha(0.85f), Vector2.zero, size);
            }
            else
            {
                view.area = Ui.Panel(rt, "Area", Vector2.zero, size, accent.WithAlpha(0.10f), 26, true);
                view.outline = Ui.Outline(rt, "Outline", Vector2.zero, size, accent.WithAlpha(0.85f), 26);
            }
            view.title = Ui.Text(rt, "Title", titleText, Fonts.Label, size.y * 0.17f, accent, new Vector2(0, size.y * 0.24f), new Vector2(size.x, 40));
            view.odds = Ui.Text(rt, "Odds", oddsText, Fonts.Label, size.y * 0.12f, Theme.Ivory.WithAlpha(0.75f), new Vector2(0, -size.y * 0.28f), new Vector2(size.x, 30));
            view.stack = ChipStackView.Create(rt, new Vector2(0, -2), Mathf.Min(size.x, size.y) * 0.42f);

            view.click = view.area.gameObject.AddComponent<ClickTarget>();
            view.click.Clicked += () => view.Clicked?.Invoke(view.Id);
            view.click.Hovered += h =>
            {
                view.hovered = h;
                view.Refresh();
            };
            view.SetInteractable(false);
            return view;
        }

        public void SetAmount(long amount)
        {
            stack.SetAmount(amount);
            title.alpha = amount > 0 ? 0.25f : 1f;
            odds.alpha = amount > 0 ? 0f : 0.75f;
        }

        public void SetInteractable(bool value)
        {
            click.Interactable = value;
            if (!value)
                hovered = false;
            Refresh();
        }

        /// <summary>精算結果を表示する。勝ちは光り、負けは暗くなる。</summary>
        public void ShowOutcome(BetOutcome outcome)
        {
            switch (outcome)
            {
                case BetOutcome.Win:
                    Tween.Tint(glow, accent.WithAlpha(0.55f), 0.35f);
                    Tween.Scale(transform, Vector3.one * 1.06f, 0.35f, Ease.OutBack);
                    break;
                case BetOutcome.Lose:
                    Tween.Tint(outline, accent.WithAlpha(0.25f), 0.4f);
                    break;
            }
        }

        public void ResetOutcome()
        {
            Tween.Tint(glow, accent.WithAlpha(0f), 0.3f);
            Tween.Scale(transform, Vector3.one, 0.3f);
            Tween.Stop(outline);
            Refresh();
        }

        void Refresh()
        {
            outline.color = accent.WithAlpha(hovered && click.Interactable ? 1f : 0.85f);
            area.color = accent.WithAlpha(hovered && click.Interactable ? 0.22f : 0.10f);
        }
    }
}
