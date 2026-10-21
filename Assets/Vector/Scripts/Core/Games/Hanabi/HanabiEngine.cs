using System;
using System.Collections.Generic;

namespace Vector.Core.Hanabi
{
    /// <summary>
    /// HANABI の1発の進行を管理する純粋なロジック。
    /// <para>流れ: BeginBetting → Launch（暴発点を抽選）→ CashOut（開く）か Burst（弾けた）→ Settle</para>
    /// 暴発点は打ち上げた瞬間に決まり、画面はそれに向かって倍率を上げていくだけ。いつ開くかだけがプレイヤーの選択で、どこで開いても RTP は変わらない。
    /// </summary>
    public sealed class HanabiEngine
    {
        readonly HanabiRules rules;
        readonly IRandomSource random;
        int crash;

        public HanabiEngine(HanabiRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public HanabiRules Rules => rules;
        public HanabiPhase Phase { get; private set; } = HanabiPhase.Betting;
        public BetBoard Bets { get; private set; } = new BetBoard();
        public int RoundNumber { get; private set; }

        /// <summary>開いた倍率（百分の一倍。開けなかったら 0）。</summary>
        public int CashedAt { get; private set; }

        public void BeginBetting()
        {
            if (Phase != HanabiPhase.Complete && Phase != HanabiPhase.Betting)
                throw new InvalidOperationException("花火が精算されていません。");
            Phase = HanabiPhase.Betting;
        }

        /// <summary>ベットを確定して打ち上げる（暴発点はこの瞬間に決まる）。</summary>
        public void Launch(BetBoard bets)
        {
            Require(HanabiPhase.Betting);
            WagerCheck.ThrowIfInvalid(rules, bets);
            Bets = bets.Clone();
            RoundNumber++;
            CashedAt = 0;
            crash = rules.CrashFor(random.Next(HanabiRules.Resolution));
            Phase = HanabiPhase.Rising;
        }

        /// <summary>玉が倍率 hundredths まで昇ろうとしたとき、もう弾けているか（暴発点ちょうどまでは昇る。画面が弾ける瞬間を知るため）。</summary>
        public bool HasBurstAt(int hundredths)
        {
            Require(HanabiPhase.Rising);
            return hundredths > crash;
        }

        /// <summary>倍率 hundredths で開く。暴発点以下ならその倍率で払い戻しが決まり、越えていれば開けずに終わる。開けたら true。</summary>
        public bool CashOut(int hundredths)
        {
            Require(HanabiPhase.Rising);
            if (hundredths < rules.MinCashOut)
                throw new ArgumentOutOfRangeException(nameof(hundredths), "1.01 倍未満では開けません。");
            bool opened = hundredths <= crash;
            CashedAt = opened ? hundredths : 0;
            Phase = HanabiPhase.Finished;
            return opened;
        }

        /// <summary>開く前に弾けた。</summary>
        public void Burst()
        {
            Require(HanabiPhase.Rising);
            CashedAt = 0;
            Phase = HanabiPhase.Finished;
        }

        public HanabiResult Settle()
        {
            Require(HanabiPhase.Finished);
            var bets = new List<SpotResult>();
            foreach (var bet in Bets.NonZero())
            {
                // 端数は切り捨て
                long ret = CashedAt > 0 ? checked(bet.Value * CashedAt / HanabiRules.One) : 0;
                bets.Add(new SpotResult(bet.Key, bet.Value, ret > bet.Value ? BetOutcome.Win : ret == bet.Value ? BetOutcome.Push : BetOutcome.Lose, ret));
            }
            Phase = HanabiPhase.Complete;
            return new HanabiResult(crash, CashedAt, bets);
        }

        /// <summary>精算前の花火を無効にする（障害時の救済）。返金は呼び出し側が行う。</summary>
        public void Abort()
        {
            if (Phase == HanabiPhase.Rising || Phase == HanabiPhase.Finished)
                Phase = HanabiPhase.Complete;
        }

        void Require(HanabiPhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"現在のフェーズは {Phase} です（{phase} が必要）。");
        }
    }
}
