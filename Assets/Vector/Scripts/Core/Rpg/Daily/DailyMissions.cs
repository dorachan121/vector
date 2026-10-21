using System;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// 毎日入れ替わる3つのミッション（遊ぶ量・勝ち／賭けた額・ゲームの種類）と、3つすべての達成ボーナス。
    /// どのミッションになるかは日付だけで決まる（同じ日は何度開いても同じ）。
    /// </summary>
    public static class DailyMissions
    {
        public const int Count = 3;

        public static readonly RewardGrant AllClearBonus = new RewardGrant { Chips = 1_000, Comps = 50 };

        static readonly long[] roundTargets = { 10, 15, 20 };

        public static DailyMissionDef[] For(int day)
        {
            uint h = Hash(day);
            var games = (GameId[])Enum.GetValues(typeof(GameId));
            var volume = new DailyMissionDef { Kind = DailyMissionKind.PlayRounds, Target = roundTargets[h % 3], Xp = 50, Comps = 20 };
            var skill = (h >> 4) % 2 == 0
                ? new DailyMissionDef { Kind = DailyMissionKind.WinRounds, Target = 3 + (h >> 8) % 3, Xp = 80, Comps = 30 }
                : new DailyMissionDef { Kind = DailyMissionKind.Wager, Target = 1_000 * (1 + (h >> 8) % 3), Xp = 80, Comps = 30 };
            var variety = (h >> 12) % 3 == 0
                ? new DailyMissionDef { Kind = DailyMissionKind.DistinctGames, Target = 3, Xp = 60, Chips = 500 }
                : new DailyMissionDef { Kind = DailyMissionKind.PlayGame, Game = games[(h >> 16) % (uint)games.Length], Target = 5, Xp = 60, Chips = 500 };
            return new[] { volume, skill, variety };
        }

        static uint Hash(int day)
        {
            unchecked
            {
                uint h = (uint)day * 2654435761u;
                h ^= h >> 15;
                h *= 2246822519u;
                h ^= h >> 13;
                return h;
            }
        }
    }
}
