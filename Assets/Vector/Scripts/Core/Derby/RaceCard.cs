using System;
using System.Collections.Generic;

namespace Vector.Core.Derby
{
    /// <summary>
    /// 1レースの出走表（距離・各走者のゲート・着順の確率・発売オッズ）。発売中の馬券の上下限も答える。
    /// </summary>
    public sealed class RaceCard : IBetLimits
    {
        readonly DerbyRules rules;
        readonly int[] gates;
        readonly int[] oddsTenths = new int[DerbyTickets.Count];

        public RaceCard(DerbyRules rules, int distance, IReadOnlyList<int> gates)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            if (gates == null || gates.Count != DerbyTickets.Runners)
                throw new ArgumentException("走者4人ぶんのゲートが必要です。", nameof(gates));
            if (distance < 1 || distance > RaceProbabilities.SuitSize)
                throw new ArgumentOutOfRangeException(nameof(distance));
            Distance = distance;
            this.gates = new int[DerbyTickets.Runners];
            var needed = new int[DerbyTickets.Runners];
            var full = new int[DerbyTickets.Runners];
            for (int r = 0; r < DerbyTickets.Runners; r++)
            {
                if (gates[r] < 0 || gates[r] >= distance)
                    throw new ArgumentOutOfRangeException(nameof(gates));
                this.gates[r] = gates[r];
                needed[r] = distance - gates[r];
                full[r] = RaceProbabilities.SuitSize;
            }
            Probabilities = RaceProbabilities.Compute(needed, full);
            for (int t = 0; t < DerbyTickets.Count; t++)
                oddsTenths[t] = DerbyTickets.IsValid(t) ? rules.OddsTenthsFor(Probabilities.Of(t)) : 0;
        }

        /// <summary>ゴールの位置（歩数）。</summary>
        public int Distance { get; }

        /// <summary>走者のスート位置（0 〜 Distance-1）。</summary>
        public IReadOnlyList<int> Gates => gates;

        public RaceProbabilities Probabilities { get; }

        /// <summary>馬券のオッズ（10倍した値、払い戻しは賭け金×オッズ）。発売しないなら 0。</summary>
        public int OddsTenths(int ticket) => DerbyTickets.IsValid(ticket) ? oddsTenths[ticket] : 0;

        /// <summary>的中時の勝ち額の倍率（賭け金を含まない）。</summary>
        public Odds PaysFor(int ticket)
        {
            int tenths = OddsTenths(ticket);
            if (tenths == 0)
                throw new InvalidOperationException("発売していない馬券です。");
            return new Odds(tenths - 10, 10);
        }

        /// <summary>この馬券の厳密な RTP（的中確率 × オッズ）。</summary>
        public double ReturnOf(int ticket) => Probabilities.Of(ticket) * OddsTenths(ticket) / 10.0;

        public bool IsSpotAvailable(int spot) => OddsTenths(spot) > 0;

        public long MinFor(int spot) => DerbyTickets.IsWin(spot) ? rules.WinMinBet : rules.ExactaMinBet;

        public long MaxFor(int spot) => DerbyTickets.IsWin(spot) ? rules.WinMaxBet : rules.ExactaMaxBet;

        public bool IsPrimary(int spot) => true;

        /// <summary>距離とゲートを抽選して出走表を作る。</summary>
        public static RaceCard Draw(DerbyRules rules, IRandomSource random)
        {
            int distance = rules.MinDistance + random.Next(rules.MaxDistance - rules.MinDistance + 1);
            var gates = new int[DerbyTickets.Runners];
            for (int r = 0; r < gates.Length; r++)
                gates[r] = random.Next(rules.MaxGate + 1);
            return rules.CardFor(distance, gates);
        }
    }
}
