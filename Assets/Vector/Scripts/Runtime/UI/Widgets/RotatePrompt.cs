using System.Collections;
using UnityEngine;
using Vector.Core;
using Vector.Game.Audio;

namespace Vector.Game.UI.Widgets
{
    /// <summary>
    /// 画面の向きが変わるシーンの切り替えで、暗転した画面に出す回転の案内（回る端末の絵・光の放射・行き先のひとこと）。
    /// 手に持つ端末では、新しい向きに持ち替えるか、タップするか、しばらく経つまで待つ。PC では短く見せるだけ。
    /// </summary>
    public static class RotatePrompt
    {
        const float MinSeconds = 1.2f;
        const float MaxSeconds = 4.5f;

        /// <summary>端末の絵が回る1周期（立てて見せる・回す・寝かせて見せる）。</summary>
        const float CycleSeconds = 1.8f;

        static readonly Vector2 PhoneSize = new Vector2(190, 340);

        public static IEnumerator Play(Transform parent, ScreenShape target)
        {
            bool toTable = target == ScreenShape.Landscape;
            var hit = Ui.Image(parent, "Rotate", null, new Color(0, 0, 0, 0.001f), Vector2.zero, Vector2.zero, true);
            var root = hit.rectTransform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            bool skipped = false;
            hit.gameObject.AddComponent<ClickTarget>().Clicked += () => skipped = true;

            var rays = Ui.Image(root, "Rays", Chrome.Rays(16), Palette.Gold.WithAlpha(0.22f), new Vector2(0, 60), Vector2.one * 760);
            Spin.Add(rays, 10f);
            Ui.Image(root, "Glow", Art.Glow, Palette.Magenta.WithAlpha(0.3f), new Vector2(0, 60), Vector2.one * 560);
            var phone = Phone(root, toTable);
            var arrow = Ui.Icon(root, "Arrow", IconKind.Rotate, Palette.GoldLight, new Vector2(170, 210), 86);
            // 時計回り（縦→横）か反時計回り（横→縦）かを矢印の向きで示す
            arrow.rectTransform.localScale = new Vector3(toTable ? -1 : 1, 1, 1);

            var title = Ui.Headline(root, "Title", toTable ? "TABLE READY" : "WELCOME BACK", Fonts.Display, 84, new Vector2(0, -230), new Vector2(1000, 100));
            title.characterSpacing = 8;
            var line = Ui.Text(root, "Line", toTable ? "端末を横向きにして、テーブルへどうぞ" : "端末を縦向きに戻してください", Fonts.Body, 34, Palette.Ivory, new Vector2(0, -320), new Vector2(1000, 50));
            Ui.Outline(line, Palette.Ink, 0.2f);
            bool handheld = SystemInfo.deviceType == DeviceType.Handheld;
            if (handheld)
                Ui.Text(root, "Skip", "タップでスキップ", Fonts.Body, 22, Palette.Muted, new Vector2(0, -390), new Vector2(600, 32));

            float from = toTable ? 0 : -90, to = toTable ? -90 : 0;
            float t = 0;
            while (t < MinSeconds || (t < MaxSeconds && !skipped && !OrientationLock.IsHeld(target)))
            {
                phone.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(from, to, Swing(t % CycleSeconds / CycleSeconds)));
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // 回し終えた姿を見せてから明ける
            phone.localEulerAngles = new Vector3(0, 0, to);
            var check = Ui.Rect(root, "Check", new Vector2(150, -50), Vector2.one * 96);
            Ui.Image(check, "Disc", Art.Circle, Palette.Emerald, Vector2.zero, Vector2.one * 96);
            Ui.Icon(check, "Mark", IconKind.Check, Palette.Ivory, Vector2.zero, 58);
            check.localScale = Vector3.zero;
            Tween.Scale(check, Vector3.one, 0.3f, Ease.OutBack);
            Sfx.Play(SfxKind.Chip);
            for (float wait = 0; wait < 0.35f; wait += Time.unscaledDeltaTime)
                yield return null;
            Object.Destroy(root.gameObject);
        }

        /// <summary>1周期のうちの回り具合（0 → 1）。はじめと終わりはしばらく止まって見せる。</summary>
        static float Swing(float phase)
        {
            float t = Mathf.Clamp01((phase - 0.25f) / 0.4f);
            return t * t * (3 - 2 * t);
        }

        /// <summary>端末の絵（金の縁・画面・カメラ）。画面には行き先（テーブルのカードか、ホーム）を描く。</summary>
        static RectTransform Phone(RectTransform parent, bool toTable)
        {
            var phone = Ui.Rect(parent, "Phone", new Vector2(0, 60), PhoneSize);
            Ui.Shadow(phone, "Shadow", new Vector2(0, -8), PhoneSize, 34, 0.6f);
            Ui.Sliced(phone, "Body", Chrome.Shade, Palette.NightDeep, Vector2.zero, PhoneSize, 34);
            Ui.Sliced(phone, "Frame", Chrome.Frame, Palette.Gold, Vector2.zero, PhoneSize, 34);
            var screen = PhoneSize - new Vector2(34, 70);
            Ui.Sliced(phone, "Screen", Chrome.Shade, toTable ? Theme.FeltCenter : Palette.Violet, Vector2.zero, screen, 14);
            Ui.Panel(phone, "Camera", new Vector2(0, PhoneSize.y / 2 - 20), new Vector2(46, 9), Palette.Muted.WithAlpha(0.6f), 4.5f);
            if (toTable)
            {
                // 横に寝かせたときに正しく見えるよう、カードは画面に対して横向きに並べる
                var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds };
                for (int i = 0; i < suits.Length; i++)
                {
                    var card = Ui.Panel(phone, "Card", new Vector2(0, (i - 1) * 76), new Vector2(76, 56), Theme.CardFace, 8);
                    bool red = suits[i] != Suit.Spades;
                    var suit = Ui.Image(card.rectTransform, "Suit", Art.Suit(suits[i]), red ? Theme.CardRed : Theme.CardBlack, Vector2.zero, Vector2.one * 30);
                    suit.rectTransform.localEulerAngles = new Vector3(0, 0, 90);
                }
            }
            else
            {
                Ui.Icon(phone, "Home", IconKind.Home, Palette.Gold, new Vector2(0, 20), 78);
                Ui.Image(phone, "Chip", Chrome.Coin, Color.white, new Vector2(0, -70), Vector2.one * 52);
            }
            return phone;
        }
    }
}
