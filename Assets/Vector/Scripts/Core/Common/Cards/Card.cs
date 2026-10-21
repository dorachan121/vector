using System;

namespace Vector.Core
{
    /// <summary>トランプ1枚。不変の値型。</summary>
    public readonly struct Card : IEquatable<Card>
    {
        public const int DistinctCount = 52;
        const int RanksPerSuit = 13;

        public readonly Suit Suit;
        public readonly Rank Rank;

        public Card(Rank rank, Suit suit)
        {
            if (rank < Rank.Ace || rank > Rank.King)
                throw new ArgumentOutOfRangeException(nameof(rank));
            if (suit > Suit.Clubs)
                throw new ArgumentOutOfRangeException(nameof(suit));
            Rank = rank;
            Suit = suit;
        }

        /// <summary>ラインの合計に加算される点数（A=1, J=11, Q=12, K=13）。</summary>
        public int Value => (int)Rank;

        public bool IsRed => Suit == Suit.Hearts || Suit == Suit.Diamonds;

        /// <summary>0〜51の通し番号。</summary>
        public int Id => (int)Suit * RanksPerSuit + (int)Rank - 1;

        public string RankLabel
        {
            get
            {
                switch (Rank)
                {
                    case Rank.Ace: return "A";
                    case Rank.Jack: return "J";
                    case Rank.Queen: return "Q";
                    case Rank.King: return "K";
                    default: return Value.ToString();
                }
            }
        }

        public static Card FromId(int id)
        {
            if (id < 0 || id >= DistinctCount)
                throw new ArgumentOutOfRangeException(nameof(id));
            return new Card((Rank)(id % RanksPerSuit + 1), (Suit)(id / RanksPerSuit));
        }

        public bool Equals(Card other) => Suit == other.Suit && Rank == other.Rank;
        public override bool Equals(object obj) => obj is Card other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(Card a, Card b) => a.Equals(b);
        public static bool operator !=(Card a, Card b) => !a.Equals(b);

        public override string ToString() => RankLabel + "SHDC"[(int)Suit];
    }
}
