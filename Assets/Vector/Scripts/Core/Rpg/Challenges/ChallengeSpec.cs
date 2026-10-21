using Vector.Core.Colosseum;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// フロアの主の挑戦（トーナメント形式）。参加費を払うと挑戦用のチップ（所持金とは別）を受け取り、
    /// 決められたラウンド数のうちに目標額まで増やせば勝ち。挑戦用のチップは持ち帰れない。
    /// 勝つ確率は打ち方と運しだいで、ゲームの配当や確率は通常のテーブルと同じ。
    /// </summary>
    public sealed class ChallengeSpec
    {
        /// <summary>挑む相手（人の id）。</summary>
        public string Master;

        public FloorId Floor;
        public GameId Game;

        /// <summary>挑むのに必要なレベル。</summary>
        public int RequiredLevel;

        /// <summary>参加費（所持金から払う）。</summary>
        public long EntryFee;

        /// <summary>挑戦用のチップ。</summary>
        public long Stack;

        /// <summary>この額に届いたら勝ち。</summary>
        public long Target;

        /// <summary>挑戦できるラウンド数。</summary>
        public int Rounds;

        /// <summary>はじめて勝ったときの賞金（所持金に入る）。</summary>
        public long Prize;

        public int XpReward;

        /// <summary>はじめて勝ったときにもらえる記念品。</summary>
        public string Trophy;

        /// <summary>COLOSSEUM の挑戦で戦う相手（null なら通常の獣）。</summary>
        public BattleSetup? Opponent;

        /// <summary>勝ったときに立てる印。</summary>
        public string DefeatedFlag => StoryFlags.Defeated(Master);
    }
}
