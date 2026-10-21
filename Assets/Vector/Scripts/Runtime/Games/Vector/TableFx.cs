using System.Collections;
using UnityEngine;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>カードを配る・手札から置く・片付けるといった演出をまとめたもの（チップの演出は CasinoDirector が受け持つ）。</summary>
    public sealed class TableFx
    {
        const float DealSeconds = 0.32f;
        const float FlipSeconds = 0.28f;

        readonly TableView view;

        public TableFx(TableView view) => this.view = view;

        /// <summary>シューから盤面のマスへ配る。</summary>
        public IEnumerator DealToBoard(Card card, int cell, Seat owner)
        {
            var cardView = SpawnAtShoe(card);
            view.Board.Attach(cardView, cell, owner);
            Tween.Move(cardView.Rect, BoardView.CellPosition(cell), DealSeconds);
            yield return Tween.Wait(DealSeconds);
            cardView.Flip(true, FlipSeconds);
            Sfx.Play(SfxKind.CardFlip);
            yield return Tween.Wait(FlipSeconds);
        }

        /// <summary>シューから手札へ配る。</summary>
        public IEnumerator DealToHand(Card card, int index)
        {
            var cardView = SpawnAtShoe(card);
            view.Hand.Add(cardView);
            Tween.Move(cardView.Rect, HandView.SlotPosition(index, RoundEngine.HandSize), DealSeconds);
            yield return Tween.Wait(DealSeconds * 0.6f);
            cardView.Flip(true, FlipSeconds);
            Sfx.Play(SfxKind.CardFlip, 0.6f);
        }

        /// <summary>手札の1枚を盤面のマスへ移す。</summary>
        public IEnumerator MoveFromHand(int handIndex, int cell, Seat owner)
        {
            var cardView = view.Hand.Take(handIndex);
            view.Board.Attach(cardView, cell, owner);
            Sfx.Play(SfxKind.CardDeal);
            Tween.Move(cardView.Rect, BoardView.CellPosition(cell), DealSeconds, Ease.OutCubic);
            yield return Tween.Wait(DealSeconds);
        }

        /// <summary>盤面と手札のカードを捨て札置き場へ集めて消す。</summary>
        public IEnumerator CollectCards()
        {
            var cards = view.Board.DetachAll();
            cards.AddRange(view.Hand.TakeAll());
            if (cards.Count == 0)
                yield break;
            Sfx.Play(SfxKind.CardDeal);
            var target = view.Casino.FxPoint(view.Discard.position);
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                card.transform.SetParent(view.Casino.Fx, true);
                card.Flip(false, 0.2f);
                Tween.Move(card.Rect, target, 0.35f, Ease.InCubic, () => Object.Destroy(card.gameObject), i * 0.03f);
            }
            yield return Tween.Wait(0.35f + cards.Count * 0.03f);
        }

        CardView SpawnAtShoe(Card card)
        {
            var cardView = CardView.Create(view.Casino.Fx, "Card " + card, view.Casino.FxPoint(view.Shoe.DealPoint.position));
            cardView.SetCard(card);
            cardView.Rect.localEulerAngles = new Vector3(0, 0, 90);
            Tween.Rotate(cardView.transform, 0f, DealSeconds);
            Sfx.Play(SfxKind.CardDeal);
            return cardView;
        }
    }
}
