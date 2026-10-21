namespace Vector.Core.Colosseum
{
    /// <summary>
    /// COLOSSEUM のルールと配当（不変）。カードは毎回 52 枚のデッキから1枚を引く無限デッキ（A=1 … K=13）。
    /// 体力の差は Tools/Simulator の厳密計算で、最善に打ってもハウスエッジが残るように決めた値（README 参照）。
    /// </summary>
    public sealed class BattleRules : IBetLimits
    {
        public const int Main = 0;

        /// <summary>K（13）を引いて斬ると会心の一撃で2倍のダメージ。</summary>
        public const int CriticalValue = 13;

        public const int CardValues = 13;

        public static readonly BattleRules Standard = new BattleRules(new BattleSetup(30, 42));

        public BattleRules(BattleSetup setup)
        {
            Setup = setup;
        }

        public BattleSetup Setup { get; }

        public long MinBet { get; } = 10;
        public long MaxBet { get; } = 10_000;

        /// <summary>獣を倒したときの配当。</summary>
        public Odds WinPays { get; } = new Odds(1);

        /// <summary>
        /// 決着がつかないまま続けられる手数の上限。互いの一撃が同じ点数（相殺）だと何も起きないため、
        /// 理論上は終わらない可能性がある。上限に達したら引き分けとして返金する（確率は無視できるほど小さい）。
        /// </summary>
        public int MaxTurns { get; } = 99;

        public float BettingSeconds { get; } = 15f;
        public float DecisionSeconds { get; } = 20f;

        public bool IsSpotAvailable(int spot) => spot == Main;
        public long MinFor(int spot) => MinBet;
        public long MaxFor(int spot) => MaxBet;
        public bool IsPrimary(int spot) => spot == Main;

        /// <summary>斬ったときのダメージ。</summary>
        public static int StrikeDamage(int cardValue) => cardValue == CriticalValue ? cardValue * 2 : cardValue;
    }
}
