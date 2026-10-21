using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>オッズ表の1マス（DERBY の馬券、ORBIT の賭け先など）。クリックでチップを置き、置いた額を右下に表示する。</summary>
    public sealed class TicketView : MonoBehaviour, IBetTarget
    {
        Image plate;
        Image outline;
        Image glow;
        TextMeshProUGUI odds;
        TextMeshProUGUI amount;
        Image pill;
        ClickTarget click;
        Color accent;
        bool open = true;
        bool hovered;

        public int Id { get; private set; }
        public Vector3 ChipWorldPosition => transform.position;
        public RectTransform Rect => (RectTransform)transform;

        public event Action<int> Clicked;

        public static TicketView Create(Transform parent, int id, Vector2 position, Vector2 size, string label, Color accent, Color labelColor)
        {
            var rt = Ui.Rect(parent, "Ticket" + id, position, size);
            var view = rt.gameObject.AddComponent<TicketView>();
            view.Id = id;
            view.accent = accent;
            view.glow = Ui.Image(rt, "Glow", Art.Glow, Color.clear, Vector2.zero, size * 1.5f);
            view.plate = Ui.Panel(rt, "Plate", Vector2.zero, size, new Color(0, 0, 0, 0.55f), 10, true);
            view.outline = Ui.Outline(rt, "Outline", Vector2.zero, size, accent.WithAlpha(0.6f), 10);
            bool hasLabel = !string.IsNullOrEmpty(label);
            if (hasLabel)
                Ui.Text(rt, "Label", label, Fonts.Label, size.y * 0.26f, labelColor, new Vector2(0, size.y * 0.2f), new Vector2(size.x - 8, size.y * 0.4f));
            view.odds = Ui.Text(rt, "Odds", "", Fonts.Display, size.y * (hasLabel ? 0.4f : 0.5f), Theme.GoldBright,
                new Vector2(0, hasLabel ? -size.y * 0.15f : 0), new Vector2(size.x - 8, size.y * 0.55f));
            view.pill = Ui.Panel(rt, "Pill", new Vector2(size.x / 2 - 30, -size.y / 2 + 2), new Vector2(64, 22), Theme.Gold, 11);
            view.amount = Ui.Text(view.pill.transform, "Amount", "", Fonts.Label, 15, Theme.Ink, Vector2.zero, new Vector2(64, 22));
            view.click = view.plate.gameObject.AddComponent<ClickTarget>();
            view.click.Clicked += () => view.Clicked?.Invoke(view.Id);
            view.click.Hovered += h =>
            {
                view.hovered = h;
                view.Refresh();
            };
            view.SetAmount(0);
            view.SetInteractable(false);
            return view;
        }

        /// <summary>オッズを表示する。open が false なら発売しない（チップを置けない）。</summary>
        public void SetOdds(string text, bool open)
        {
            this.open = open;
            odds.text = text;
            odds.color = open ? Theme.GoldBright : Theme.Muted.WithAlpha(0.5f);
            Refresh();
        }

        public void SetAmount(long value)
        {
            pill.gameObject.SetActive(value > 0);
            amount.text = Labels.ChipValue(value);
        }

        public void SetInteractable(bool value)
        {
            click.Interactable = value && open;
            if (!click.Interactable)
                hovered = false;
            Refresh();
        }

        public void ShowOutcome(BetOutcome outcome)
        {
            if (outcome == BetOutcome.Win)
            {
                Tween.Tint(glow, Theme.GoldBright.WithAlpha(0.7f), 0.3f);
                Tween.Scale(transform, Vector3.one * 1.08f, 0.3f, Ease.OutBack);
            }
            else if (outcome == BetOutcome.Lose)
            {
                Tween.Tint(outline, accent.WithAlpha(0.15f), 0.3f);
            }
        }

        /// <summary>的中した馬券を（賭けていなくても）光らせる。</summary>
        public void Highlight(bool on) => Tween.Tint(glow, on ? Theme.Gold.WithAlpha(0.45f) : Color.clear, 0.3f);

        public void ResetOutcome()
        {
            Tween.Tint(glow, Color.clear, 0.3f);
            Tween.Scale(transform, Vector3.one, 0.3f);
            Tween.Stop(outline);
            Refresh();
        }

        void Refresh()
        {
            bool lit = hovered && click.Interactable;
            plate.color = lit ? accent.WithAlpha(0.35f) : new Color(0, 0, 0, open ? 0.55f : 0.3f);
            outline.color = accent.WithAlpha(open ? (lit ? 1f : 0.6f) : 0.15f);
        }
    }
}
