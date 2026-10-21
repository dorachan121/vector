using System;
using Vector.Core;

namespace Vector.Game.Persistence
{
    /// <summary>1つのゲームの通算成績。</summary>
    [Serializable]
    public sealed class GameStats
    {
        /// <summary>ゲームの種類（<see cref="GameId"/> の値）。</summary>
        public int game;

        public int rounds;
        public int wins;
        public long wagered;
        public long returned;
        public long biggestWin;

        public long Net => returned - wagered;

        public void Record(IRoundOutcome result)
        {
            long net = result.TotalReturn - result.TotalStake;
            rounds++;
            if (net > 0)
                wins++;
            wagered = PlayerProfile.Clamp(wagered + result.TotalStake);
            returned = PlayerProfile.Clamp(returned + result.TotalReturn);
            biggestWin = Math.Max(biggestWin, net);
        }

        public void Sanitize()
        {
            rounds = Math.Max(0, rounds);
            wins = Math.Min(Math.Max(0, wins), rounds);
            wagered = PlayerProfile.Clamp(wagered);
            returned = PlayerProfile.Clamp(returned);
            biggestWin = PlayerProfile.Clamp(biggestWin);
        }
    }
}
