using System.Collections.Generic;

namespace Vector.Core.Colosseum
{
    /// <summary>COLOSSEUM の1ラウンドの最終結果。</summary>
    public sealed class BattleResult : IRoundOutcome
    {
        public BattleResult(RoundWinner winner, int turns, int playerHp, int beastHp, IReadOnlyList<SpotResult> bets)
        {
            Winner = winner;
            Turns = turns;
            PlayerHp = playerHp;
            BeastHp = beastHp;
            Bets = bets;
        }

        /// <summary>Player = 獣を倒した、Dealer = 倒された、Tie = 手数の上限で引き分け。</summary>
        public RoundWinner Winner { get; }

        public int Turns { get; }

        /// <summary>決着時の残り体力（0 未満は 0）。</summary>
        public int PlayerHp { get; }
        public int BeastHp { get; }

        public IReadOnlyList<SpotResult> Bets { get; }

        public long TotalStake => SpotResult.TotalStake(Bets);
        public long TotalReturn => SpotResult.TotalReturn(Bets);
        public long Net => TotalReturn - TotalStake;
    }
}
