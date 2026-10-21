using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>
    /// CROWN DICE のルールと配当表（不変）。配当は Tools/Simulator の厳密計算で検証した値（README 参照）。
    /// </summary>
    public sealed class CrownRules : IBetLimits
    {
        /// <summary>メインのベット（アンティ）。</summary>
        public const int Ante = 0;

        /// <summary>最初の5個の役に賭けるサイドベット。</summary>
        public const int Opening = 1;

        public static readonly CrownRules Standard = new CrownRules(new Odds(5));

        /// <param name="crownYachtBonus">王冠ヨットのアンティボーナス。シミュレーターで別の値を検証するときに差し替える。</param>
        public CrownRules(Odds crownYachtBonus)
        {
            CrownYachtBonus = crownYachtBonus;
        }

        public long MinBet { get; } = 10;
        public long MaxBet { get; } = 10_000;
        public long OpeningMinBet { get; } = 5;
        public long OpeningMaxBet { get; } = 1_000;

        /// <summary>1人が振れる回数（最初の1回を含む）。プレイヤーもディーラーも同じ。</summary>
        public int RollsPerHand { get; } = 3;

        /// <summary>ディーラーに勝ったときのアンティの配当。引き分けは返金。</summary>
        public Odds WinPays { get; } = new Odds(1);

        /// <summary>王冠の目のヨットで勝負を終えたとき、勝敗に関係なくアンティに上乗せするボーナス。</summary>
        public Odds CrownYachtBonus { get; }

        public IReadOnlyDictionary<OpeningHand, Odds> OpeningPays { get; } = new Dictionary<OpeningHand, Odds>
        {
            { OpeningHand.CrownYacht, new Odds(1000) },
            { OpeningHand.Yacht, new Odds(100) },
            { OpeningHand.CrownFour, new Odds(30) },
            { OpeningHand.FourOfAKind, new Odds(15) },
            { OpeningHand.FullHouse, new Odds(4) },
            { OpeningHand.Straight, new Odds(4) },
            { OpeningHand.CrownThree, new Odds(1) },
        };

        /// <summary>ベット受付の制限時間（秒）。</summary>
        public float BettingSeconds { get; } = 15f;

        /// <summary>振り直しの判断の制限時間（秒）。時間切れは戦略ヒントの手で自動進行。</summary>
        public float DecisionSeconds { get; } = 25f;

        public bool IsSpotAvailable(int spot) => spot == Ante || spot == Opening;

        public long MinFor(int spot) => spot == Opening ? OpeningMinBet : MinBet;

        public long MaxFor(int spot) => spot == Opening ? OpeningMaxBet : MaxBet;

        public bool IsPrimary(int spot) => spot == Ante;
    }
}
