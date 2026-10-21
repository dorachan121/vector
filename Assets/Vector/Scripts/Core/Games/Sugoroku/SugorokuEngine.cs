using System;
using System.Collections.Generic;

namespace Vector.Core.Sugoroku
{
    /// <summary>
    /// SUGOROKU の1回の道中の進行を管理する純粋なロジック。
    /// <para>流れ: BeginBetting → Start（ベットを確定して最初の2つの目を振る）→ Choose を上がるか回数を使い切るまで → Settle</para>
    /// 賭け金の引き落とし・払い戻しは呼び出し側が行う。
    /// </summary>
    public sealed class SugorokuEngine
    {
        readonly SugorokuRules rules;
        readonly IRandomSource random;
        readonly List<SugorokuStep> steps = new List<SugorokuStep>();

        public SugorokuEngine(SugorokuRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            Reset();
        }

        public SugorokuRules Rules => rules;
        public SugorokuPhase Phase { get; private set; } = SugorokuPhase.Betting;
        public BetBoard Bets { get; private set; } = new BetBoard();
        public int RoundNumber { get; private set; }

        public int Position { get; private set; }
        public int Tier { get; private set; }
        public int TurnsLeft { get; private set; }

        /// <summary>いま出ている2つの目（Choosing のときだけ意味がある）。</summary>
        public (int first, int second) Dice { get; private set; }

        public IReadOnlyList<SugorokuStep> Steps => steps;

        public void BeginBetting()
        {
            if (Phase != SugorokuPhase.Complete && Phase != SugorokuPhase.Betting)
                throw new InvalidOperationException("道中が精算されていません。");
            Phase = SugorokuPhase.Betting;
            Reset();
        }

        /// <summary>ベットを確定し、振り出しから最初の2つの目を振る。</summary>
        public void Start(BetBoard bets)
        {
            Require(SugorokuPhase.Betting);
            WagerCheck.ThrowIfInvalid(rules, bets);
            Bets = bets.Clone();
            RoundNumber++;
            Reset();
            Roll();
            Phase = SugorokuPhase.Choosing;
        }

        /// <summary>出ている目の index 番目（0 か 1）で進む。上がるか回数を使い切ったら終わり、そうでなければ次の目を振る。</summary>
        public SugorokuStep Choose(int index)
        {
            Require(SugorokuPhase.Choosing);
            if (index != 0 && index != 1)
                throw new ArgumentOutOfRangeException(nameof(index));
            int die = index == 0 ? Dice.first : Dice.second;
            int from = Position;
            int landed = Math.Min(rules.Goal, from + die);
            var (to, tier) = rules.Land(from, Tier, die);
            var step = new SugorokuStep(die, from, landed, rules.At(landed), to, Tier, tier);
            steps.Add(step);
            Position = to;
            Tier = tier;
            TurnsLeft--;
            if (Position >= rules.Goal || TurnsLeft == 0)
                Phase = SugorokuPhase.Finished;
            else
                Roll();
            return step;
        }

        public SugorokuResult Settle()
        {
            Require(SugorokuPhase.Finished);
            bool reached = Position >= rules.Goal;
            var bets = new List<SpotResult>();
            foreach (var bet in Bets.NonZero())
            {
                // 端数は切り捨て
                long ret = reached ? checked(bet.Value * rules.PayFor(Tier) / 100) : 0;
                var outcome = ret > bet.Value ? BetOutcome.Win : ret == bet.Value ? BetOutcome.Push : BetOutcome.Lose;
                bets.Add(new SpotResult(bet.Key, bet.Value, outcome, ret));
            }
            Phase = SugorokuPhase.Complete;
            return new SugorokuResult(reached, Tier, steps.ToArray(), bets);
        }

        /// <summary>精算前の道中を無効にする（障害時の救済）。返金は呼び出し側が行う。</summary>
        public void Abort()
        {
            if (Phase == SugorokuPhase.Choosing || Phase == SugorokuPhase.Finished)
                Phase = SugorokuPhase.Complete;
        }

        void Roll() => Dice = (random.Next(6) + 1, random.Next(6) + 1);

        void Reset()
        {
            Position = 0;
            Tier = rules.StartTier;
            TurnsLeft = rules.Turns;
            steps.Clear();
        }

        void Require(SugorokuPhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"現在のフェーズは {Phase} です（{phase} が必要）。");
        }
    }
}
