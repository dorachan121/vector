using System;
using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>
    /// 大勝ちの演出（回る光・降り注ぐ金貨・数え上がる勝ち額）。操作は止めず、しばらくすると消える。タップでもすぐ消せる。
    /// </summary>
    public sealed class BigWinBanner : MonoBehaviour
    {
        const float ShowSeconds = 2.4f;

        CanvasGroup group;
        float shown;
        bool closing;

        public static void Show(Transform parent, long amount)
        {
            var rt = Ui.Rect(parent, "BigWin", new Vector2(0, 60), new Vector2(1100, 420));
            var banner = rt.gameObject.AddComponent<BigWinBanner>();
            banner.group = Ui.Group(rt.gameObject);
            var rays = Ui.Image(rt, "Rays", Chrome.Rays(20), Palette.Gold.WithAlpha(0.4f), Vector2.zero, Vector2.one * 900);
            Spin.Add(rays, 24f);
            Ui.Image(rt, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.55f), Vector2.zero, new Vector2(1000, 520));
            var title = Ui.Headline(rt, "Title", "BIG WIN", Fonts.Display, 170, new Vector2(0, 60), new Vector2(1000, 190));
            title.characterSpacing = 10;
            var value = Ui.Headline(rt, "Amount", "", Fonts.Display, 90, new Vector2(0, -100), new Vector2(1000, 110));
            Tween.Run(value, "count", 1.1f, Ease.OutCubic, t => value.text = "+" + Labels.Money((long)Math.Round(amount * (double)t)));
            rt.localScale = Vector3.one * 0.4f;
            Tween.Scale(rt, Vector3.one, 0.45f, Ease.OutBack);
            ParticleBurst.Coins(parent, new Vector2(1700, 900), 70);
            var hit = Ui.Image(rt, "Hit", null, new Color(0, 0, 0, 0.001f), Vector2.zero, new Vector2(1100, 420), true);
            hit.gameObject.AddComponent<ClickTarget>().Clicked += banner.Close;
        }

        void Update()
        {
            shown += Time.unscaledDeltaTime;
            if (shown >= ShowSeconds)
                Close();
        }

        void Close()
        {
            if (closing)
                return;
            closing = true;
            Tween.Fade(group, 0, 0.3f, Ease.OutQuad, () => Destroy(gameObject));
        }
    }
}
