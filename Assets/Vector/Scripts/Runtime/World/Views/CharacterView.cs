using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Rpg;
using Vector.Game.UI;

namespace Vector.Game.World
{
    /// <summary>
    /// フロアを歩く人（二頭身）。顔・髪・服の色と、向きに合わせた目の位置で描く。話しかけられるときは頭の上に「!」。
    /// </summary>
    public sealed class CharacterView : MonoBehaviour
    {
        Image hair;
        Image hairBack;
        Image[] eyes;
        Image bubble;
        TextMeshProUGUI bubbleText;
        TextMeshProUGUI nameTag;
        RectTransform figure;
        float size;
        bool nameShown;

        public RectTransform Rect => (RectTransform)transform;
        public int Facing { get; private set; }

        public static CharacterView Create(Transform parent, string name, CharacterLook look, float size, Vector2 position)
        {
            var rt = Ui.Rect(parent, name, position, new Vector2(size, size));
            var view = rt.gameObject.AddComponent<CharacterView>();
            view.size = size;
            if (look.Master)
            {
                var aura = Ui.Image(rt, "Aura", Art.Ring(0.1f), Theme.GoldBright.WithAlpha(0.75f), new Vector2(0, -size * 0.4f), new Vector2(size * 0.95f, size * 0.32f));
                Flicker.Add(aura);
            }
            Ui.Image(rt, "Shadow", Art.Circle, new Color(0, 0, 0, 0.35f), new Vector2(0, -size * 0.42f), new Vector2(size * 0.66f, size * 0.2f));
            view.figure = Ui.Rect(rt, "Figure", Vector2.zero, new Vector2(size, size));
            var f = view.figure;
            Ui.Panel(f, "Body", new Vector2(0, -size * 0.17f), new Vector2(size * 0.56f, size * 0.46f), look.Outfit, size * 0.16f);
            Ui.Panel(f, "Collar", new Vector2(0, -size * 0.0f), new Vector2(size * 0.3f, size * 0.1f), look.Accent, size * 0.05f);
            view.hairBack = Ui.Image(f, "HairBack", Art.Circle, look.Hair, new Vector2(0, size * 0.2f), Vector2.one * size * 0.54f);
            Ui.Image(f, "Head", Art.Circle, look.Skin, new Vector2(0, size * 0.19f), Vector2.one * size * 0.5f);
            view.hair = Ui.Image(f, "Hair", Art.HairCap, look.Hair, new Vector2(0, size * 0.25f), Vector2.one * size * 0.56f);
            view.eyes = new[]
            {
                Ui.Image(f, "EyeL", Art.Circle, Theme.Ink, Vector2.zero, Vector2.one * size * 0.075f),
                Ui.Image(f, "EyeR", Art.Circle, Theme.Ink, Vector2.zero, Vector2.one * size * 0.075f),
            };
            if (look.Crown)
                Ui.Image(f, "Crown", Art.Gem(4), Theme.GoldBright, new Vector2(0, size * 0.5f), Vector2.one * size * 0.26f);

            view.bubble = Ui.Panel(rt, "Bubble", new Vector2(0, size * 0.78f), new Vector2(size * 0.42f, size * 0.42f), Theme.GoldBright, size * 0.21f);
            view.bubbleText = Ui.Text(view.bubble.transform, "Mark", "!", Fonts.Display, size * 0.34f, Theme.Ink, Vector2.zero, new Vector2(size * 0.42f, size * 0.42f));
            view.bubble.gameObject.SetActive(false);
            view.nameTag = Ui.Text(rt, "Name", "", Fonts.Body, 18, Theme.Ivory, new Vector2(0, -size * 0.62f), new Vector2(200, 24));
            view.nameTag.outlineWidth = 0.25f;
            view.nameTag.outlineColor = new Color(0, 0, 0, 0.9f);
            view.nameTag.alpha = 0;
            view.SetFacing(Directions.Down);
            return view;
        }

        public void SetFacing(int facing)
        {
            Facing = facing;
            bool up = facing == Directions.Up;
            hairBack.enabled = up;
            hair.enabled = !up;
            float eyeY = size * 0.16f;
            float spread = size * 0.09f;
            float shift = facing == Directions.Left ? -size * 0.07f : facing == Directions.Right ? size * 0.07f : 0;
            eyes[0].enabled = eyes[1].enabled = !up;
            eyes[0].rectTransform.anchoredPosition = new Vector2(-spread + shift, eyeY);
            eyes[1].rectTransform.anchoredPosition = new Vector2(spread + shift, eyeY);
        }

        /// <summary>1歩ぶんの弾み（歩いているように見せる）。</summary>
        public void Bob(float seconds)
        {
            Tween.Run(figure, "bob", seconds, Ease.Linear, t => figure.anchoredPosition = new Vector2(0, Mathf.Sin(t * Mathf.PI) * size * 0.08f),
                () => figure.anchoredPosition = Vector2.zero);
        }

        /// <summary>頭の上の吹き出し（mark は "!" や "?"、null で消す）。</summary>
        public void SetBubble(string mark)
        {
            bubble.gameObject.SetActive(mark != null);
            if (mark != null)
                bubbleText.text = mark;
        }

        public void SetName(string text) => nameTag.text = text;

        public void ShowName(bool visible)
        {
            if (visible == nameShown)
                return;
            nameShown = visible;
            float from = nameTag.alpha, to = visible ? 1 : 0;
            Tween.Run(nameTag, "fade", 0.15f, Ease.OutQuad, t => nameTag.alpha = Mathf.Lerp(from, to, t));
        }
    }
}
