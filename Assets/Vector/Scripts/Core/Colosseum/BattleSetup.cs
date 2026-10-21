using System;

namespace Vector.Core.Colosseum
{
    /// <summary>対戦の初期条件（両者の体力と、獣が手負いで猛り狂ったときの追加ダメージ）。</summary>
    public readonly struct BattleSetup : IEquatable<BattleSetup>
    {
        public readonly int PlayerHp;
        public readonly int BeastHp;

        /// <summary>獣の体力が最大の 1/3 以下になると、一撃にこの値が加わる（0 なら無し）。</summary>
        public readonly int RageBonus;

        public BattleSetup(int playerHp, int beastHp, int rageBonus = 0)
        {
            if (playerHp <= 0)
                throw new ArgumentOutOfRangeException(nameof(playerHp));
            if (beastHp <= 0)
                throw new ArgumentOutOfRangeException(nameof(beastHp));
            if (rageBonus < 0)
                throw new ArgumentOutOfRangeException(nameof(rageBonus));
            PlayerHp = playerHp;
            BeastHp = beastHp;
            RageBonus = rageBonus;
        }

        public bool IsEnraged(int beastHp) => RageBonus > 0 && beastHp * 3 <= BeastHp;

        /// <summary>獣の予告カードの点数から、実際に受けるダメージ。</summary>
        public int IntentDamage(int intentValue, int beastHp) => intentValue + (IsEnraged(beastHp) ? RageBonus : 0);

        public bool Equals(BattleSetup other) => PlayerHp == other.PlayerHp && BeastHp == other.BeastHp && RageBonus == other.RageBonus;
        public override bool Equals(object obj) => obj is BattleSetup other && Equals(other);
        public override int GetHashCode() => (PlayerHp * 397 + BeastHp) * 397 + RageBonus;

        public override string ToString() => $"YOU {PlayerHp} vs BEAST {BeastHp} (+{RageBonus})";
    }
}
