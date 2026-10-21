using System;
using System.Collections.Generic;
using UnityEngine;
using Vector.Game.Audio;

namespace Vector.Game.UI.Widgets
{
    /// <summary>
    /// 報酬を受け取ったときの演出（回る光・弾む報酬のアイコン・紙吹雪）。中身はダイアログなので、開いている間はテーブルの持ち時間も止まる。
    /// </summary>
    public static class RewardPopup
    {
        public static Dialog Show(Transform root, string title, string message, IReadOnlyList<Reward> rewards, Action closed = null)
        {
            var dialog = Dialog.Open(root, title, message, new Vector2(900, 600));
            var content = dialog.Content;
            var rays = Ui.Image(content, "Rays", Chrome.Rays(18), Palette.Gold.WithAlpha(0.35f), new Vector2(0, 8), new Vector2(520, 520));
            Spin.Add(rays, 18f);
            float spacing = 220f;
            for (int i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                var x = (i - (rewards.Count - 1) / 2f) * spacing;
                var slot = Ui.Rect(content, "Reward" + i, new Vector2(x, 20), new Vector2(180, 200));
                Ui.Image(slot, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.45f), new Vector2(0, 20), new Vector2(220, 220));
                var icon = Ui.Image(slot, "Icon", reward.Icon, reward.Tint, new Vector2(0, 26), new Vector2(120, 120));
                icon.preserveAspect = true;
                icon.rectTransform.localScale = Vector3.zero;
                Tween.Scale(icon.rectTransform, Vector3.one, 0.45f, Ease.OutBack, null, 0.15f + i * 0.12f);
                Float.Add(icon.rectTransform, 6f, 2f);
                var amount = Ui.Headline(slot, "Amount", reward.Amount, Fonts.Display, 44, new Vector2(0, -70), new Vector2(240, 56));
                amount.enableAutoSizing = true;
                amount.fontSizeMin = 24;
                amount.fontSizeMax = 44;
            }
            var twinkles = Ui.Rect(content, "Twinkles", Vector2.zero, content.sizeDelta);
            Twinkle.Add(twinkles, 8, Palette.GoldLight, 40);
            dialog.AddButton("OK", closed, true);
            ParticleBurst.Confetti(root, new Vector2(0, 120), 110);
            Sfx.Play(SfxKind.Sparkle);
            return dialog;
        }
    }
}
