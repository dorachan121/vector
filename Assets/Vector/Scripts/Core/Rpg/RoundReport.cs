using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>1ラウンドの集計で起きたこと（演出用）。</summary>
    public sealed class RoundReport
    {
        public long Xp;
        public int LevelsGained;
        public long Comps;
        public VipRank? NewRank;
        public readonly List<QuestDef> CompletedQuests = new List<QuestDef>();

        /// <summary>このラウンドで達成したデイリーミッション。</summary>
        public readonly List<DailyMissionDef> CompletedDailies = new List<DailyMissionDef>();
    }
}
