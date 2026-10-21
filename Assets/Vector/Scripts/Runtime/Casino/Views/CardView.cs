using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>
    /// 1枚のトランプ。表面は角にランクとスート、中央に大きなスート、A/J/Q/K には点数を添える。
    /// </summary>
    public sealed class CardView : MonoBehaviour
    {
        public static readonly Vector2 Size = new Vector2(100, 140);
        const float Radius = 9f;

        GameObject front;
        Image back;
        Image glow;
        Image ownerRing;
        Image dim;
        TextMeshProUGUI rankTop;
        TextMeshProUGUI rankBottom;
        TextMeshProUGUI valueTag;
        Image suitTop;
        Image suitBottom;
        Image pip;

        public Card Card { get; private set; }
        public bool FaceUp { get; private set; }
        public RectTransform Rect => (RectTransform)transform;
        public ClickTarget Click { get; private set; }

        public static CardView Create(Transform parent, string name, Vector2 position)
        {
            var rt = Ui.Rect(parent, name, position, Size);
            var view = rt.gameObject.AddComponent<CardView>();
            view.Build(rt);
            return view;
        }

        void Build(RectTransform rt)
        {
            glow = Ui.Image(rt, "Glow", Art.Glow, Color.clear, Vector2.zero, Size * 1.7f);
            Ui.Panel(rt, "Shadow", new Vector2(3, -4), Size, new Color(0, 0, 0, 0.35f), Radius);

            var face = Ui.Panel(rt, "Face", Vector2.zero, Size, Theme.CardFace, Radius, true);
            front = face.gameObject;
            Ui.Outline(face.transform, "Border", Vector2.zero, Size, new Color(0, 0, 0, 0.18f), Radius);
            float cx = Size.x / 2 - 20, cy = Size.y / 2 - 22;
            rankTop = Ui.Text(face.transform, "RankTop", "", Fonts.Label, 32, Theme.CardBlack, new Vector2(-cx, cy), new Vector2(44, 36));
            suitTop = Ui.Image(face.transform, "SuitTop", null, Theme.CardBlack, new Vector2(-cx, cy - 26), new Vector2(18, 18));
            rankBottom = Ui.Text(face.transform, "RankBottom", "", Fonts.Label, 32, Theme.CardBlack, new Vector2(cx, -cy), new Vector2(44, 36));
            rankBottom.rectTransform.localEulerAngles = new Vector3(0, 0, 180);
            suitBottom = Ui.Image(face.transform, "SuitBottom", null, Theme.CardBlack, new Vector2(cx, -cy + 26), new Vector2(18, 18));
            suitBottom.rectTransform.localEulerAngles = new Vector3(0, 0, 180);
            pip = Ui.Image(face.transform, "Pip", null, Theme.CardBlack, new Vector2(0, 4), new Vector2(52, 52));
            valueTag = Ui.Text(face.transform, "Value", "", Fonts.Label, 14, Theme.Muted, new Vector2(cx - 2, cy + 4), new Vector2(50, 18));

            back = Ui.Image(rt, "Back", Art.CardBack, Color.white, Vector2.zero, Size, true);
            ownerRing = Ui.Outline(rt, "Owner", Vector2.zero, Size + new Vector2(8, 8), Color.clear, Radius + 4);
            dim = Ui.Panel(rt, "Dim", Vector2.zero, Size, new Color(0, 0, 0, 0f), Radius);

            Click = rt.gameObject.AddComponent<ClickTarget>();
            SetFaceUp(false);
        }

        public void SetCard(Card card)
        {
            Card = card;
            var color = card.IsRed ? Theme.CardRed : Theme.CardBlack;
            var suit = Art.Suit(card.Suit);
            rankTop.text = rankBottom.text = card.RankLabel;
            rankTop.color = rankBottom.color = color;
            suitTop.sprite = suitBottom.sprite = pip.sprite = suit;
            suitTop.color = suitBottom.color = pip.color = color;
            bool faceOrAce = card.Rank == Rank.Ace || card.Rank >= Rank.Jack;
            valueTag.text = faceOrAce ? card.Value + " PTS" : "";
        }

        public void SetFaceUp(bool up)
        {
            FaceUp = up;
            front.SetActive(up);
            back.gameObject.SetActive(!up);
        }

        /// <summary>カードを返す。横に潰して面を入れ替え、また広げる。</summary>
        public void Flip(bool toFaceUp, float duration = 0.28f)
        {
            bool swapped = false;
            Tween.Run(this, "flip", duration, Ease.InOutQuad, t =>
            {
                if (!swapped && t >= 0.5f)
                {
                    swapped = true;
                    SetFaceUp(toFaceUp);
                }
                float x = Mathf.Abs(1 - 2 * t);
                Rect.localScale = new Vector3(Mathf.Max(0.02f, x), 1, 1) * (1 + 0.08f * (1 - x));
            }, () => Rect.localScale = Vector3.one);
        }

        /// <summary>盤面上で誰が置いたカードかを縁の色で示す。</summary>
        public void SetOwner(Seat seat)
        {
            switch (seat)
            {
                case Seat.Player: ownerRing.color = Theme.PlayerBlue; break;
                case Seat.Dealer: ownerRing.color = Theme.DealerRed; break;
                case Seat.House: ownerRing.color = Theme.Gold; break;
                default: ownerRing.color = Color.clear; break;
            }
        }

        public void SetGlow(Color color, float alpha = 0.75f) => Tween.Tint(glow, color.WithAlpha(alpha), 0.25f);

        public void ClearGlow() => Tween.Tint(glow, Color.clear, 0.25f);

        public void SetDimmed(bool dimmed) => Tween.Tint(dim, new Color(0, 0, 0, dimmed ? 0.45f : 0f), 0.3f);
    }
}
