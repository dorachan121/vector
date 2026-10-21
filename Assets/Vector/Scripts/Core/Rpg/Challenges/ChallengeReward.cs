namespace Vector.Core.Rpg
{
    /// <summary>挑戦の結果で受け取ったもの。</summary>
    public sealed class ChallengeReward
    {
        /// <summary>決着した挑戦。</summary>
        public ChallengeSpec Spec;

        public bool Won;

        /// <summary>この主にはじめて勝った（賞金・記念品・次のフロアが手に入る）。</summary>
        public bool FirstVictory;

        public long Prize;
        public long Xp;
        public int LevelsGained;
        public string Trophy;
    }
}
