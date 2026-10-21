using System;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Game.Persistence;

namespace Vector.Game.Story
{
    /// <summary>
    /// ストーリーモードの進み具合（プレイヤーデータの rpg）を、規則（RpgSession）で更新して保存する窓口。
    /// テーブルの精算はどのモードで遊んでも会員の実績として数える（経験値・コンプ・クエスト）。
    /// </summary>
    public sealed class StoryProgress
    {
        readonly ProfileService profiles;

        long refundedEntryFee;

        public StoryProgress(ProfileService profiles)
        {
            this.profiles = profiles;
            Session = new RpgSession(profiles.Profile.rpg, new ProfileAccount(profiles), Today);
            // 作られるのは起動ごとに1回で、この時点で進行中の挑戦は無い。記録が残っていれば前回の起動で中断したもの
            CancelChallenge();
        }

        /// <summary>日付の通し番号の起点（端末の現地時刻の日付で数える）。</summary>
        static readonly DateTime Epoch = new DateTime(2020, 1, 1);

        public RpgSession Session { get; }
        public RpgState State => Session.State;

        /// <summary>挑戦の結果を受け取って保存する（テーブルを離れる前に確定させる）。</summary>
        public ChallengeReward FinishChallenge(ChallengeRun run)
        {
            var reward = Session.FinishChallenge(run);
            profiles.Save();
            return reward;
        }

        /// <summary>決着しなかった挑戦（障害でテーブルを離れた・前回の起動で中断した）の参加費を返す。</summary>
        public void CancelChallenge()
        {
            long fee = Session.RefundInterruptedChallenge();
            if (fee <= 0)
                return;
            refundedEntryFee += fee;
            profiles.Save();
        }

        /// <summary>中断した挑戦で返した参加費（お知らせ用。読むと 0 に戻る）。</summary>
        public long TakeRefundedEntryFee()
        {
            long fee = refundedEntryFee;
            refundedEntryFee = 0;
            return fee;
        }

        /// <summary>テーブルの精算を集計して保存する。</summary>
        public RoundReport OnRound(GameId game, IRoundOutcome outcome, IReadOnlyCollection<string> tags)
        {
            var report = Session.OnRound(new RoundEvent(game, outcome.TotalStake, outcome.TotalReturn, tags));
            profiles.Save();
            return report;
        }

        public void Save() => profiles.Save();

        /// <summary>今日の日付の通し番号（ログインボーナスとデイリーミッションの日付）。</summary>
        static int Today() => (int)(DateTime.Now.Date - Epoch).TotalDays;
    }
}
