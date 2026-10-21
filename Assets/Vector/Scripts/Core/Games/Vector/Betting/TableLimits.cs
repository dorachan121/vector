namespace Vector.Core
{
    /// <summary>
    /// VECTOR のモードごとのベットエリアと上下限。プレイヤーモードは MAIN、観戦モードは PLAYER / DEALER / TIE が主のベットで、
    /// BONUS はどちらのモードでも置けるサイドベット。ベットエリアの番号は <see cref="BetSpot"/> の値。
    /// </summary>
    public sealed class TableLimits : IBetLimits
    {
        readonly TableRules rules;
        readonly GameMode mode;

        public TableLimits(TableRules rules, GameMode mode)
        {
            this.rules = rules;
            this.mode = mode;
        }

        public bool IsSpotAvailable(int spot)
        {
            switch ((BetSpot)spot)
            {
                case BetSpot.Main: return mode == GameMode.Player;
                case BetSpot.Bonus: return true;
                case BetSpot.Player:
                case BetSpot.Dealer:
                case BetSpot.Tie:
                    return mode == GameMode.Audience;
                default: return false;
            }
        }

        public long MinFor(int spot) => spot == (int)BetSpot.Bonus ? rules.BonusMinBet : rules.MinBet;

        public long MaxFor(int spot) => spot == (int)BetSpot.Bonus ? rules.BonusMaxBet : rules.MaxBet;

        public bool IsPrimary(int spot) => spot != (int)BetSpot.Bonus;
    }
}
