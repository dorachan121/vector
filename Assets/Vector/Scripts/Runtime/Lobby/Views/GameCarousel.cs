using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.Audio;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ゲームのカードを横に並べて1枚ずつ選ぶ（真ん中のカードを大きく、両隣を小さく暗く見せる。端からはもう一方の端へ回る）。
    /// 横にはらうか、左右の矢印か、下の点で送る。
    /// </summary>
    public sealed class GameCarousel : MonoBehaviour
    {
        const float Spacing = 720f;
        const float SideScale = 0.8f;
        const float Seconds = 0.3f;

        readonly List<RectTransform> cards = new List<RectTransform>();
        readonly List<CanvasGroup> groups = new List<CanvasGroup>();
        readonly List<Image> dots = new List<Image>();

        public event Action<int> Changed;

        public int Index { get; private set; }

        public static GameCarousel Create(Transform parent, Vector2 position, IReadOnlyList<GameEntry> games, float dotsY)
        {
            var rt = Ui.Rect(parent, "Carousel", position, GameKeyCard.Size);
            var carousel = rt.gameObject.AddComponent<GameCarousel>();
            var swipe = Ui.Image(rt, "Swipe", null, new Color(1, 1, 1, 0.001f), Vector2.zero, new Vector2(Ui.Portrait.x, GameKeyCard.Size.y), true);
            swipe.gameObject.AddComponent<SwipeArea>().Swiped += carousel.Step;
            foreach (var entry in games)
            {
                var card = GameKeyCard.Create(rt, entry);
                carousel.cards.Add(card);
                carousel.groups.Add(Ui.Group(card.gameObject));
            }
            Ui.IconButton(rt, "Prev", IconKind.Back, new Vector2(-GameKeyCard.Size.x / 2 - 6, 60), 88, Palette.VioletBright, () => carousel.Step(-1));
            var next = Ui.IconButton(rt, "Next", IconKind.Back, new Vector2(GameKeyCard.Size.x / 2 + 6, 60), 88, Palette.VioletBright, () => carousel.Step(1));
            next.transform.Find("Icon").localScale = new Vector3(-1, 1, 1);
            for (int i = 0; i < games.Count; i++)
                carousel.dots.Add(Ui.Image(rt, "Dot" + i, Art.Circle, Palette.Muted, new Vector2((i - (games.Count - 1) / 2f) * 36, dotsY), Vector2.one * 16));
            carousel.Layout(false);
            return carousel;
        }

        public void Step(int direction) => Show(Index + direction);

        public void Show(int index)
        {
            int n = cards.Count;
            index = (index % n + n) % n;
            if (index == Index)
                return;
            Sfx.Play(SfxKind.CardDeal, 0.6f);
            Index = index;
            Layout(true);
            Changed?.Invoke(index);
        }

        void Layout(bool animate)
        {
            int n = cards.Count;
            for (int i = 0; i < n; i++)
            {
                // 真ん中から見た位置（-n/2 から n/2 まで。端の向こうはもう一方の端へ回る）
                int offset = ((i - Index) % n + n + n / 2) % n - n / 2;
                var card = cards[i];
                var position = new Vector2(offset * Spacing, 0);
                var scale = Vector3.one * (offset == 0 ? 1f : SideScale);
                float alpha = offset == 0 ? 1f : Mathf.Abs(offset) == 1 ? 0.55f : 0f;
                groups[i].blocksRaycasts = offset == 0;
                if (animate && Mathf.Abs(offset) <= 1)
                {
                    Tween.Move(card, position, Seconds, Ease.OutCubic);
                    Tween.Scale(card, scale, Seconds, Ease.OutCubic);
                    Tween.Fade(groups[i], alpha, Seconds);
                }
                else
                {
                    card.anchoredPosition = position;
                    card.localScale = scale;
                    groups[i].alpha = alpha;
                }
            }
            // 真ん中のカードを前面に（矢印と点はさらにその前）
            cards[Index].SetSiblingIndex(transform.childCount - 3 - dots.Count);
            for (int i = 0; i < dots.Count; i++)
            {
                dots[i].color = i == Index ? Palette.Gold : Palette.Muted.WithAlpha(0.5f);
                dots[i].rectTransform.sizeDelta = Vector2.one * (i == Index ? 22 : 14);
            }
        }
    }
}
