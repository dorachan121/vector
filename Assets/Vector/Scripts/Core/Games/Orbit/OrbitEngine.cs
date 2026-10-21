using System;
using System.Collections.Generic;

namespace Vector.Core.Orbit
{
    /// <summary>
    /// ORBIT の1回のスピンの進行を管理する純粋なロジック。
    /// <para>流れ: BeginBetting → Spin（ベットを確定して結果を抽選）→ Settle</para>
    /// 結果はスピンした瞬間に決まり、惑星が回る演出はそれをなぞるだけ。賭け金の引き落とし・払い戻しは呼び出し側が行う。
    /// </summary>
    public sealed class OrbitEngine
    {
        readonly OrbitRules rules;
        readonly IRandomSource random;

        public OrbitEngine(OrbitRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public OrbitRules Rules => rules;
        public OrbitPhase Phase { get; private set; } = OrbitPhase.Betting;
        public BetBoard Bets { get; private set; } = new BetBoard();
        public int RoundNumber { get; private set; }

        /// <summary>直近のスピンの結果。</summary>
        public OrbitSpin Last { get; private set; }

        public void BeginBetting()
        {
            if (Phase != OrbitPhase.Complete && Phase != OrbitPhase.Betting)
                throw new InvalidOperationException("スピンが精算されていません。");
            Phase = OrbitPhase.Betting;
        }

        /// <summary>ベットを確定し、彗星と3つの惑星の位置を抽選する。</summary>
        public OrbitSpin Spin(BetBoard bets)
        {
            Require(OrbitPhase.Betting);
            WagerCheck.ThrowIfInvalid(rules, bets);
            Bets = bets.Clone();
            RoundNumber++;
            Last = random.Next(OrbitRules.CometOneIn) == 0
                ? OrbitSpin.CometPass
                : OrbitSpin.Planets(random.Next(OrbitSpin.InnerSlots), random.Next(OrbitSpin.MiddleSlots), random.Next(OrbitSpin.OuterSlots));
            Phase = OrbitPhase.Spun;
            return Last;
        }

        public OrbitResult Settle()
        {
            Require(OrbitPhase.Spun);
            var bets = new List<SpotResult>();
            foreach (var bet in Bets.NonZero())
                bets.Add(Last.Wins(bet.Key) ? SpotResult.Win(bet.Key, bet.Value, rules.PaysFor(bet.Key)) : SpotResult.Lose(bet.Key, bet.Value));
            Phase = OrbitPhase.Complete;
            return new OrbitResult(Last, bets);
        }

        /// <summary>精算前のスピンを無効にする（障害時の救済）。返金は呼び出し側が行う。</summary>
        public void Abort()
        {
            if (Phase == OrbitPhase.Spun)
                Phase = OrbitPhase.Complete;
        }

        void Require(OrbitPhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"現在のフェーズは {Phase} です（{phase} が必要）。");
        }
    }
}
