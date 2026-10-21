using System;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Rpg;

namespace Vector.Game.Casino
{
    /// <summary>
    /// フロアの主の挑戦用のチップを手持ちにする（所持金とは別で、保存もしない）。
    /// 精算のたびに挑戦のラウンド数と目標を判定する。
    /// </summary>
    public sealed class ChallengeWallet : ITableWallet
    {
        readonly long tableMinimum;

        public ChallengeWallet(ChallengeRun run, long tableMinimum)
        {
            Run = run ?? throw new ArgumentNullException(nameof(run));
            this.tableMinimum = tableMinimum;
        }

        public ChallengeRun Run { get; }

        public long Balance => Run.Bankroll.Balance;
        public bool CanReload => false;
        public long ReloadTarget => 0;

        public void RecordDeal(long balanceAfterDebit, long stake) => Sync(balanceAfterDebit);

        /// <summary>挑戦用のチップの勝負は、経験値やクエストには数えない。</summary>
        public void RecordSettlement(long balance, IRoundOutcome outcome, IReadOnlyCollection<string> tags)
        {
            Sync(balance);
            if (!Run.IsOver)
                Run.RecordRound(tableMinimum);
        }

        public void RecordVoid(long balance) => Sync(balance);

        public long Reload(long minimum) => 0;

        /// <summary>挑戦用のチップをテーブルの手持ちに合わせる。</summary>
        void Sync(long balance)
        {
            long current = Run.Bankroll.Balance;
            if (balance > current)
                Run.Bankroll.Credit(balance - current);
            else if (balance < current)
                Run.Bankroll.TryDebit(current - balance);
        }
    }
}
