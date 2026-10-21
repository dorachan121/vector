using System;
using System.Collections.Generic;

namespace Vector.Core.Derby
{
    /// <summary>
    /// FOUR BEASTS DERBY の1レースの進行を管理する純粋なロジック。
    /// <para>流れ: BeginBetting（出走表とオッズ）→ Start（馬券を確定）→ Step を1着・2着が決まるまで → Settle</para>
    /// デッキは1組 52 枚をレースごとにシャッフルし、引いたカードのスートの走者が1歩進む。
    /// 賭け金の引き落とし・払い戻しは呼び出し側が行う。
    /// </summary>
    public sealed class DerbyEngine
    {
        readonly DerbyRules rules;
        readonly IRandomSource random;
        readonly Card[] deck = new Card[Card.DistinctCount];
        readonly int[] positions = new int[DerbyTickets.Runners];
        readonly int[] drawnBySuit = new int[DerbyTickets.Runners];
        int next;

        public DerbyEngine(DerbyRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            Phase = DerbyPhase.Complete;
            BeginBetting();
        }

        public DerbyRules Rules => rules;
        public DerbyPhase Phase { get; private set; }
        public RaceCard Race { get; private set; }
        public BetBoard Bets { get; private set; } = new BetBoard();
        public int RaceNumber { get; private set; }
        public IReadOnlyList<int> Positions => positions;
        public int First { get; private set; } = -1;
        public int Second { get; private set; } = -1;
        public int CardsDrawn => next;

        /// <summary>馬券の上下限（発売中の出走表）。WagerSession に渡す。</summary>
        public IBetLimits Limits => new CurrentCard(this);

        /// <summary>次のレースの出走表を抽選し、デッキをシャッフルして馬券の受付を始める。</summary>
        public void BeginBetting()
        {
            if (Phase != DerbyPhase.Complete)
                throw new InvalidOperationException("レースが終わっていません。");
            Race = RaceCard.Draw(rules, random);
            for (int i = 0; i < deck.Length; i++)
                deck[i] = Card.FromId(i);
            for (int i = deck.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
            next = 0;
            for (int r = 0; r < positions.Length; r++)
            {
                positions[r] = Race.Gates[r];
                drawnBySuit[r] = 0;
            }
            First = Second = -1;
            Bets = new BetBoard();
            Phase = DerbyPhase.Betting;
        }

        /// <summary>馬券を確定してレースを始める。</summary>
        public void Start(BetBoard bets)
        {
            Require(DerbyPhase.Betting);
            WagerCheck.ThrowIfInvalid(Race, bets);
            Bets = bets.Clone();
            RaceNumber++;
            Phase = DerbyPhase.Running;
        }

        /// <summary>カードを1枚引いてレースを進める。</summary>
        public RaceStep Step()
        {
            Require(DerbyPhase.Running);
            var card = deck[next++];
            int runner = (int)card.Suit;
            drawnBySuit[runner]++;
            bool finished = positions[runner] >= Race.Distance;
            int place = 0;
            if (!finished)
            {
                positions[runner]++;
                if (positions[runner] >= Race.Distance)
                {
                    if (First < 0)
                    {
                        First = runner;
                        place = 1;
                    }
                    else
                    {
                        Second = runner;
                        place = 2;
                        Phase = DerbyPhase.Finished;
                    }
                }
            }
            return new RaceStep(card, runner, !finished, positions[runner], place);
        }

        /// <summary>今の局面からの着順の確率（ライブオッズの表示用）。</summary>
        public RaceProbabilities LiveProbabilities()
        {
            if (Phase != DerbyPhase.Running && Phase != DerbyPhase.Betting)
                throw new InvalidOperationException("レース中ではありません。");
            var needed = new int[DerbyTickets.Runners];
            var remaining = new int[DerbyTickets.Runners];
            for (int r = 0; r < needed.Length; r++)
            {
                needed[r] = Math.Max(0, Race.Distance - positions[r]);
                remaining[r] = RaceProbabilities.SuitSize - drawnBySuit[r];
            }
            return RaceProbabilities.Compute(needed, remaining, First);
        }

        public RaceResult Settle()
        {
            Require(DerbyPhase.Finished);
            var bets = new List<SpotResult>();
            foreach (var bet in Bets.NonZero())
            {
                bool hit = DerbyTickets.IsWin(bet.Key)
                    ? bet.Key == First
                    : DerbyTickets.First(bet.Key) == First && DerbyTickets.Second(bet.Key) == Second;
                bets.Add(hit ? SpotResult.Win(bet.Key, bet.Value, Race.PaysFor(bet.Key)) : SpotResult.Lose(bet.Key, bet.Value));
            }
            Phase = DerbyPhase.Complete;
            return new RaceResult(Race, First, Second, (int[])positions.Clone(), next, bets);
        }

        /// <summary>進行中のレースを無効にする（障害時の救済）。返金は呼び出し側が行う。</summary>
        public void Abort()
        {
            if (Phase == DerbyPhase.Running || Phase == DerbyPhase.Finished)
                Phase = DerbyPhase.Complete;
        }

        void Require(DerbyPhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"現在のフェーズは {Phase} です（{phase} が必要）。");
        }

        /// <summary>常に「いま発売中の出走表」を参照する上下限（レースごとに出走表が替わるため）。</summary>
        sealed class CurrentCard : IBetLimits
        {
            readonly DerbyEngine engine;
            public CurrentCard(DerbyEngine engine) => this.engine = engine;
            public bool IsSpotAvailable(int spot) => engine.Race.IsSpotAvailable(spot);
            public long MinFor(int spot) => engine.Race.MinFor(spot);
            public long MaxFor(int spot) => engine.Race.MaxFor(spot);
            public bool IsPrimary(int spot) => engine.Race.IsPrimary(spot);
        }
    }
}
