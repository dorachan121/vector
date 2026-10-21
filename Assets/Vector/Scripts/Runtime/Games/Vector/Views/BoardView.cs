using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>
    /// 3×3の盤面。マス（クリックで配置先を選ぶ）、8本のラインの合計札、勝負を決めたラインの強調表示を持つ。
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        const float Gap = 10f;
        static readonly Vector2 CellSize = CardView.Size;

        readonly Image[] slots = new Image[Lines.CellCount];
        readonly Image[] fills = new Image[Lines.CellCount];
        readonly ClickTarget[] slotClicks = new ClickTarget[Lines.CellCount];
        readonly CardView[] cards = new CardView[Lines.CellCount];
        readonly List<(Line line, LineBadge badge)> badges = new List<(Line, LineBadge)>();
        readonly List<Image> lineMarks = new List<Image>();
        RectTransform lineLayer;
        Image hintMarker;

        public event Action<int> CellClicked;

        public static BoardView Create(Transform parent, Vector2 position)
        {
            var rt = Ui.Rect(parent, "Board", position, GridSize);
            var view = rt.gameObject.AddComponent<BoardView>();
            view.Build(rt);
            return view;
        }

        static Vector2 GridSize => new Vector2(CellSize.x * 3 + Gap * 2, CellSize.y * 3 + Gap * 2);

        public static Vector2 CellPosition(int cell)
        {
            int col = cell % 3, row = cell / 3;
            return new Vector2((col - 1) * (CellSize.x + Gap), (1 - row) * (CellSize.y + Gap));
        }

        void Build(RectTransform rt)
        {
            for (int i = 0; i < Lines.CellCount; i++)
            {
                int cell = i;
                slots[i] = Ui.Image(rt, "Slot" + i, Art.RoundedOutline(32, 3f), Theme.FeltPrint, CellPosition(i), CellSize, true);
                slots[i].pixelsPerUnitMultiplier = 32f / 10f;
                fills[i] = Ui.Panel(slots[i].transform, "Fill", Vector2.zero, CellSize, Color.clear, 10);
                slotClicks[i] = slots[i].gameObject.AddComponent<ClickTarget>();
                slotClicks[i].Clicked += () => CellClicked?.Invoke(cell);
                slotClicks[i].Hovered += h => OnSlotHover(cell, h);
            }

            hintMarker = Ui.Image(rt, "Hint", Art.Glow, Color.clear, Vector2.zero, CellSize * 1.5f);
            lineLayer = Ui.Rect(rt, "Lines", Vector2.zero, GridSize);

            var half = GridSize / 2;
            const float badgeOffset = 46f;
            foreach (var family in Lines.Families)
            {
                foreach (var line in Lines.Of(family))
                {
                    Vector2 pos;
                    string prefix = "";
                    switch (family)
                    {
                        case LineFamily.Row:
                            pos = new Vector2(half.x + badgeOffset, CellPosition(line.A).y);
                            break;
                        case LineFamily.Column:
                            pos = new Vector2(CellPosition(line.A).x, -half.y - badgeOffset * 0.62f);
                            break;
                        default:
                            // 0-4-8 は右下、2-4-6 は左下の角に置く
                            bool down = line.A == 0;
                            pos = new Vector2(down ? half.x + badgeOffset : -half.x - badgeOffset, -half.y - badgeOffset * 0.62f);
                            prefix = down ? "\\ " : "/ ";
                            break;
                    }
                    badges.Add((line, LineBadge.Create(rt, $"Sum{family}{line.Index}", pos, prefix)));
                }
            }
            SetBadgesVisible(false);
        }

        /// <summary>カードを盤面に載せる（位置の移動は呼び出し側で演出する）。</summary>
        public void Attach(CardView card, int cell, Seat owner)
        {
            cards[cell] = card;
            card.transform.SetParent(transform, true);
            card.transform.SetSiblingIndex(lineLayer.GetSiblingIndex());
            card.SetOwner(owner);
            slots[cell].color = Color.clear;
            fills[cell].color = Color.clear;
        }

        /// <summary>空いているマスだけクリックできるようにする。</summary>
        public void SetSelectable(bool selectable)
        {
            for (int i = 0; i < Lines.CellCount; i++)
            {
                slotClicks[i].Interactable = selectable && cards[i] == null;
                if (cards[i] != null)
                    continue;
                slots[i].color = selectable ? Theme.GoldBright.WithAlpha(0.8f) : Theme.FeltPrint;
                fills[i].color = selectable ? Theme.Gold.WithAlpha(0.07f) : Color.clear;
            }
        }

        void OnSlotHover(int cell, bool hovered)
        {
            if (cards[cell] != null)
                return;
            slots[cell].color = hovered ? Theme.GoldBright : Theme.GoldBright.WithAlpha(0.8f);
            fills[cell].color = Theme.Gold.WithAlpha(hovered ? 0.25f : 0.07f);
            Tween.Scale(slots[cell].transform, Vector3.one * (hovered ? 1.05f : 1f), 0.12f);
        }

        /// <summary>推奨のマスを光らせる。-1 で消す。</summary>
        public void SetHint(int cell)
        {
            if (cell < 0)
            {
                Tween.Tint(hintMarker, Color.clear, 0.2f);
                return;
            }
            hintMarker.rectTransform.anchoredPosition = CellPosition(cell);
            Tween.Tint(hintMarker, Theme.GoldBright.WithAlpha(0.6f), 0.25f);
        }

        /// <summary>
        /// 各ラインの合計を表示する。プレイヤーのラインは青、ディーラーのラインは赤で、
        /// それぞれ現在の最大ラインを強調する。
        /// </summary>
        public void UpdateSums(int[] values, LineFamily playerFamily)
        {
            SetBadgesVisible(true);
            int playerBest = Scoring.FamilyScore(values, playerFamily);
            var dealerFamily = Scoring.DealerFamily(values, playerFamily);
            int dealerBest = Scoring.FamilyScore(values, dealerFamily);
            foreach (var (line, badge) in badges)
            {
                int sum = line.Sum(values);
                bool mine = line.Family == playerFamily;
                bool emphasized = mine ? sum == playerBest : line.Family == dealerFamily && sum == dealerBest;
                badge.Show(sum, mine ? Theme.PlayerBlue : Theme.DealerRed, emphasized && sum > 0);
            }
        }

        /// <summary>勝負を決めたラインに太い帯を引く。</summary>
        public void MarkLines(IEnumerable<Line> lines, Color color)
        {
            foreach (var line in lines)
            {
                Vector2 a = CellPosition(line.A), c = CellPosition(line.C);
                var mark = Ui.Panel(lineLayer, "Mark" + line, (a + c) / 2, new Vector2(Vector2.Distance(a, c) + CellSize.x * 0.7f, 26), color.WithAlpha(0f), 13);
                mark.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(c.y - a.y, c.x - a.x) * Mathf.Rad2Deg);
                Tween.Tint(mark, color.WithAlpha(0.8f), 0.35f);
                lineMarks.Add(mark);
                foreach (int cell in new[] { line.A, line.B, line.C })
                    cards[cell]?.SetGlow(color);
            }
        }

        /// <summary>強調表示以外のカードを暗くする。</summary>
        public void DimExcept(ICollection<int> keep)
        {
            for (int i = 0; i < Lines.CellCount; i++)
                cards[i]?.SetDimmed(!keep.Contains(i));
        }

        /// <summary>盤面上のカードを取り外して返す（片付けの演出用）。</summary>
        public List<CardView> DetachAll()
        {
            var list = new List<CardView>();
            for (int i = 0; i < Lines.CellCount; i++)
            {
                if (cards[i] != null)
                    list.Add(cards[i]);
                cards[i] = null;
                slots[i].color = Theme.FeltPrint;
                fills[i].color = Color.clear;
            }
            foreach (var mark in lineMarks)
                Destroy(mark.gameObject);
            lineMarks.Clear();
            SetHint(-1);
            SetBadgesVisible(false);
            return list;
        }

        void SetBadgesVisible(bool visible)
        {
            foreach (var entry in badges)
                entry.badge.gameObject.SetActive(visible);
        }
    }
}
