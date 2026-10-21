using System;

namespace Vector.Core
{
    /// <summary>
    /// 全ゲーム共通の、テーブルに座っている間の賭け金の管理。チップの置き方（残高・上限のチェック、取り消し、
    /// リベット、倍掛け）と、勝負の開始時の引き落とし・精算時の払い戻し・障害時の返金を受け持つ。
    /// ゲームの進行（何をもって勝ちとするか）は各ゲームのエンジンが持つ。
    /// </summary>
    public sealed class WagerSession
    {
        readonly IBetLimits limits;
        readonly Bankroll bankroll;
        BetBoard lastBets;

        public WagerSession(IBetLimits limits, Bankroll bankroll)
        {
            this.limits = limits ?? throw new ArgumentNullException(nameof(limits));
            this.bankroll = bankroll ?? throw new ArgumentNullException(nameof(bankroll));
        }

        public IBetLimits Limits => limits;

        /// <summary>テーブルに置いたがまだ勝負に入っていない（引き落とし前の）ベット。</summary>
        public BetBoard Bets { get; } = new BetBoard();

        /// <summary>勝負中のベット（引き落とし済み）。勝負中でなければ空。</summary>
        public BetBoard InPlay { get; private set; } = new BetBoard();

        /// <summary>ベット受付中（勝負中ではない）。</summary>
        public bool IsBetting { get; private set; } = true;

        public long Balance => bankroll.Balance;

        /// <summary>テーブルに置いたチップを除いた手持ち。</summary>
        public long Available => bankroll.Balance - Bets.Total;

        public bool CanRebet => IsBetting && Bets.IsEmpty && lastBets != null && bankroll.CanAfford(lastBets.Total) && IsWithinLimits(lastBets, 1);

        public bool CanDouble => IsBetting && !Bets.IsEmpty && Bets.Total <= Available && IsWithinLimits(Bets, 2);

        /// <summary>手持ちが最低額に届かず、もう賭けられない。</summary>
        public bool IsBrokeFor(long minimum) => IsBetting && Bets.IsEmpty && Available < minimum;

        public ChipError TryAddChip(int spot, long amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (!IsBetting)
                return ChipError.NotBetting;
            if (!limits.IsSpotAvailable(spot))
                return ChipError.SpotUnavailable;
            if (amount > Available)
                return ChipError.InsufficientFunds;
            if (Bets[spot] + amount > limits.MaxFor(spot))
                return ChipError.AboveMaximum;
            Bets.Add(spot, amount);
            return ChipError.None;
        }

        public bool TryUndo(out int spot, out long amount)
        {
            if (IsBetting)
                return Bets.TryUndo(out spot, out amount);
            spot = -1;
            amount = 0;
            return false;
        }

        public void ClearBets()
        {
            if (IsBetting)
                Bets.Clear();
        }

        /// <summary>前のラウンドと同じベットを置き直す。</summary>
        public bool TryRebet()
        {
            if (!CanRebet)
                return false;
            foreach (var bet in lastBets.NonZero())
                Bets.Add(bet.Key, bet.Value);
            return true;
        }

        /// <summary>置いているベットをすべて2倍にする。</summary>
        public bool TryDouble()
        {
            if (!CanDouble)
                return false;
            foreach (var bet in Bets.Clone().NonZero())
                Bets.Add(bet.Key, bet.Value);
            return true;
        }

        public WagerCheck CheckBets() => WagerCheck.Validate(limits, Bets);

        /// <summary>ベットを引き落として勝負に入る。ベットが不正なら例外。勝負中のベットを返す。</summary>
        public BetBoard Commit()
        {
            if (!IsBetting)
                throw new InvalidOperationException("勝負中です。");
            var check = CheckBets();
            if (!check.IsValid)
                throw new InvalidOperationException($"ベットが不正です: {check.Error} ({check.Spot})");
            if (!bankroll.TryDebit(Bets.Total))
                throw new InvalidOperationException("手持ちが足りません。");
            InPlay = Bets.Clone();
            lastBets = Bets.Clone();
            Bets.Clear();
            IsBetting = false;
            return InPlay;
        }

        /// <summary>精算した払い戻し（賭け金を含む）を手持ちに戻し、ベット受付に戻る。</summary>
        public void Settle(long totalReturn)
        {
            if (IsBetting)
                throw new InvalidOperationException("勝負中ではありません。");
            if (totalReturn < 0)
                throw new ArgumentOutOfRangeException(nameof(totalReturn));
            bankroll.Credit(totalReturn);
            InPlay = new BetBoard();
            IsBetting = true;
        }

        /// <summary>
        /// 進行中の勝負を無効にして賭け金を返金する（障害時の救済）。返金額を返す。
        /// 勝負中でなければ何もせず 0 を返すので、二重に返金されることはない。
        /// </summary>
        public long Void()
        {
            if (IsBetting)
                return 0;
            long refund = InPlay.Total;
            bankroll.Credit(refund);
            InPlay = new BetBoard();
            IsBetting = true;
            return refund;
        }

        /// <summary>手持ちにチップを足す（キャッシャーでの補充）。</summary>
        public void Deposit(long amount) => bankroll.Credit(amount);

        bool IsWithinLimits(BetBoard bets, long factor)
        {
            foreach (var bet in bets.NonZero())
            {
                if (!limits.IsSpotAvailable(bet.Key) || bet.Value * factor > limits.MaxFor(bet.Key))
                    return false;
            }
            return true;
        }
    }
}
