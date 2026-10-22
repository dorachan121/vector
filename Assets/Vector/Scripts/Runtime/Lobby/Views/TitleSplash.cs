using System;
using TMPro;
using UnityEngine;
using Vector.Game.Audio;
using Vector.Game.UI;
using Vector.Game.World;

namespace Vector.Game.Lobby
{
    /// <summary>起動して最初に出すタイトル（ロゴ・回る光・看板の2人・TAP TO START）。押すと明けてホームになる。</summary>
    public sealed class TitleSplash : MonoBehaviour
    {
        CanvasGroup group;
        TextMeshProUGUI prompt;
        Action done;
        bool closing;

        public static TitleSplash Show(Transform parent, Action done)
        {
            var cover = Ui.GradientCover(parent, "Title", Palette.VioletBright, Palette.NightDeep, true);
            var splash = cover.gameObject.AddComponent<TitleSplash>();
            splash.done = done;
            splash.group = Ui.Group(cover.gameObject);
            var root = cover.rectTransform;

            var rays = Ui.Image(root, "Rays", Chrome.Rays(20), Palette.Gold.WithAlpha(0.2f), new Vector2(0, 420), new Vector2(1700, 1700));
            Spin.Add(rays, 6f);
            Ui.Image(root, "Glow", Art.Glow, Palette.Magenta.WithAlpha(0.28f), new Vector2(0, 200), new Vector2(1300, 1500));
            PortraitView.Create(root, "Zero", Cast.Of("zero").Look, new Vector2(250, -360), 1000);
            PortraitView.Create(root, "Lena", Cast.Of("lena").Look, new Vector2(-250, -400), 1000);
            var twinkles = Ui.Rect(root, "Twinkles", new Vector2(0, 480), new Vector2(1000, 700));
            Twinkle.Add(twinkles, 14, Palette.GoldLight, 60);

            var logo = Ui.Headline(root, "Logo", "VECTOR", Fonts.Display, 210, new Vector2(0, 600), new Vector2(1060, 240));
            logo.characterSpacing = 12;
            Ui.Outline(logo, Palette.Ink, 0.22f);
            var sub = Ui.Text(root, "Sub", "CASINO RESORT", Fonts.Label, 48, Palette.Ivory, new Vector2(0, 452), new Vector2(1000, 60));
            sub.characterSpacing = 22;
            Ui.Outline(sub, Palette.Ink, 0.2f);
            Ui.Ribbon(root, "Tagline", "TEN ORIGINAL GAMES  ·  CASINO RPG", new Vector2(0, 352), 760, Palette.Ruby, 28);

            Ui.Image(root, "Shade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0f), Palette.NightDeep), Color.white, new Vector2(0, -820), new Vector2(Ui.Portrait.x * 3, 760));
            // 看板の2人の服に重なっても読めるよう、文字の後ろに暗い帯を敷く
            Ui.Panel(root, "TapPlate", new Vector2(0, -690), new Vector2(640, 96), Palette.NightDeep.WithAlpha(0.78f), 48);
            splash.prompt = Ui.Text(root, "Tap", "TAP TO START", Fonts.Label, 54, Palette.GoldLight, new Vector2(0, -690), new Vector2(900, 70));
            splash.prompt.characterSpacing = 12;
            Ui.Outline(splash.prompt, Palette.Ink, 0.22f);
            Ui.Text(root, "Notice", "このゲームのチップは遊戯用です。現金・景品とは交換できません。", Fonts.Body, 20, Palette.Muted, new Vector2(0, -850), new Vector2(1000, 28));
            Ui.Text(root, "NoticeEn", "PLAY MONEY ONLY — NO REAL-MONEY GAMBLING", Fonts.Label, 18, Palette.Muted.WithAlpha(0.8f), new Vector2(0, -886), new Vector2(1000, 24));
            var click = cover.gameObject.AddComponent<ClickTarget>();
            click.Clicked += splash.Close;
            return splash;
        }

        void Update()
        {
            prompt.alpha = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 3.2f);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                Close();
        }

        void Close()
        {
            if (closing)
                return;
            closing = true;
            Sfx.Play(SfxKind.Sparkle);
            group.blocksRaycasts = false;
            Tween.Fade(group, 0, 0.45f, Ease.OutQuad, () =>
            {
                Destroy(gameObject);
                done?.Invoke();
            });
        }
    }
}
