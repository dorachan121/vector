using System;
using System.Collections.Generic;

namespace Vector.Core.Delve
{
    /// <summary>
    /// DELVE の1回の探索の進行を管理する純粋なロジック。
    /// <para>流れ: Deal → （扉を選ぶ → 生き残れば次の深さ / 引き返す）→ Settle</para>
    /// 魔物の扉は、扉を開けた瞬間にその深さの扉の中から無作為に選ぶ（事前に決めておくのと確率は同じ）。
    /// 賭け金の引き落とし・払い戻しは呼び出し側が行う。
    /// </summary>
    public sealed class DelveEngine
    {
        readonly DelveRules rules;
        readonly IRandomSource random;

        public DelveEngine(DelveRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public DelveRules Rules => rules;
        public DelvePhase Phase { get; private set; } = DelvePhase.Betting;
        public BetBoard Bets { get; private set; } = new BetBoard();
        public int RoundNumber { get; private set; }

        /// <summary>無事に抜けた深さ。</summary>
        public int ClearedDepth { get; private set; }

        /// <summary>次に挑む深さ（1始まり）。</summary>
        public int CurrentDepth => ClearedDepth + 1;

        public bool Busted { get; private set; }

        /// <summary>今引き返せるか（1つ以上の深さを抜けている）。</summary>
        public bool CanCashOut => Phase == DelvePhase.Choosing && ClearedDepth > 0;

        public void BeginBetting()
        {
            if (Phase != DelvePhase.Complete && Phase != DelvePhase.Betting)
                throw new InvalidOperationException("探索が終わっていません。");
            Phase = DelvePhase.Betting;
        }

        public void Deal(BetBoard bets)
        {
            Require(DelvePhase.Betting);
            WagerCheck.ThrowIfInvalid(rules, bets);
            Bets = bets.Clone();
            RoundNumber++;
            ClearedDepth = 0;
            Busted = false;
            Phase = DelvePhase.Choosing;
        }

        /// <summary>今の深さの扉を1つ開ける。</summary>
        public DoorReveal Open(int door)
        {
            Require(DelvePhase.Choosing);
            var floor = rules.Floors[ClearedDepth];
            if (door < 0 || door >= floor.Doors)
                throw new ArgumentOutOfRangeException(nameof(door));

            // 扉の中から魔物の数だけ無作為に選ぶ（部分的な Fisher–Yates）
            var order = new int[floor.Doors];
            for (int i = 0; i < order.Length; i++)
                order[i] = i;
            var monsters = new bool[floor.Doors];
            for (int i = 0; i < floor.Monsters; i++)
            {
                int j = i + random.Next(order.Length - i);
                (order[i], order[j]) = (order[j], order[i]);
                monsters[order[i]] = true;
            }

            var reveal = new DoorReveal(CurrentDepth, door, monsters);
            if (!reveal.Survived)
            {
                Busted = true;
                Phase = DelvePhase.Finished;
            }
            else if (++ClearedDepth == rules.Depths)
            {
                Phase = DelvePhase.Finished;
            }
            return reveal;
        }

        /// <summary>今までに抜けた深さの倍率で引き返す。</summary>
        public void CashOut()
        {
            if (!CanCashOut)
                throw new InvalidOperationException("まだ引き返せません。");
            Phase = DelvePhase.Finished;
        }

        public DelveResult Settle()
        {
            Require(DelvePhase.Finished);
            var bets = new List<SpotResult>();
            foreach (var bet in Bets.NonZero())
                bets.Add(Busted ? SpotResult.Lose(bet.Key, bet.Value) : SpotResult.Win(bet.Key, bet.Value, rules.PaysAt(ClearedDepth)));
            Phase = DelvePhase.Complete;
            return new DelveResult(ClearedDepth, Busted, bets);
        }

        /// <summary>進行中の探索を無効にする（障害時の救済）。返金は呼び出し側が行う。</summary>
        public void Abort()
        {
            if (Phase == DelvePhase.Choosing || Phase == DelvePhase.Finished)
                Phase = DelvePhase.Complete;
        }

        void Require(DelvePhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"現在のフェーズは {Phase} です（{phase} が必要）。");
        }
    }
}
