using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// テーブルのルールと配当表（不変）。配当は Tools/Simulator で検証した値で、
    /// 戦略ヒントどおりに打った場合でもハウスエッジが残るように設定している（README 参照）。
    /// </summary>
    public sealed class TableRules
    {
        public static readonly TableRules Standard = new TableRules(new Odds(1));

        /// <param name="mainPays">メインベットの配当。シミュレーターで別の配当を検証するときに差し替える。</param>
        public TableRules(Odds mainPays)
        {
            MainPays = mainPays;
        }

        public int DeckCount { get; } = 6;

        public long MinBet { get; } = 10;
        public long MaxBet { get; } = 10_000;
        public long BonusMinBet { get; } = 5;
        public long BonusMaxBet { get; } = 1_000;

        /// <summary>メインベットの配当（ROW / COLUMN / SLASH 共通）。</summary>
        public Odds MainPays { get; }

        public Odds BackPlayerPays { get; } = new Odds(1);
        public Odds BackDealerPays { get; } = new Odds(9, 10);
        public Odds BackTiePays { get; } = new Odds(7);

        public IReadOnlyDictionary<PokerHand, Odds> BonusPays { get; } = new Dictionary<PokerHand, Odds>
        {
            { PokerHand.RoyalFlush, new Odds(1000) },
            { PokerHand.StraightFlush, new Odds(250) },
            { PokerHand.FiveOfAKind, new Odds(150) },
            { PokerHand.FourOfAKind, new Odds(40) },
            { PokerHand.FullHouse, new Odds(20) },
            { PokerHand.Flush, new Odds(15) },
            { PokerHand.Straight, new Odds(12) },
            { PokerHand.ThreeOfAKind, new Odds(8) },
            { PokerHand.TwoPair, new Odds(4) },
        };

        /// <summary>ディーラーの先読み（ハウスウェイ）で1マスあたりに試すプレイアウト数。</summary>
        public int HouseWayPlayouts { get; } = 96;

        /// <summary>ベット受付の制限時間（秒）。</summary>
        public float BettingSeconds { get; } = 15f;

        /// <summary>ライン選択・カード配置の制限時間（秒）。時間切れは戦略ヒントの手で自動進行。</summary>
        public float DecisionSeconds { get; } = 25f;

        public bool IsSpotAvailable(GameMode mode, BetSpot spot)
        {
            switch (spot)
            {
                case BetSpot.Main: return mode == GameMode.Player;
                case BetSpot.Bonus: return true;
                default: return mode == GameMode.Audience;
            }
        }

        public long MinFor(BetSpot spot) => spot == BetSpot.Bonus ? BonusMinBet : MinBet;

        public long MaxFor(BetSpot spot) => spot == BetSpot.Bonus ? BonusMaxBet : MaxBet;

        /// <summary>配る前のベット検証。</summary>
        public BetCheck Validate(GameMode mode, BetLayout bets)
        {
            bool hasPrimary = false;
            foreach (var bet in bets.NonZero())
            {
                if (!IsSpotAvailable(mode, bet.Key))
                    return new BetCheck(BetError.SpotUnavailable, bet.Key);
                if (bet.Value < MinFor(bet.Key))
                    return new BetCheck(BetError.BelowMinimum, bet.Key, MinFor(bet.Key));
                if (bet.Value > MaxFor(bet.Key))
                    return new BetCheck(BetError.AboveMaximum, bet.Key, MaxFor(bet.Key));
                hasPrimary |= bet.Key != BetSpot.Bonus;
            }
            return hasPrimary ? BetCheck.Ok : new BetCheck(BetError.NoPrimaryBet);
        }
    }
}
