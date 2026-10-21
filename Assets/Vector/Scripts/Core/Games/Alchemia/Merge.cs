using System.Collections.Generic;

namespace Vector.Core.Alchemia
{
    /// <summary>1つの融合（同じ元素が3つ以上つながった塊）。</summary>
    public readonly struct Merge
    {
        public readonly Element Element;

        /// <summary>塊のマス（番号は左上から右へ、行ごとに 0〜）。</summary>
        public readonly IReadOnlyList<int> Cells;

        /// <summary>融合した結果のシンボルが残るマス（塊の中で最も位の高いマス）。</summary>
        public readonly int Anchor;

        /// <summary>融合でできた位。賢者の石がさらに融合した（大錬成）なら 0。</summary>
        public readonly int NewTier;

        /// <summary>この融合の配当（賭け金に対する倍率を100倍した値。連鎖倍率を含む）。</summary>
        public readonly long PayHundredths;

        public Merge(Element element, IReadOnlyList<int> cells, int anchor, int newTier, long payHundredths)
        {
            Element = element;
            Cells = cells;
            Anchor = anchor;
            NewTier = newTier;
            PayHundredths = payHundredths;
        }

        /// <summary>賢者の石が融合した大錬成。盤面のその元素がすべて消える。</summary>
        public bool IsTransmutation => NewTier == 0;
    }
}
