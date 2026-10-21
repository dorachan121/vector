namespace Vector.Core.Rpg
{
    /// <summary>フロアの進み具合。</summary>
    public enum FloorStatus : byte
    {
        /// <summary>まだ行けない（下のフロアの主に勝っていない）。</summary>
        Locked,

        /// <summary>行けるが、主に挑むにはレベルが足りない。</summary>
        Open,

        /// <summary>主に挑める。</summary>
        Ready,

        /// <summary>主に勝った。</summary>
        Cleared,
    }
}
