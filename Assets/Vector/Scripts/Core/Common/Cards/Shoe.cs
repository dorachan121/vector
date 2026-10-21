using System;

namespace Vector.Core
{
    /// <summary>
    /// 複数デッキを束ねたシュー。Fisher–Yates でシャッフルし、カットカードに達したら次のラウンド前に再シャッフルする。
    /// </summary>
    public sealed class Shoe
    {
        /// <summary>カットカードを入れる位置の範囲（シュー全体に対する割合）。</summary>
        const float CutCardMin = 0.70f;
        const float CutCardMax = 0.80f;

        readonly IRandomSource random;
        readonly Card[] cards;
        int position;
        int cutCardIndex;

        public int TotalCards => cards.Length;
        public int Remaining => cards.Length - position;

        /// <summary>カットカードが出ていれば、現在のラウンド終了後にシャッフルが必要。</summary>
        public bool CutCardReached => position >= cutCardIndex;

        /// <summary>このセッションで何番目のシューか（1始まり）。</summary>
        public int ShoeNumber { get; private set; }

        /// <summary>シャッフル直後に捨てたカード（カジノの慣例のバーンカード）。</summary>
        public Card BurnedCard { get; private set; }

        public Shoe(int deckCount, IRandomSource random)
        {
            if (deckCount < 1)
                throw new ArgumentOutOfRangeException(nameof(deckCount));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            cards = new Card[deckCount * Card.DistinctCount];
            for (int i = 0; i < cards.Length; i++)
                cards[i] = Card.FromId(i % Card.DistinctCount);
            Shuffle();
        }

        /// <summary>全カードを回収してシャッフルし、カットカードを入れて1枚バーンする。</summary>
        public void Shuffle()
        {
            for (int i = cards.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
            position = 0;
            ShoeNumber++;

            int min = (int)(cards.Length * CutCardMin);
            int max = (int)(cards.Length * CutCardMax);
            cutCardIndex = min + random.Next(max - min + 1);
            BurnedCard = Draw();
        }

        public Card Draw()
        {
            if (position >= cards.Length)
                throw new InvalidOperationException("シューにカードが残っていません。");
            return cards[position++];
        }
    }
}
