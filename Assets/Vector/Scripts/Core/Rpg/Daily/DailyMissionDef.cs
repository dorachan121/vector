namespace Vector.Core.Rpg
{
    /// <summary>デイリーミッション1つ（条件と報酬）。文言は表示側（Runtime）が作る。</summary>
    public sealed class DailyMissionDef
    {
        public DailyMissionKind Kind;
        public long Target;

        /// <summary>PlayGame のゲーム。</summary>
        public GameId Game;

        public long Xp;
        public long Comps;
        public long Chips;
    }
}
