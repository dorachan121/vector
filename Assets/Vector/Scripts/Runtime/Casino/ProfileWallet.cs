using System;
using Vector.Core;
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

        /// <summary>精算するたびに呼ばれる（RPG の経験値などの集計用）。</summary>
        public static event Action<GameId, IRoundOutcome> Settled;

        public long Balance => profiles.Profile.balance;
        public bool CanReload => true;
        public long ReloadTarget => PlayerProfile.StartingBalance;

        public void RecordDeal(long balanceAfterDebit, long stake) => profiles.RecordDeal(balanceAfterDebit, stake);

        public void RecordSettlement(long balance, IRoundOutcome outcome)
        {
            profiles.RecordSettlement(balance, outcome, game);
            Settled?.Invoke(game, outcome);
        }

        public void RecordVoid(long balance) => profiles.RecordVoid(balance);

        public long Reload(long tableMinimum) => profiles.Reload(tableMinimum);

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Settled = null;
    }
}
