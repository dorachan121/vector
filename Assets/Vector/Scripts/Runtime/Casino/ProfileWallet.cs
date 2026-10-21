using System;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Game.Persistence;

namespace Vector.Game.Casino
{
    /// <summary>プレイヤーデータの所持金を使う（ふだんのテーブル）。</summary>
    public sealed class ProfileWallet : ITableWallet
    {
        readonly ProfileService profiles;
        readonly GameId game;

        public ProfileWallet(ProfileService profiles, GameId game)
        {
            this.profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            this.game = game;
        }

        public long Balance => profiles.Profile.balance;
        public bool CanReload => true;
        public long ReloadTarget => PlayerProfile.StartingBalance;

        public void RecordDeal(long balanceAfterDebit, long stake) => profiles.RecordDeal(balanceAfterDebit, stake);

        /// <summary>直前の精算で会員の実績（経験値・コンプ・クエスト）に起きたこと。</summary>
        public RoundReport LastReport { get; private set; }

        public void RecordSettlement(long balance, IRoundOutcome outcome, IReadOnlyCollection<string> tags)
        {
            profiles.RecordSettlement(balance, outcome, game);
            LastReport = GameContext.Story.OnRound(game, outcome, tags);
        }

        public void RecordVoid(long balance) => profiles.RecordVoid(balance);

        public long Reload(long tableMinimum) => profiles.Reload(tableMinimum);
    }
}
