namespace Vector.Core.Colosseum
{
    /// <summary>1手のやり取り（演出用）。</summary>
    public readonly struct BattleExchange
    {
        public readonly BattleAction Action;
        public readonly Card Intent;
        public readonly Card PlayerCard;

        /// <summary>獣の一撃の実際のダメージ（猛り狂いの加算を含む）。</summary>
        public readonly int IntentDamage;

        public readonly int DamageToBeast;
        public readonly int DamageToPlayer;

        /// <summary>K で斬った会心の一撃。</summary>
        public readonly bool Critical;

        /// <summary>受け止めた点数が一撃と同じで、何も起きなかった。</summary>
        public readonly bool Clash;

        public readonly int PlayerHp;
        public readonly int BeastHp;

        public BattleExchange(BattleAction action, Card intent, Card playerCard, int intentDamage, int damageToBeast, int damageToPlayer,
            bool critical, bool clash, int playerHp, int beastHp)
        {
            Action = action;
            Intent = intent;
            PlayerCard = playerCard;
            IntentDamage = intentDamage;
            DamageToBeast = damageToBeast;
            DamageToPlayer = damageToPlayer;
            Critical = critical;
            Clash = clash;
            PlayerHp = playerHp;
            BeastHp = beastHp;
        }

        public bool BeastDefeated => BeastHp <= 0;
        public bool PlayerDefeated => PlayerHp <= 0;
    }
}
