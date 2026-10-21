namespace Vector.Core.Rpg
{
    /// <summary>
    /// ログインボーナス（7日で1巡）。ログインした日数で進み、休んでもリセットしない（毎日の来場を強いない）。
    /// 報酬は遊戯用チップとコンプだけで、ゲームの確率には影響しない。
    /// </summary>
    public static class LoginBonus
    {
        public const int CycleLength = 7;

        static readonly RewardGrant[] rewards =
        {
            new RewardGrant { Chips = 500 },
            new RewardGrant { Comps = 20 },
            new RewardGrant { Chips = 1_000 },
            new RewardGrant { Comps = 40 },
            new RewardGrant { Chips = 1_500 },
            new RewardGrant { Comps = 60 },
            new RewardGrant { Chips = 3_000, Item = ItemCatalog.LuckyCoin },
        };

        /// <summary>ログインした日数（0 から）に対応する報酬（その巡の何日目か）。</summary>
        public static RewardGrant RewardFor(int loginCount) => rewards[Day(loginCount)];

        /// <summary>巡の中の何日目か（0〜6）。</summary>
        public static int Day(int loginCount) => (loginCount % CycleLength + CycleLength) % CycleLength;
    }
}
