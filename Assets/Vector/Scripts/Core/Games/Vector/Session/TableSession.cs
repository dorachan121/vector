using System;

namespace Vector.Core
{
    /// <summary>
    /// 1つのテーブルに座っている間の状態。ベットの置き方（残高・上限のチェック、取り消し、リベット、倍掛け）と、
    /// 配札時の引き落とし・精算時の払い戻しを RoundEngine と Bankroll の間で取り持つ。
    /// </summary>
    public sealed class TableSession
    {
        readonly TableRules rules;
        readonly Bankroll bankroll;
        BetLayout lastBets;

        public TableSession(TableRules rules, Bankroll bankroll, RoundEngine round, GameMode mode)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.bankroll = bankroll ?? throw new ArgumentNullException(nameof(bankroll));
            Round = round ?? throw new ArgumentNullException(nameof(round));
            Mode = mode;
        }

        public GameMode Mode { get; }
        public RoundEngine Round { get; }
        public TableRules Rules => rules;

        /// <summary>テーブルに置いたがまだ配られていない（引き落とし前の）ベット。</summary>
        public BetLayout Bets { get; } = new BetLayout();

        public Scoreboard Scoreboard { get; } = new Scoreboard();

        public long Balance => bankroll.Balance;

        /// <summary>テーブルに置いたチップを除いた手持ち。</summary>
        public long Available => bankroll.Balance - Bets.Total;

        bool IsBetting => Round.Phase == RoundPhase.Betting;

        public bool CanRebet => IsBetting && Bets.IsEmpty && lastBets != null && bankroll.CanAfford(lastBets.Total);

        public bool CanDouble
        {
            get
            {
                if (!IsBetting || Bets.IsEmpty || Bets.Total > Available)
                    return false;
                foreach (var bet in Bets.NonZero())
                {
                    if (bet.Value * 2 > rules.MaxFor(bet.Key))
                        return false;
                }
                return true;
            }
        }

        /// <summary>手持ちがテーブルミニマムに届かず、もう賭けられない。</summary>
        public bool IsBroke => Available < rules.MinBet && Bets.IsEmpty;

        public ChipError TryAddChip(BetSpot spot, long amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (!IsBetting)
                return ChipError.NotBetting;
            if (!rules.IsSpotAvailable(Mode, spot))
                return ChipError.SpotUnavailable;
            if (amount > Available)
                return ChipError.InsufficientFunds;
            if (Bets[spot] + amount > rules.MaxFor(spot))
                return ChipError.AboveMaximum;
            Bets.Add(spot, amount);
            return ChipError.None;
        }

        public bool TryUndo(out BetSpot spot, out long amount)
        {
            if (IsBetting)
                return Bets.TryUndo(out spot, out amount);
            spot = default;
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

        public BetCheck CheckBets() => rules.Validate(Mode, Bets);

        /// <summary>ベットを引き落として配札する。ベットが不正なら例外。</summary>
        public void Deal()
        {
            var check = CheckBets();
            if (!check.IsValid)
                throw new InvalidOperationException($"ベットが不正です: {check.Error}");
            if (!bankroll.TryDebit(Bets.Total))
                throw new InvalidOperationException("手持ちが足りません。");
            Round.Deal(Mode, Bets);
            lastBets = Bets.Clone();
            Bets.Clear();
        }

        /// <summary>勝敗を判定して払い戻す。</summary>
        public RoundResult Settle()
        {
            var result = Round.Settle();
            bankroll.Credit(result.TotalReturn);
            Scoreboard.Add(result.Winner);
            return result;
        }

        /// <summary>進行中のラウンドを無効にして賭け金を返金する。返金額を返す。</summary>
        public long VoidRound()
        {
            long refund = Round.Abort();
            bankroll.Credit(refund);
            return refund;
        }

        /// <summary>手持ちにチップを足す（キャッシャーでの補充）。</summary>
        public void Deposit(long amount) => bankroll.Credit(amount);

        /// <summary>次のラウンドのベット受付を始める。シャッフルしたら出目表をリセットして true。</summary>
        public bool BeginBetting()
        {
            bool shuffled = Round.BeginBetting();
            if (shuffled)
                Scoreboard.Clear();
            return shuffled;
        }
    }
}
