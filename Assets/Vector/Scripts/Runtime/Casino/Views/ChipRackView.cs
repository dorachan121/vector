using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>手元のチップラック。額面を選び、手持ちで買えない額面は暗くする。</summary>
    public sealed class ChipRackView : MonoBehaviour
    {
        const float Diameter = 82f;
        const float Spacing = 92f;
        const float SelectedLift = 14f;

        readonly List<ChipView> chips = new List<ChipView>();
        readonly List<ClickTarget> clicks = new List<ClickTarget>();
        Image selector;

        public long Selected { get; private set; }

        public event Action<long> Changed;

        public static ChipRackView Create(Transform parent, Vector2 position)
        {
            var count = ChipSet.Denominations.Count;
            var rt = Ui.Rect(parent, "ChipRack", position, new Vector2(Spacing * count, Diameter + 30));
            var view = rt.gameObject.AddComponent<ChipRackView>();
            view.selector = Ui.Image(rt, "Selector", Art.Glow, Theme.GoldBright.WithAlpha(0.8f), Vector2.zero, Vector2.one * Diameter * 1.7f);
            for (int i = 0; i < count; i++)
            {
                long value = ChipSet.Denominations[i];
                var chip = ChipView.Create(rt, value, Diameter, new Vector2((i - (count - 1) / 2f) * Spacing, 0));
                var hit = Ui.Image(chip.Rect, "Hit", Art.Circle, Color.clear, Vector2.zero, Vector2.one * Diameter, true);
                var click = hit.gameObject.AddComponent<ClickTarget>();
                click.Clicked += () => view.Select(value, true);
                view.chips.Add(chip);
                view.clicks.Add(click);
            }
            view.Select(ChipSet.Denominations[2], false);
            return view;
        }

        public void Select(long value, bool notify)
        {
            Selected = value;
            for (int i = 0; i < chips.Count; i++)
            {
                bool on = chips[i].Value == value;
                var pos = chips[i].Rect.anchoredPosition;
                Tween.Move(chips[i].Rect, new Vector2(pos.x, on ? SelectedLift : 0), 0.15f, Ease.OutBack);
                if (on)
                    Tween.Move((RectTransform)selector.transform, new Vector2(pos.x, SelectedLift), 0.15f);
            }
            if (notify)
                Changed?.Invoke(value);
        }

        /// <summary>手持ちで置ける額面だけ選べるようにする。選択中が置けなくなったら置ける最大額面へ移す。</summary>
        public void Refresh(long available, bool interactable)
        {
            long bestAffordable = 0;
            for (int i = 0; i < chips.Count; i++)
            {
                bool affordable = chips[i].Value <= available;
                clicks[i].Interactable = interactable && affordable;
                Ui.Group(chips[i].gameObject).alpha = interactable && affordable ? 1f : 0.35f;
                if (affordable)
                    bestAffordable = chips[i].Value;
            }
            selector.enabled = interactable;
            if (interactable && Selected > available && bestAffordable > 0)
                Select(bestAffordable, false);
        }

        public Vector3 WorldPositionOf(long value)
        {
            foreach (var chip in chips)
            {
                if (chip.Value == value)
                    return chip.Rect.position;
            }
            return transform.position;
        }
    }
}
