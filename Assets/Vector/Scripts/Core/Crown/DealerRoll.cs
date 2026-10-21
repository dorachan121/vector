using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>ディーラーの1手（振った・止めた）。</summary>
    public readonly struct DealerRoll
    {
        /// <summary>残したダイスの位置。最初の1回は 0（全部振る）。</summary>
        public readonly int HeldMask;

        /// <summary>この手の後のディーラーの目。</summary>
        public readonly IReadOnlyList<int> Dice;

        /// <summary>振らずに止めた。</summary>
        public readonly bool Stood;

        public DealerRoll(int heldMask, IReadOnlyList<int> dice, bool stood)
        {
            HeldMask = heldMask;
            Dice = dice;
            Stood = stood;
        }
    }
}
