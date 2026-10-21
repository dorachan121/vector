using Vector.Core.Rpg;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>デイリーミッションと報酬の文言。</summary>
    public static class MissionText
    {
        public static string Describe(DailyMissionDef mission)
        {
            switch (mission.Kind)
            {
                case DailyMissionKind.PlayRounds: return $"テーブルで {mission.Target} ラウンド遊ぶ";
                case DailyMissionKind.WinRounds: return $"{mission.Target} ラウンド勝つ";
                case DailyMissionKind.Wager: return $"合計 {Labels.Money(mission.Target)} 賭ける";
                case DailyMissionKind.PlayGame: return $"{GameCatalog.Of(mission.Game).Title} で {mission.Target} ラウンド遊ぶ";
                default: return $"{mission.Target} 種類のゲームを遊ぶ";
            }
        }

        /// <summary>進み具合の文言（賭けた額は金額で）。</summary>
        public static string Progress(DailyMissionDef mission, long progress)
            => mission.Kind == DailyMissionKind.Wager ? $"{Labels.Money(progress)} / {Labels.Money(mission.Target)}" : $"{progress} / {mission.Target}";

        public static string Reward(long xp, long comps, long chips)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (chips > 0)
                parts.Add(Labels.Money(chips));
            if (comps > 0)
                parts.Add($"COMP {comps}");
            if (xp > 0)
                parts.Add($"XP {xp}");
            return string.Join("  ·  ", parts);
        }
    }
}
