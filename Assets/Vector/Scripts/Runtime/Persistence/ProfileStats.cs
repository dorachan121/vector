using System;
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

        public long Net => totalReturned - totalWagered;

        public void Record(RoundResult result)
        {
            roundsPlayed++;
            if (result.Net > 0)
                roundsWon++;
            else if (result.Net < 0)
                roundsLost++;
            else
                roundsPushed++;
            totalWagered = PlayerProfile.Clamp(totalWagered + result.TotalStake);
            totalReturned = PlayerProfile.Clamp(totalReturned + result.TotalReturn);
            biggestWin = Math.Max(biggestWin, result.Net);
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
        }
    }
}
