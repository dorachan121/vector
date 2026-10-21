using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>おすすめのバナーを順に見せる（数秒ごとに自動で切り替え、左右の矢印と下の点でも選べる）。</summary>
    public sealed class BannerCarousel : MonoBehaviour
    {
        const float SecondsPerBanner = 6f;
        const float FadeSeconds = 0.4f;

        readonly List<CanvasGroup> pages = new List<CanvasGroup>();
        readonly List<Image> dots = new List<Image>();
        Vector2 size;
        int current;
        float shownFor;

        public static BannerCarousel Create(Transform parent, Vector2 position, Vector2 size)
        {
            var rt = Ui.Rect(parent, "Banners", position, size);
            var carousel = rt.gameObject.AddComponent<BannerCarousel>();
            carousel.size = size;
            return carousel;
        }

        /// <summary>バナーを1枚足す。build にはバナーの土台（大きさ size の RectTransform）が渡される。</summary>
        public void Add(Action<RectTransform> build)
        {
            var page = Ui.Rect(transform, "Banner" + pages.Count, Vector2.zero, size);
            var group = Ui.Group(page.gameObject);
            build(page);
            group.alpha = pages.Count == 0 ? 1 : 0;
            group.blocksRaycasts = pages.Count == 0;
            pages.Add(group);
            LayoutControls();
        }

        void LayoutControls()
        {
            foreach (var d in dots)
                Destroy(d.gameObject);
            dots.Clear();
            for (int i = 0; i < pages.Count; i++)
            {
                var dot = Ui.Image(transform, "Dot" + i, Art.Circle, Palette.Muted, new Vector2((i - (pages.Count - 1) / 2f) * 30, -size.y / 2 - 26), Vector2.one * 14);
                dots.Add(dot);
            }
            if (pages.Count == 2)
            {
                Ui.IconButton(transform, "Prev", IconKind.Back, new Vector2(-size.x / 2 - 4, 0), 56, new Color(1, 1, 1, 0.1f), () => Show(current - 1));
                var next = Ui.IconButton(transform, "Next", IconKind.Back, new Vector2(size.x / 2 + 4, 0), 56, new Color(1, 1, 1, 0.1f), () => Show(current + 1));
                next.transform.Find("Icon").localScale = new Vector3(-1, 1, 1);
            }
            RefreshDots();
        }

        void RefreshDots()
        {
            for (int i = 0; i < dots.Count; i++)
            {
                dots[i].color = i == current ? Palette.Gold : Palette.Muted.WithAlpha(0.5f);
                dots[i].rectTransform.sizeDelta = Vector2.one * (i == current ? 18 : 12);
            }
        }

        public void Show(int index)
        {
            if (pages.Count == 0)
                return;
            index = (index % pages.Count + pages.Count) % pages.Count;
            if (index == current)
                return;
            var from = pages[current];
            var to = pages[index];
            from.blocksRaycasts = false;
            Tween.Fade(from, 0, FadeSeconds);
            to.blocksRaycasts = true;
            Tween.Fade(to, 1, FadeSeconds);
            var rt = (RectTransform)to.transform;
            rt.anchoredPosition = new Vector2(index > current ? 60 : -60, 0);
            Tween.Move(rt, Vector2.zero, FadeSeconds);
            current = index;
            shownFor = 0;
            RefreshDots();
        }

        void Update()
        {
            if (pages.Count < 2 || Dialog.IsAnyOpen)
                return;
            shownFor += Time.unscaledDeltaTime;
            if (shownFor >= SecondsPerBanner)
                Show(current + 1);
        }
    }
}
