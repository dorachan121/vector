using System.Collections.Generic;

namespace Vector.Core.Derby
{
    /// <summary>1レースの最終結果。</summary>
    public sealed class RaceResult : IRoundOutcome
    {
        public RaceResult(RaceCard card, int first, int second, IReadOnlyList<int> positions, int cardsDrawn, IReadOnlyList<SpotResult> bets)
        {
            Card = card;
            First = first;
            Second = second;
            Positions = positions;
            CardsDrawn = cardsDrawn;
            Bets = bets;
        }

        public RaceCard Card { get; }
        public int First { get; }
        public int Second { get; }

        /// <summary>決着時の各走者の位置。</summary>
        public IReadOnlyList<int> Positions { get; }

        public int CardsDrawn { get; }
        public IReadOnlyList<SpotResult> Bets { get; }

        public int WinningExacta => DerbyTickets.Exacta(First, Second);

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;
    }
}
