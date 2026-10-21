using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>ベットエリアに積まれたチップの山と金額表示。</summary>
    public sealed class ChipStackView : MonoBehaviour
    {
        const int MaxVisibleChips = 6;
        const float ChipStep = 6f;

        readonly List<ChipView> chips = new List<ChipView>();
        RectTransform pile;
        Image amountPill;
        TextMeshProUGUI amountText;
        float diameter;
        int visibleCount;

        public long Amount { get; private set; }
        public RectTransform Rect => (RectTransform)transform;

        public static ChipStackView Create(Transform parent, Vector2 position, float diameter)
        {
            var rt = Ui.Rect(parent, "ChipStack", position, new Vector2(diameter, diameter));
            var view = rt.gameObject.AddComponent<ChipStackView>();
            view.diameter = diameter;
            view.pile = Ui.Rect(rt, "Pile", Vector2.zero, Vector2.one * diameter);
            view.amountPill = Ui.Panel(rt, "Amount", new Vector2(0, -diameter * 0.72f), new Vector2(diameter * 1.25f, 30), new Color(0, 0, 0, 0.75f), 15);
            view.amountText = Ui.Text(view.amountPill.transform, "Text", "", Fonts.Label, 22, Theme.GoldBright, Vector2.zero, new Vector2(diameter * 1.25f, 30));
            view.SetAmount(0);
            return view;
        }

        public void SetAmount(long amount)
        {
            Amount = amount;
            var breakdown = ChipSet.Breakdown(amount, MaxVisibleChips);
            breakdown.Reverse();
            while (chips.Count < breakdown.Count)
                chips.Add(ChipView.Create(pile, 1, diameter, Vector2.zero));
            for (int i = 0; i < chips.Count; i++)
            {
                bool visible = i < breakdown.Count;
                chips[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;
                chips[i].SetValue(breakdown[i]);
                chips[i].Rect.anchoredPosition = new Vector2(0, i * ChipStep);
            }
            visibleCount = breakdown.Count;
            amountPill.gameObject.SetActive(amount > 0);
            amountText.text = Labels.Money(amount);
        }

        /// <summary>山の一番上のチップの位置（キャンバス上）。チップを飛ばす演出の起点・終点に使う。</summary>
        public Vector3 TopWorldPosition => pile.TransformPoint(new Vector3(0, Mathf.Max(0, visibleCount - 1) * ChipStep, 0));
    }
}
