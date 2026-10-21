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

        public StoryProgress(ProfileService profiles)
        {
            this.profiles = profiles;
            Session = new RpgSession(profiles.Profile.rpg, new ProfileAccount(profiles));
        }

        public RpgSession Session { get; }
        public RpgState State => Session.State;

        /// <summary>テーブルの精算を集計して保存する。</summary>
        public RoundReport OnRound(GameId game, IRoundOutcome outcome, IReadOnlyCollection<string> tags)
        {
            var report = Session.OnRound(new RoundEvent(game, outcome.TotalStake, outcome.TotalReturn, tags));
            profiles.Save();
            return report;
        }

        public void Save() => profiles.Save();
    }
}
