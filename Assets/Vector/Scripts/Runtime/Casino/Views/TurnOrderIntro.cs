using System;
using System.Collections;
using UnityEngine;
using Vector.Game.Audio;
using Vector.Game.UI;
using Vector.Game.World;

namespace Vector.Game.Casino
{
    /// <summary>
    /// 先攻・後攻のあるゲームのラウンドの始まりの演出。左右から2人の札が滑り込み、回る「1ST」のボタンが先攻の側に置かれて、
    /// 先攻・後攻の帯が出る。順番はゲームの規則で決まっていて（いつもプレイヤーが先）、演出はそれを見せるだけ。
    /// テーブルの最初のラウンドは大きく、以降のラウンドは短い帯だけにする。タップで飛ばせる。
    /// </summary>
    public static class TurnOrderIntro
    {
        static readonly Vector2 PlateSize = new Vector2(300, 360);
        const float PlateX = 470;

        public static IEnumerator Play(Transform parent, TurnSeat first, TurnSeat second, bool full)
        {
            var hit = Ui.Image(parent, "TurnOrder", null, new Color(0, 0, 0, 0.001f), Vector2.zero, Vector2.zero, true);
            var root = hit.rectTransform;
            Ui.Cover(root);
            var group = Ui.Group(hit.gameObject);
            bool skipped = false;
            hit.gameObject.AddComponent<ClickTarget>().Clicked += () => skipped = true;
            group.alpha = 0;
            Tween.Fade(group, 1, 0.15f);
            if (full)
                yield return Full(root, first, second, () => skipped);
            else
                yield return Short(root, first, () => skipped);
            Tween.Fade(group, 0, 0.25f, Ease.OutQuad, () => UnityEngine.Object.Destroy(root.gameObject));
            yield return Tween.Wait(0.25f);
        }

        static IEnumerator Full(RectTransform root, TurnSeat first, TurnSeat second, Func<bool> skipped)
        {
            Ui.Image(root, "Dim", null, new Color(0.02f, 0.01f, 0.06f, 0.72f), Vector2.zero, new Vector2(Ui.Landscape.x * 2, Ui.Landscape.y * 2));
            var rays = Ui.Image(root, "Rays", Chrome.Rays(18), Palette.Gold.WithAlpha(0.22f), Vector2.zero, Vector2.one * 1100);
            Spin.Add(rays, 14f);
            var title = Ui.Ribbon(root, "Title", "TURN ORDER", new Vector2(0, 330), 620, Palette.Ruby, 40);
            Pop(title.transform.parent, 0);

            var left = Plate(root, first, new Vector2(-1300, 10));
            var right = Plate(root, second, new Vector2(1300, 10));
            Tween.Move(left, new Vector2(-PlateX, 10), 0.4f, Ease.OutBack);
            Tween.Move(right, new Vector2(PlateX, 10), 0.4f, Ease.OutBack);
            Sfx.Play(SfxKind.CardDeal);
            var vs = Ui.Headline(root, "Vs", "VS", Fonts.Display, 150, new Vector2(0, 20), new Vector2(320, 170));
            Pop(vs.transform, 0.35f);
            yield return Wait(0.75f, skipped);

            // 「1ST」のボタンが回りながら先攻の札の上へ
            var button = Ui.Rect(root, "Button", new Vector2(0, -200), Vector2.one * 150);
            Ui.Shadow(button, "Shadow", new Vector2(0, -6), Vector2.one * 150, 75, 0.6f);
            Ui.Image(button, "Disc", Art.Circle, Palette.Ivory, Vector2.zero, Vector2.one * 150);
            Ui.Image(button, "Ring", Art.Ring(0.08f), Palette.GoldMid, Vector2.zero, Vector2.one * 150);
            Ui.Text(button, "Label", "1ST", Fonts.Display, 56, Palette.Ink, new Vector2(0, 2), new Vector2(140, 70));
            Sfx.Play(SfxKind.Chip);
            for (float t = 0; t < 0.7f && !skipped(); t += Time.unscaledDeltaTime * Tween.Speed)
            {
                button.localScale = new Vector3(Mathf.Cos(t * 22f), 1, 1);
                yield return null;
            }
            button.localScale = Vector3.one;
            var spot = new Vector2(-PlateX + PlateSize.x / 2 - 20, 10 + PlateSize.y / 2 - 10);
            Tween.Move(button, spot, 0.35f, Ease.OutBack);
            yield return Wait(0.35f, skipped);
            Sfx.Play(SfxKind.ChipsCollect);
            ParticleBurst.Sparkles(root, spot, Palette.GoldLight, 16, 360f);

            Tag(left, "先攻  FIRST", Palette.Gold, Palette.Ink);
            Tag(right, "後攻  SECOND", Palette.Violet, Palette.Ivory);
            yield return Wait(1.1f, skipped);
        }

        static IEnumerator Short(RectTransform root, TurnSeat first, Func<bool> skipped)
        {
            var band = Ui.Rect(root, "Band", new Vector2(0, 40), new Vector2(900, 150));
            Ui.Image(band, "Glow", Art.Glow, Palette.Gold.WithAlpha(0.35f), Vector2.zero, new Vector2(1100, 260));
            Ui.Ribbon(band, "Ribbon", "ROUND START", new Vector2(0, 18), 700, Palette.Ruby, 40);
            Ui.Text(band, "Turn", $"先攻  {first.Name}", Fonts.Body, 30, Palette.GoldLight, new Vector2(0, -52), new Vector2(700, 40));
            Pop(band, 0);
            Sfx.Play(SfxKind.Chip);
            yield return Wait(0.85f, skipped);
        }

        /// <summary>1人ぶんの札（色の地・顔・名前）。</summary>
        static RectTransform Plate(RectTransform parent, TurnSeat seat, Vector2 position)
        {
            var rt = Ui.Card(parent, "Seat" + seat.Name, position, PlateSize, Color.Lerp(seat.Color, Palette.NightDeep, 0.35f), 30);
            Ui.Image(rt, "Glow", Art.Glow, seat.Color.WithAlpha(0.6f), new Vector2(0, 40), new Vector2(320, 320));
            var window = Ui.Clip(rt, "Face", new Vector2(0, 34), new Vector2(PlateSize.x - 30, PlateSize.y - 110));
            if (seat.Look.HasValue)
                PortraitView.Create(window, "Bust", seat.Look.Value, new Vector2(0, -40), 330);
            else
                Ui.Image(window, "Picture", seat.Picture, Color.white, new Vector2(0, 0), Vector2.one * 220).preserveAspect = true;
            var name = Ui.Text(rt, "Name", seat.Name, Fonts.Label, 40, Palette.Ivory, new Vector2(0, -PlateSize.y / 2 + 40), new Vector2(PlateSize.x - 20, 50));
            Ui.Outline(name, Palette.Ink, 0.22f);
            name.enableAutoSizing = true;
            name.fontSizeMin = 22;
            name.fontSizeMax = 40;
            return rt;
        }

        static void Tag(RectTransform plate, string text, Color color, Color textColor)
        {
            var tag = Ui.Rect(plate, "Tag", new Vector2(0, -PlateSize.y / 2 - 50), new Vector2(300, 70));
            Ui.Shadow(tag, "Shadow", new Vector2(0, -4), new Vector2(300, 70), 35, 0.55f);
            Ui.Sliced(tag, "Face", Chrome.Gloss, color, Vector2.zero, new Vector2(300, 70), 35);
            Ui.Sliced(tag, "Shine", Chrome.Shine, Color.white, Vector2.zero, new Vector2(300, 70), 35);
            var label = Ui.Text(tag, "Label", text, Fonts.Body, 32, textColor, new Vector2(0, 1), new Vector2(290, 60));
            if (textColor.grayscale > 0.5f)
                Ui.Outline(label, Palette.Ink, 0.2f);
            Pop(tag, 0);
        }

        static void Pop(Transform t, float delay)
        {
            t.localScale = Vector3.zero;
            Tween.Scale(t, Vector3.one, 0.35f, Ease.OutBack, null, delay);
        }

        /// <summary>演出の速さ（高速ディール）に合わせて待つ。タップされたらすぐ終える。</summary>
        static IEnumerator Wait(float seconds, Func<bool> skipped)
        {
            for (float t = 0; t < seconds && !skipped(); t += Time.unscaledDeltaTime * Tween.Speed)
                yield return null;
        }
    }
}
