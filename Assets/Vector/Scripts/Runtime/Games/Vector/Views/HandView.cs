using System;
using System.Collections.Generic;
using UnityEngine;
using Vector.Core;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>手札（最大4枚）。クリックで1枚を選ぶと持ち上がって光る。</summary>
    public sealed class HandView : MonoBehaviour
    {
        const float Spacing = 18f;
        const float Lift = 22f;

        readonly List<CardView> cards = new List<CardView>();
        bool selectable;

        public int Selected { get; private set; } = -1;

        public event Action<int> CardClicked;

        public static HandView Create(Transform parent, Vector2 position)
        {
            var rt = Ui.Rect(parent, "Hand", position, new Vector2(CardView.Size.x * RoundEngine.HandSize + Spacing * 3, CardView.Size.y));
            return rt.gameObject.AddComponent<HandView>();
        }

        public static Vector2 SlotPosition(int index, int count)
        {
            float width = CardView.Size.x + Spacing;
            return new Vector2((index - (count - 1) / 2f) * width, 0);
        }

        /// <summary>配られたカードを手札に加える（位置の移動は呼び出し側で演出する）。</summary>
        public void Add(CardView card)
        {
            card.transform.SetParent(transform, true);
            cards.Add(card);
            card.Click.Clicked += () => CardClicked?.Invoke(cards.IndexOf(card));
            card.Click.Hovered += h => OnHover(card, h);
            card.Click.Interactable = selectable;
        }

        /// <summary>カードを手札から外して返す。残りは詰めて並べ直す。</summary>
        public CardView Take(int index)
        {
            var card = cards[index];
            cards.RemoveAt(index);
            card.Click.Interactable = false;
            card.ClearGlow();
            Select(-1);
            Relayout();
            return card;
        }

        public void SetSelectable(bool value)
        {
            selectable = value;
            foreach (var card in cards)
                card.Click.Interactable = value;
            if (!value)
                Select(-1);
        }

        public void Select(int index)
        {
            Selected = index;
            for (int i = 0; i < cards.Count; i++)
            {
                bool on = i == index;
                if (on)
                    cards[i].SetGlow(Theme.GoldBright, 0.9f);
                else
                    cards[i].ClearGlow();
                Tween.Move(cards[i].Rect, SlotPosition(i, cards.Count) + new Vector2(0, on ? Lift : 0), 0.15f);
            }
        }

        /// <summary>推奨のカードを光らせる。-1 で消す。</summary>
        public void SetHint(int index)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (i == Selected)
                    continue;
                if (i == index)
                    cards[i].SetGlow(Theme.GoldBright, 0.85f);
                else
                    cards[i].ClearGlow();
            }
        }

        public void Relayout()
        {
            for (int i = 0; i < cards.Count; i++)
                Tween.Move(cards[i].Rect, SlotPosition(i, cards.Count), 0.2f);
        }

        public List<CardView> TakeAll()
        {
            var all = new List<CardView>(cards);
            cards.Clear();
            Selected = -1;
            return all;
        }

        void OnHover(CardView card, bool hovered)
        {
            int index = cards.IndexOf(card);
            if (index < 0 || index == Selected)
                return;
            Tween.Move(card.Rect, SlotPosition(index, cards.Count) + new Vector2(0, hovered ? Lift * 0.5f : 0), 0.12f);
        }
    }
}
