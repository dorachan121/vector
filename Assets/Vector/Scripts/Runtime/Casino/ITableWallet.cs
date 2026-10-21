using System.Collections.Generic;
using Vector.Core;

namespace Vector.Game.Casino
{
    /// <summary>
    /// テーブルの手持ちの出どころ。ふだんはプレイヤーデータの所持金（保存あり）、RPG の挑戦では挑戦用のチップ（保存なし）。
    /// </summary>
    public interface ITableWallet
    {
        /// <summary>着席したときの手持ち。</summary>
        long Balance { get; }

        /// <summary>手持ちが足りなくなったときに補充できるか。</summary>
        bool CanReload { get; }

        /// <summary>補充する額の目安（ダイアログの文言用）。</summary>
        long ReloadTarget { get; }

        /// <summary>勝負に入った（引き落とした）ことを記録する。</summary>
        void RecordDeal(long balanceAfterDebit, long stake);

        /// <summary>精算結果を記録する。tags はそのラウンドの出来事（クエストの条件用、RoundTags）。</summary>
        void RecordSettlement(long balance, IRoundOutcome outcome, IReadOnlyCollection<string> tags);

        /// <summary>障害で無効にした勝負の返金後の手持ちを記録する。</summary>
        void RecordVoid(long balance);

        /// <summary>手持ちがミニマム未満なら補充して、補充した額を返す。</summary>
        long Reload(long tableMinimum);
    }
}
