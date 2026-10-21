using System;

namespace Vector.Core.Alchemia
{
    /// <summary>盤面の1マスのシンボル（元素と位）。位 0 は空きマス。</summary>
    public readonly struct Symbol : IEquatable<Symbol>
    {
        /// <summary>最高の位（賢者の石）。</summary>
        public const int MaxTier = 5;

        public readonly Element Element;

        /// <summary>1（素）〜 5（賢者の石）。0 は空き。</summary>
        public readonly byte Tier;

        public Symbol(Element element, int tier)
        {
            if (tier < 0 || tier > MaxTier)
                throw new ArgumentOutOfRangeException(nameof(tier));
            Element = element;
            Tier = (byte)tier;
        }

        public static Symbol Empty => default;

        public bool IsEmpty => Tier == 0;

        public bool Equals(Symbol other) => Element == other.Element && Tier == other.Tier;
        public override bool Equals(object obj) => obj is Symbol other && Equals(other);
        public override int GetHashCode() => (int)Element * 8 + Tier;
        public static bool operator ==(Symbol a, Symbol b) => a.Equals(b);
        public static bool operator !=(Symbol a, Symbol b) => !a.Equals(b);

        public override string ToString() => IsEmpty ? "--" : "FWAEL"[(int)Element] + Tier.ToString();
    }
}
