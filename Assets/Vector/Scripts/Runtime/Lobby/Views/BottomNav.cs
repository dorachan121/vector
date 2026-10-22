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
    /// <summary>
    /// ホーム画面の下のタブ（ホーム・ストーリー・プレイ・ミッション・ショップ）。真ん中のプレイはバーから盛り上がった大きな金のボタン。
    /// 選んだタブは光り、知らせがあればバッジを出す。画面の下端に付く。
    /// </summary>
    public sealed class BottomNav
    {
        public const float Height = 168f;

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
            (HomeTabId.Home, IconKind.Home, "ホーム"),
            (HomeTabId.Story, IconKind.Story, "ストーリー"),
            (HomeTabId.Play, IconKind.Play, "プレイ"),
            (HomeTabId.Missions, IconKind.Missions, "ミッション"),
            (HomeTabId.Shop, IconKind.Shop, "ショップ"),
        };

        readonly List<Tab> tabs = new List<Tab>();

        public event Action<HomeTabId> Selected;

        public static BottomNav Create(Transform parent)
        {
            var nav = new BottomNav();
            float w = Ui.Portrait.x;
            var rt = Ui.Edge(parent, "Nav", false, w, Height);
            // 帯は横に長めに敷き、縦長の画面を横に広げた PC のウィンドウでも途切れないようにする
            Ui.Shadow(rt, "Shadow", new Vector2(0, 10), new Vector2(w * 3, Height), 20, 0.7f);
            Ui.Image(rt, "Bar", Chrome.Gradient(Theme.Hex("2B1460"), Palette.NightDeep), Color.white, Vector2.zero, new Vector2(w * 3, Height), true);
            // 縦に長い端末で画面の枠の下に余った所も、帯の下端の色で続ける
            Ui.Image(rt, "BarBelow", null, Palette.NightDeep, new Vector2(0, -Height / 2 - ScreenFrame.Bleed / 2), new Vector2(w * 3, ScreenFrame.Bleed), true);
            Ui.Image(rt, "Edge", null, Palette.GoldMid, new Vector2(0, Height / 2 - 2), new Vector2(w * 3, 4));
            Ui.Image(rt, "EdgeGlow", Art.Glow, Palette.Gold.WithAlpha(0.3f), new Vector2(0, Height / 2), new Vector2(w, 50));

            float spacing = w / Items.Length;
            for (int i = 0; i < Items.Length; i++)
            {
                var (id, icon, label) = Items[i];
                float x = (i - (Items.Length - 1) / 2f) * spacing;
                var tab = id == HomeTabId.Play ? Center(rt, x, icon, label) : Side(rt, x, spacing, icon, label);
                tab.Id = id;
                var hit = Ui.Image(rt, "Tab" + id, null, new Color(1, 1, 1, 0.001f), new Vector2(x, id == HomeTabId.Play ? 30 : 0), new Vector2(spacing - 6, id == HomeTabId.Play ? Height + 60 : Height - 12), true);
                hit.gameObject.AddComponent<ClickTarget>().Clicked += () =>
                {
                    Sfx.Play(SfxKind.Button);
                    nav.Selected?.Invoke(id);
                };
                tab.Badge.SetCount(0);
                nav.tabs.Add(tab);
            }
            return nav;
        }

        static Tab Side(RectTransform rt, float x, float spacing, IconKind icon, string label)
        {
            var tab = new Tab();
            tab.Highlight = Ui.Image(rt, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.35f), new Vector2(x, 10), new Vector2(spacing, Height));
            tab.Icon = Ui.Icon(rt, "Icon", icon, Palette.Muted, new Vector2(x, 20), 66);
            tab.Label = Ui.Text(rt, "Name", label, Fonts.Body, 24, Palette.Muted, new Vector2(x, -44), new Vector2(spacing, 32));
            tab.Badge = Badge.Create(rt, new Vector2(x + 44, 52), 38);
            return tab;
        }

        /// <summary>真ん中の盛り上がったボタン（金の円・回る光・プレイのアイコン）。</summary>
        static Tab Center(RectTransform rt, float x, IconKind icon, string label)
        {
            var tab = new Tab();
            var at = new Vector2(x, 42);
            const float size = 196;
            var rays = Ui.Image(rt, "Rays", Chrome.Rays(12), Palette.GoldLight.WithAlpha(0.35f), at, Vector2.one * 320);
            Spin.Add(rays, 12f);
            tab.Highlight = rays;
            Ui.Shadow(rt, "CenterShadow", at + new Vector2(0, -8), Vector2.one * size, size / 2, 0.7f);
            Ui.Sliced(rt, "CenterRing", Chrome.Gloss, Theme.Hex("2B1460"), at, Vector2.one * (size + 22), (size + 22) / 2);
            Ui.Sliced(rt, "CenterFace", Chrome.Gloss, Palette.Gold, at, Vector2.one * size, size / 2);
            Ui.Sliced(rt, "CenterShine", Chrome.Shine, Color.white, at, Vector2.one * size, size / 2);
            Ui.Sliced(rt, "CenterRim", Chrome.Rim, Palette.GoldLight, at, Vector2.one * size, size / 2);
            tab.Icon = Ui.Icon(rt, "CenterIcon", icon, Palette.Ink, at + new Vector2(8, 14), 84);
            tab.Label = Ui.Text(rt, "CenterName", label, Fonts.Body, 28, Palette.Ink, at + new Vector2(0, -54), new Vector2(180, 36));
            tab.Badge = Badge.Create(rt, at + new Vector2(70, 70), 40);
            return tab;
        }

        public void SetActive(HomeTabId id)
        {
            foreach (var tab in tabs)
            {
                bool on = tab.Id == id;
                if (tab.Id == HomeTabId.Play)
                {
                    tab.Highlight.color = Palette.GoldLight.WithAlpha(on ? 0.6f : 0.3f);
                    continue;
                }
                tab.Highlight.enabled = on;
                tab.Icon.color = on ? Palette.GoldLight : Palette.Muted;
                tab.Label.color = on ? Palette.GoldLight : Palette.Muted;
                tab.Icon.rectTransform.localScale = Vector3.one * (on ? 1.14f : 1f);
            }
        }

        public void SetBadge(HomeTabId id, int count) => Find(id).Badge.SetCount(count);

        public void SetAlert(HomeTabId id, bool on) => Find(id).Badge.SetAlert(on);

        Tab Find(HomeTabId id) => tabs.Find(t => t.Id == id);
    }
}
