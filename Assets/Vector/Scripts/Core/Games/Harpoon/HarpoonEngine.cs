using System;

namespace Vector.Core.Harpoon
{
    /// <summary>
    /// HARPOON の1発を抽選する純粋なロジック。泳ぐ獲物の動きは画面の演出で、結果には関係しない
    /// （どの獲物を狙ったかだけが確率を決め、撃った瞬間に結果が決まる）。賭け金の引き落とし・払い戻しは呼び出し側が行う。
    /// </summary>
    public sealed class HarpoonEngine
    {
        readonly HarpoonRules rules;
        readonly IRandomSource random;

        public HarpoonEngine(HarpoonRules rules, IRandomSource random)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public HarpoonRules Rules => rules;

        /// <summary>これまでに撃った数。</summary>
        public int ShotNumber { get; private set; }

        /// <summary>狙った獲物1つへのベットで1発撃ち、捕まえたかどうか（金のクラゲなら倍率も）を抽選して精算する。</summary>
        public HarpoonResult Fire(BetBoard bets)
        {
            WagerCheck.ThrowIfInvalid(rules, bets);
            int quarry = -1;
            long stake = 0;
            foreach (var bet in bets.NonZero())
            {
                if (quarry >= 0)
                    throw new InvalidOperationException("1発で狙える獲物は1つです。");
                quarry = bet.Key;
                stake = bet.Value;
            }

            ShotNumber++;
            var (hits, outOf) = rules.CatchOdds(quarry);
            int multiple = random.Next(outOf) < hits ? DrawMultiple(quarry) : 0;
            var result = multiple > 0
                ? new SpotResult(quarry, stake, BetOutcome.Win, checked(stake * multiple))
                : SpotResult.Lose(quarry, stake);
            return new HarpoonResult(quarry, multiple, new[] { result });
        }

        /// <summary>捕まえた獲物の倍率を重みで選ぶ（候補が1つならそれ）。</summary>
        int DrawMultiple(int quarry)
        {
            var prizes = rules.PrizesOf(quarry);
            if (prizes.Count == 1)
                return prizes[0].Multiple;
            int total = 0;
            foreach (var p in prizes)
                total += p.Weight;
            int pick = random.Next(total);
            foreach (var p in prizes)
            {
                if (pick < p.Weight)
                    return p.Multiple;
                pick -= p.Weight;
            }
            throw new InvalidOperationException("倍率を選べませんでした。");
        }
    }
}
