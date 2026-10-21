using System;
using System.Collections.Generic;
using Vector.Core;

namespace Vector.Game.Persistence
{
    /// <summary>通算成績。</summary>
    [Serializable]
    public sealed class ProfileStats
    {
        public int roundsPlayed;
        public int roundsWon;
        public int roundsLost;
        public int roundsPushed;
        public long totalWagered;
        public long totalReturned;
        public long biggestWin;
        public int reloads;

        /// <summary>ゲームごとの成績。</summary>
        public List<GameStats> games = new List<GameStats>();

        public long Net => totalReturned - totalWagered;

        public GameStats For(GameId game)
        {
            foreach (var g in games)
            {
                if (g.game == (int)game)
                    return g;
            }
            var created = new GameStats { game = (int)game };
            games.Add(created);
            return created;
        }

        public void Record(IRoundOutcome result, GameId game)
        {
            For(game).Record(result);
            long net = result.TotalReturn - result.TotalStake;
            roundsPlayed++;
            if (net > 0)
                roundsWon++;
            else if (net < 0)
                roundsLost++;
            else
                roundsPushed++;
            totalWagered = PlayerProfile.Clamp(totalWagered + result.TotalStake);
            totalReturned = PlayerProfile.Clamp(totalReturned + result.TotalReturn);
            biggestWin = Math.Max(biggestWin, net);
        }

        public void Sanitize()
        {
            roundsPlayed = Math.Max(0, roundsPlayed);
            roundsWon = Math.Max(0, roundsWon);
            roundsLost = Math.Max(0, roundsLost);
            roundsPushed = Math.Max(0, roundsPushed);
            totalWagered = PlayerProfile.Clamp(totalWagered);
            totalReturned = PlayerProfile.Clamp(totalReturned);
            biggestWin = PlayerProfile.Clamp(biggestWin);
            reloads = Math.Max(0, reloads);
            games ??= new List<GameStats>();
            games.RemoveAll(g => g == null || !Enum.IsDefined(typeof(GameId), (byte)Math.Max(0, Math.Min(255, g.game))));
            foreach (var g in games)
                g.Sanitize();
        }
    }
}
