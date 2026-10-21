using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vector.Game.Audio;

namespace Vector.Game.UI.Widgets
{
    /// <summary>画面の中の切り替え（「デイリー｜ログインボーナス」など）。選んだ項目に金の光沢が滑っていき、知らせがあればバッジを出す。</summary>
    public sealed class SegmentedTabs
    {
        const float Height = 88f;

        readonly List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();
        readonly List<Badge> badges = new List<Badge>();
        RectTransform highlight;
        float segment;
        float origin;

        public event Action<int> Selected;

        public int Current { get; private set; } = -1;

        public static SegmentedTabs Create(Transform parent, Vector2 position, float width, params string[] items)
        {
            var tabs = new SegmentedTabs { segment = width / items.Length, origin = -width / 2 + width / items.Length / 2 };
            var rt = Ui.Rect(parent, "Segments", position, new Vector2(width, Height));
            Ui.Shadow(rt, "Shadow", new Vector2(0, -4), new Vector2(width, Height), Height / 2, 0.5f);
            Ui.Sliced(rt, "Track", Chrome.Shade, Palette.NightDeep.WithAlpha(0.92f), Vector2.zero, new Vector2(width, Height), Height / 2);
            Ui.Sliced(rt, "Rim", Chrome.Rim, Palette.GoldMid.WithAlpha(0.7f), Vector2.zero, new Vector2(width, Height), Height / 2);
            var size = new Vector2(tabs.segment - 12, Height - 12);
            tabs.highlight = Ui.Rect(rt, "Highlight", new Vector2(tabs.origin, 0), size);
            Ui.Sliced(tabs.highlight, "Face", Chrome.Gloss, Palette.Gold, Vector2.zero, size, size.y / 2);
            Ui.Sliced(tabs.highlight, "Shine", Chrome.Shine, Color.white, Vector2.zero, size, size.y / 2);
            for (int i = 0; i < items.Length; i++)
            {
                int index = i;
                var at = new Vector2(tabs.origin + i * tabs.segment, 0);
                var hit = Ui.Image(rt, "Item" + i, null, new Color(1, 1, 1, 0.001f), at, new Vector2(tabs.segment, Height), true);
                hit.gameObject.AddComponent<ClickTarget>().Clicked += () =>
                {
                    Sfx.Play(SfxKind.Button);
                    tabs.Select(index);
                };
                tabs.labels.Add(Ui.Text(rt, "Label" + i, items[i], Fonts.Body, 30, Palette.Muted, at + new Vector2(0, 1), new Vector2(tabs.segment - 20, 40)));
                var badge = Badge.Create(rt, at + new Vector2(tabs.segment / 2 - 30, Height / 2 - 12), 34);
                badge.SetCount(0);
                tabs.badges.Add(badge);
            }
            return tabs;
        }

        public void Select(int index)
        {
            bool changed = index != Current;
            Current = index;
            Tween.Move(highlight, new Vector2(origin + index * segment, 0), 0.22f, Ease.OutCubic);
            for (int i = 0; i < labels.Count; i++)
                labels[i].color = i == index ? Palette.Ink : Palette.Muted;
            if (changed)
                Selected?.Invoke(index);
        }

        public void SetBadge(int index, int count) => badges[index].SetCount(count);
    }
}
