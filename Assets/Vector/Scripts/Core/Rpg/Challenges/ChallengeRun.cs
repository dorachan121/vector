using System;

namespace Vector.Core.Rpg
{
    /// <summary>進行中の挑戦（挑戦用のチップとラウンド数）。テーブル側はこれを手持ちとして使う。</summary>
    public sealed class ChallengeRun
    {
        public ChallengeRun(ChallengeSpec spec)
        {
            Spec = spec ?? throw new ArgumentNullException(nameof(spec));
            Bankroll = new Bankroll(spec.Stack);
        }

        public ChallengeSpec Spec { get; }

        /// <summary>挑戦用のチップ（テーブルの手持ち）。</summary>
        public Bankroll Bankroll { get; }

        public int RoundsPlayed { get; private set; }
        public int RoundsLeft => Spec.Rounds - RoundsPlayed;
        public ChallengeState State { get; private set; }
        public bool IsOver => State != ChallengeState.InProgress;

        /// <summary>目標まであと何チップか（0 なら到達）。</summary>
        public long Remaining => Math.Max(0, Spec.Target - Bankroll.Balance);

        /// <summary>1ラウンドの精算が済んだ（Bankroll はテーブル側が更新済み）。tableMinimum 未満になったら負け。</summary>
        public void RecordRound(long tableMinimum)
        {
            if (IsOver)
                throw new InvalidOperationException("挑戦は終わっています。");
            RoundsPlayed++;
            if (Bankroll.Balance >= Spec.Target)
                State = ChallengeState.Won;
            else if (RoundsPlayed >= Spec.Rounds || Bankroll.Balance < tableMinimum)
                State = ChallengeState.Lost;
        }

        /// <summary>手持ちが最低額を下回ってこれ以上賭けられない（ラウンドの前に確かめる）。</summary>
        public void CheckBroke(long tableMinimum)
        {
            if (!IsOver && Bankroll.Balance < tableMinimum)
                State = ChallengeState.Lost;
        }

        public void Abandon()
        {
            if (!IsOver)
                State = ChallengeState.Abandoned;
        }
    }
}
