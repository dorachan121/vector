using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.Audio;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>ホーム画面の下のタブ（HOME / GAMES / STORY / MISSIONS / SHOP）。画面の下端に付く。選んだタブは金色に光り、知らせがあればバッジを出す。</summary>
    public sealed class BottomNav
    {
        public const float Height = 164f;

        sealed class Tab
        {
            public HomeTabId Id;
            public Image Highlight;
            public Image Icon;
            public TextMeshProUGUI Label;
            public Badge Badge;
        }

        static readonly (HomeTabId id, IconKind icon, string label)[] Items =
        {
            (HomeTabId.Home, IconKind.Home, "HOME"),
            (HomeTabId.Games, IconKind.Games, "GAMES"),
            (HomeTabId.Story, IconKind.Story, "STORY"),
            (HomeTabId.Missions, IconKind.Missions, "MISSIONS"),
            (HomeTabId.Shop, IconKind.Shop, "SHOP"),
        };

        readonly List<Tab> tabs = new List<Tab>();

        public event Action<HomeTabId> Selected;

        public static BottomNav Create(Transform parent)
        {
            var nav = new BottomNav();
            float w = Ui.Portrait.x;
            var rt = Ui.Edge(parent, "Nav", false, w, Height);
            // 帯は横に長めに敷き、縦長の画面を横に広げた PC のウィンドウでも途切れないようにする
            Ui.Shadow(rt, "Shadow", new Vector2(0, 8), new Vector2(w * 3, Height), 20, 0.6f);
            Ui.Image(rt, "Bar", Chrome.Gradient(Palette.Violet, Palette.NightDeep), Color.white, Vector2.zero, new Vector2(w * 3, Height), true);
            Ui.Image(rt, "Edge", null, Palette.GoldMid.WithAlpha(0.8f), new Vector2(0, Height / 2 - 1.5f), new Vector2(w * 3, 3));
            Ui.Image(rt, "EdgeGlow", Art.Glow, Palette.Gold.WithAlpha(0.25f), new Vector2(0, Height / 2), new Vector2(w * 0.9f, 44));

            float spacing = w / Items.Length;
            for (int i = 0; i < Items.Length; i++)
            {
                var (id, icon, label) = Items[i];
                float x = (i - (Items.Length - 1) / 2f) * spacing;
                var hit = Ui.Image(rt, "Tab" + id, null, new Color(1, 1, 1, 0.001f), new Vector2(x, 0), new Vector2(spacing - 8, Height - 16), true);
                hit.gameObject.AddComponent<ClickTarget>().Clicked += () =>
                {
                    Sfx.Play(SfxKind.Button);
                    nav.Selected?.Invoke(id);
                };
                var button = hit.rectTransform;
                var tab = new Tab { Id = id };
                tab.Highlight = Ui.Sliced(button, "Highlight", Chrome.Gloss, Palette.VioletBright, new Vector2(0, -2), new Vector2(spacing - 24, Height - 36), 26);
                tab.Icon = Ui.Icon(button, "Icon", icon, Palette.Muted, new Vector2(0, 18), 64);
                tab.Label = Ui.Text(button, "Name", label, Fonts.Label, 24, Palette.Muted, new Vector2(0, -42), new Vector2(spacing, 30));
                tab.Badge = Badge.Create(button, new Vector2(52, 46), 36);
                tab.Badge.SetCount(0);
                nav.tabs.Add(tab);
            }
            return nav;
        }

        public void SetActive(HomeTabId id)
        {
            foreach (var tab in tabs)
            {
                bool on = tab.Id == id;
                tab.Highlight.enabled = on;
                tab.Icon.color = on ? Palette.Gold : Palette.Muted;
                tab.Label.color = on ? Palette.GoldLight : Palette.Muted;
                tab.Icon.rectTransform.localScale = Vector3.one * (on ? 1.12f : 1f);
            }
        }

        public void SetBadge(HomeTabId id, int count) => Find(id).Badge.SetCount(count);

        public void SetAlert(HomeTabId id, bool on) => Find(id).Badge.SetAlert(on);

        Tab Find(HomeTabId id) => tabs.Find(t => t.Id == id);
    }
}
