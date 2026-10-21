using System;
using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>
    /// CROWN DICE の1ラウンドの進行を管理する純粋なロジック。
    /// <para>流れ: Deal（王冠の目と最初の5個）→ プレイヤーが最大2回振り直す / 止める → ディーラーがハウスウェイで最大3回振る → Settle</para>
    /// 賭け金の引き落とし・払い戻しは呼び出し側（<see cref="WagerSession"/>）が行う。
    /// </summary>
    public sealed class CrownEngine
    {
        readonly CrownRules rules;
        readonly IRandomSource random;
        readonly int[] player = new int[DiceRank.DiceCount];
        readonly int[] dealer = new int[DiceRank.DiceCount];
        DealerPlan plan;

        public CrownEngine(CrownRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public CrownRules Rules => rules;
        public CrownPhase Phase { get; private set; } = CrownPhase.Betting;
        public BetBoard Bets { get; private set; } = new BetBoard();
        public int RoundNumber { get; private set; }

        /// <summary>このラウンドの王冠の目（1〜6）。この目は 6 より強い。</summary>
        public int Crown { get; private set; }

        public IReadOnlyList<int> PlayerDice => player;
        public IReadOnlyList<int> DealerDice => dealer;

        /// <summary>プレイヤーに残っている振り直しの回数。</summary>
        public int PlayerRerollsLeft { get; private set; }

        /// <summary>ディーラーに残っている振る回数（最初の1回を含む）。</summary>
        public int DealerRollsLeft { get; private set; }

        /// <summary>最初の5個の役（オープニングベット用）。</summary>
        public OpeningHand Opening { get; private set; }

        public DiceRank PlayerRank => DiceRank.Of(player, Crown);
        public DiceRank DealerRank => DiceRank.Of(dealer, Crown);

        public void BeginBetting()
        {
            if (Phase != CrownPhase.Complete && Phase != CrownPhase.Betting)
                throw new InvalidOperationException("ラウンドが終わっていません。");
            Phase = CrownPhase.Betting;
        }

        /// <summary>ベットを確定し、王冠のダイスとプレイヤーの5個を振る。</summary>
        public void Deal(BetBoard bets)
        {
            Require(CrownPhase.Betting);
            WagerCheck.ThrowIfInvalid(rules, bets);

            Bets = bets.Clone();
            RoundNumber++;
            Crown = Roll();
            for (int i = 0; i < player.Length; i++)
            {
                player[i] = Roll();
                dealer[i] = 0;
            }
            Opening = OpeningHands.Of(PlayerRank, Crown);
            PlayerRerollsLeft = rules.RollsPerHand - 1;
            DealerRollsLeft = rules.RollsPerHand;
            plan = null;
            Phase = CrownPhase.PlayerTurn;
        }

        /// <summary>holdMask のダイスを残して振り直す。全部残すことはできない（止めるなら Stand）。</summary>
        public void Reroll(int holdMask)
        {
            Require(CrownPhase.PlayerTurn);
            int all = (1 << player.Length) - 1;
            if (holdMask < 0 || holdMask >= all)
                throw new ArgumentOutOfRangeException(nameof(holdMask));
            for (int i = 0; i < player.Length; i++)
            {
                if ((holdMask & (1 << i)) == 0)
                    player[i] = Roll();
            }
            if (--PlayerRerollsLeft == 0)
                BeginDealerTurn();
        }

        /// <summary>今の手で止める。</summary>
        public void Stand()
        {
            Require(CrownPhase.PlayerTurn);
            BeginDealerTurn();
        }

        /// <summary>ディーラーが1手進める（振る、または止める）。</summary>
        public DealerRoll PlayDealer()
        {
            Require(CrownPhase.DealerTurn);
            int held = 0;
            bool first = DealerRollsLeft == rules.RollsPerHand;
            if (!first)
            {
                held = plan.ChooseHold(dealer, DealerRollsLeft);
                if (held == (1 << dealer.Length) - 1)
                {
                    Phase = CrownPhase.Showdown;
                    return new DealerRoll(held, (int[])dealer.Clone(), true);
                }
            }
            for (int i = 0; i < dealer.Length; i++)
            {
                if ((held & (1 << i)) == 0)
                    dealer[i] = Roll();
            }
            if (--DealerRollsLeft == 0)
                Phase = CrownPhase.Showdown;
            return new DealerRoll(held, (int[])dealer.Clone(), false);
        }

        /// <summary>勝敗を判定し、全ベットを精算する。</summary>
        public CrownResult Settle()
        {
            Require(CrownPhase.Showdown);
            var playerRank = PlayerRank;
            var dealerRank = DealerRank;
            bool bonus = playerRank.IsCrownYachtFor(Crown);
            var bets = new List<SpotResult>();
            foreach (var bet in Bets.NonZero())
                bets.Add(bet.Key == CrownRules.Ante ? SettleAnte(bet.Value, playerRank, dealerRank, bonus) : SettleOpening(bet.Value));
            Phase = CrownPhase.Complete;
            return new CrownResult(Crown, (int[])player.Clone(), (int[])dealer.Clone(), playerRank, dealerRank, bonus, Opening, bets);
        }

        /// <summary>進行中のラウンドを無効にする（障害時の救済）。返金は呼び出し側が行う。</summary>
        public void Abort()
        {
            if (Phase == CrownPhase.Betting || Phase == CrownPhase.Complete)
                return;
            plan = null;
            Phase = CrownPhase.Complete;
        }

        SpotResult SettleAnte(long stake, DiceRank playerRank, DiceRank dealerRank, bool bonus)
        {
            long extra = bonus ? rules.CrownYachtBonus.WinningsFor(stake) : 0;
            if (playerRank > dealerRank)
                return new SpotResult(CrownRules.Ante, stake, BetOutcome.Win, checked(stake + rules.WinPays.WinningsFor(stake) + extra));
            if (playerRank == dealerRank)
                return new SpotResult(CrownRules.Ante, stake, extra > 0 ? BetOutcome.Win : BetOutcome.Push, checked(stake + extra));
            return SpotResult.Lose(CrownRules.Ante, stake);
        }

        SpotResult SettleOpening(long stake)
            => rules.OpeningPays.TryGetValue(Opening, out var odds)
                ? SpotResult.Win(CrownRules.Opening, stake, odds)
                : SpotResult.Lose(CrownRules.Opening, stake);

        void BeginDealerTurn()
        {
            plan = DealerPlan.For(rules, PlayerRank, Crown);
            Phase = CrownPhase.DealerTurn;
        }

        int Roll() => random.Next(DiceRank.Faces) + 1;

        void Require(CrownPhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"現在のフェーズは {Phase} です（{phase} が必要）。");
        }
    }
}
