using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// 1ラウンドの進行を管理する純粋なロジック。演出側はこのクラスのメソッドを順に呼び、状態を読んで描画する。
    /// <para>流れ: Deal → ChooseFamily → (PlacePlayerCard → PlayDealerCard) ×4 → Settle → BeginBetting</para>
    /// </summary>
    public sealed class RoundEngine
    {
        public const int HandSize = 4;

        readonly TableRules rules;
        readonly Shoe shoe;
        readonly IDealerPolicy dealer;
        readonly List<Card> hand = new List<Card>(HandSize);
        LineFamily playerFamily;

        public RoundEngine(TableRules rules, Shoe shoe, IDealerPolicy dealer)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.shoe = shoe ?? throw new ArgumentNullException(nameof(shoe));
            this.dealer = dealer ?? throw new ArgumentNullException(nameof(dealer));
        }

        public Shoe Shoe => shoe;
        public Board Board { get; } = new Board();
        public RoundPhase Phase { get; private set; } = RoundPhase.Betting;
        public BetBoard Bets { get; private set; } = new BetBoard();
        public IReadOnlyList<Card> Hand => hand;
        public PokerHand BonusHand { get; private set; }
        public int RoundNumber { get; private set; }

        public LineFamily PlayerFamily
        {
            get
            {
                if (Phase < RoundPhase.PlayerTurn)
                    throw new InvalidOperationException("まだラインの種類が選ばれていません。");
                return playerFamily;
            }
        }

        /// <summary>
        /// 次のラウンドのベット受付に戻す。カットカードが出ていればシャッフルし、その場合は true を返す。
        /// </summary>
        public bool BeginBetting()
        {
            if (Phase != RoundPhase.Complete && Phase != RoundPhase.Betting)
                throw new InvalidOperationException("ラウンドが終わっていません。");
            Phase = RoundPhase.Betting;
            if (!shoe.CutCardReached)
                return false;
            shoe.Shuffle();
            return true;
        }

        /// <summary>ベットを確定し、中央のベクターカードと手札4枚を配る。</summary>
        public void Deal(GameMode mode, BetBoard bets)
        {
            Require(RoundPhase.Betting);
            WagerCheck.ThrowIfInvalid(rules.LimitsFor(mode), bets);

            Bets = bets.Clone();
            Board.Clear();
            hand.Clear();
            RoundNumber++;

            var center = shoe.Draw();
            Board.Place(Lines.CenterCell, center, Seat.House);
            for (int i = 0; i < HandSize; i++)
                hand.Add(shoe.Draw());

            var five = new List<Card>(PokerEvaluator.HandSize) { center };
            five.AddRange(hand);
            BonusHand = PokerEvaluator.Evaluate(five);

            Phase = RoundPhase.ChoosingFamily;
        }

        public void ChooseFamily(LineFamily family)
        {
            Require(RoundPhase.ChoosingFamily);
            if (family > LineFamily.Slash)
                throw new ArgumentOutOfRangeException(nameof(family));
            playerFamily = family;
            Phase = RoundPhase.PlayerTurn;
        }

        public void PlacePlayerCard(int handIndex, int cell)
        {
            Require(RoundPhase.PlayerTurn);
            if (handIndex < 0 || handIndex >= hand.Count)
                throw new ArgumentOutOfRangeException(nameof(handIndex));
            Board.Place(cell, hand[handIndex], Seat.Player);
            hand.RemoveAt(handIndex);
            Phase = Board.IsFull ? RoundPhase.Showdown : RoundPhase.DealerTurn;
        }

        /// <summary>ディーラーがシューから1枚引き、ハウスウェイに従って置く。</summary>
        public DealerMove PlayDealerCard()
        {
            Require(RoundPhase.DealerTurn);
            var card = shoe.Draw();
            int cell = dealer.ChooseCell(Board.ToValues(), card.Value, playerFamily, HandValues());
            Board.Place(cell, card, Seat.Dealer);
            Phase = Board.IsFull ? RoundPhase.Showdown : RoundPhase.PlayerTurn;
            return new DealerMove(card, cell);
        }

        public List<int> HandValues()
        {
            var values = new List<int>(hand.Count);
            foreach (var c in hand)
                values.Add(c.Value);
            return values;
        }

        /// <summary>勝敗を判定し、全ベットを精算する。</summary>
        public RoundResult Settle()
        {
            Require(RoundPhase.Showdown);
            var values = Board.ToValues();
            var dealerFamily = Scoring.DealerFamily(values, playerFamily);
            int playerScore = Scoring.FamilyScore(values, playerFamily);
            int dealerScore = Scoring.FamilyScore(values, dealerFamily);
            var winner = Scoring.WinnerOf(playerScore, dealerScore);

            var bets = new List<SpotResult>();
            foreach (var bet in Bets.NonZero())
                bets.Add(Settlement.Settle(rules, (BetSpot)bet.Key, bet.Value, winner, BonusHand));

            Phase = RoundPhase.Complete;
            return new RoundResult(playerFamily, dealerFamily, playerScore, dealerScore,
                Scoring.BestLines(values, playerFamily), Scoring.BestLines(values, dealerFamily), BonusHand, bets);
        }

        /// <summary>
        /// 進行中のラウンドを無効にする（障害時の救済）。返金すべき賭け金を返す。
        /// 進行中でなければ何もせず 0 を返すので、二重に返金されることはない。
        /// </summary>
        public long Abort()
        {
            if (Phase == RoundPhase.Betting || Phase == RoundPhase.Complete)
                return 0;
            long stake = Bets.Total;
            Board.Clear();
            hand.Clear();
            Phase = RoundPhase.Complete;
            return stake;
        }

        void Require(RoundPhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"現在のフェーズは {Phase} です（{phase} が必要）。");
        }
    }
}
