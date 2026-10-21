namespace Vector.Core.Rpg
{
    /// <summary>受け取った報酬（ログインボーナス・デイリーミッション）。チップは遊戯用。</summary>
    public sealed class RewardGrant
    {
        public long Chips;
        public long Comps;
        public long Xp;

        /// <summary>アイテム（なければ null）。</summary>
        public string Item;

        /// <summary>経験値で上がったレベルの数（<see cref="ApplyTo"/> が記録する）。</summary>
        public int LevelsGained;

        /// <summary>2つの報酬を合わせたもの（まとめて受け取ったときの表示用）。アイテムは先にあるほうを残す。</summary>
        public RewardGrant Plus(RewardGrant other) => new RewardGrant
        {
            Chips = Chips + other.Chips,
            Comps = Comps + other.Comps,
            Xp = Xp + other.Xp,
            Item = Item ?? other.Item,
            LevelsGained = LevelsGained + other.LevelsGained,
        };

        /// <summary>報酬を状態と所持金に加える。加えた内容（上がったレベルの数を含む）の写しを返す。</summary>
        public RewardGrant ApplyTo(RpgState state, IChipAccount chips)
        {
            var applied = new RewardGrant { Chips = Chips, Comps = Comps, Xp = Xp, Item = Item };
            if (Xp > 0)
                applied.LevelsGained = Progression.AddXp(state, Xp);
            state.comps += Comps;
            if (Chips > 0)
                chips.Credit(Chips);
            if (Item != null)
                state.AddItem(Item);
            return applied;
        }
    }
}
