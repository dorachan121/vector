using System;
using System.Collections.Generic;

namespace Vector.Core.Colosseum
{
    /// <summary>
    /// COLOSSEUM の1ラウンドの進行を管理する純粋なロジック。
    /// <para>流れ: Deal → （獣の予告 → プレイヤーが斬る / 受ける）を決着まで → Settle</para>
    /// カードは毎回 52 枚から1枚を引く（無限デッキ）。賭け金の引き落とし・払い戻しは呼び出し側が行う。
    /// </summary>
    public sealed class BattleEngine
    {
        readonly BattleRules rules;
        readonly IRandomSource random;
        BattleSetup setup;

        public BattleEngine(BattleRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            setup = rules.Setup;
        }

        public BattleRules Rules => rules;
        public BattleSetup Setup => setup;
        public BattlePhase Phase { get; private set; } = BattlePhase.Betting;
        public BetBoard Bets { get; private set; } = new BetBoard();
        public int RoundNumber { get; private set; }
        public int Turn { get; private set; }
        public int PlayerHp { get; private set; }
        public int BeastHp { get; private set; }

        /// <summary>獣の次の一撃の予告カード。</summary>
        public Card Intent { get; private set; }

        /// <summary>予告どおりに受けるダメージ（猛り狂いの加算を含む）。</summary>
        public int IntentDamage => setup.IntentDamage(Intent.Value, BeastHp);

        public bool IsEnraged => setup.IsEnraged(BeastHp);

        public void BeginBetting()
        {
            if (Phase != BattlePhase.Complete && Phase != BattlePhase.Betting)
                throw new InvalidOperationException("ラウンドが終わっていません。");
            Phase = BattlePhase.Betting;
        }

        /// <summary>ベットを確定して対戦を始め、獣の最初の予告を出す。setup を渡すと別の対戦相手（RPG の挑戦など）にする。</summary>
        public void Deal(BetBoard bets, BattleSetup? opponent = null)
        {
            Require(BattlePhase.Betting);
            WagerCheck.ThrowIfInvalid(rules, bets);
            Bets = bets.Clone();
            setup = opponent ?? rules.Setup;
            RoundNumber++;
            Turn = 0;
            PlayerHp = setup.PlayerHp;
            BeastHp = setup.BeastHp;
            NextIntent();
        }

        /// <summary>手を選び、カードを1枚引いて1手を解決する。決着していなければ次の予告を出す。</summary>
        public BattleExchange Act(BattleAction action)
        {
            Require(BattlePhase.PlayerTurn);
            var intent = Intent;
            int incoming = IntentDamage;
            var card = Draw();
            int toBeast = 0, toPlayer = 0;
            bool critical = false, clash = false;
            if (action == BattleAction.Strike)
            {
                toBeast = BattleRules.StrikeDamage(card.Value);
                critical = card.Value == BattleRules.CriticalValue;
                if (BeastHp - toBeast > 0)
                    toPlayer = incoming;
            }
            else if (card.Value > incoming)
            {
                toBeast = card.Value - incoming;
            }
            else if (card.Value < incoming)
            {
                toPlayer = incoming - card.Value;
            }
            else
            {
                clash = true;
            }
            BeastHp -= toBeast;
            PlayerHp -= toPlayer;
            Turn++;

            var exchange = new BattleExchange(action, intent, card, incoming, toBeast, toPlayer, critical, clash, PlayerHp, BeastHp);
            if (BeastHp <= 0 || PlayerHp <= 0 || Turn >= rules.MaxTurns)
                Phase = BattlePhase.Showdown;
            else
                NextIntent();
            return exchange;
        }

        public BattleResult Settle()
        {
            Require(BattlePhase.Showdown);
            var winner = BeastHp <= 0 ? RoundWinner.Player : PlayerHp <= 0 ? RoundWinner.Dealer : RoundWinner.Tie;
            var bets = new List<SpotResult>();
            foreach (var bet in Bets.NonZero())
            {
                switch (winner)
                {
                    case RoundWinner.Player: bets.Add(SpotResult.Win(bet.Key, bet.Value, rules.WinPays)); break;
                    case RoundWinner.Tie: bets.Add(SpotResult.Push(bet.Key, bet.Value)); break;
                    default: bets.Add(SpotResult.Lose(bet.Key, bet.Value)); break;
                }
            }
            Phase = BattlePhase.Complete;
            return new BattleResult(winner, Turn, Math.Max(0, PlayerHp), Math.Max(0, BeastHp), bets);
        }

        /// <summary>進行中のラウンドを無効にする（障害時の救済）。返金は呼び出し側が行う。</summary>
        public void Abort()
        {
            if (Phase != BattlePhase.Betting && Phase != BattlePhase.Complete)
                Phase = BattlePhase.Complete;
        }

        void NextIntent()
        {
            Intent = Draw();
            Phase = BattlePhase.PlayerTurn;
        }

        Card Draw() => Card.FromId(random.Next(Card.DistinctCount));

        void Require(BattlePhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"現在のフェーズは {Phase} です（{phase} が必要）。");
        }
    }
}
