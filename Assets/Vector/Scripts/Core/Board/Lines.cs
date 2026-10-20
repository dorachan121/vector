using System;
using System.Collections.Generic;

namespace Vector.Core
{
    /// <summary>
    /// 3×3盤面のライン定義。セル番号は左上から右下へ 0〜8。
    /// <code>
    /// 0 1 2
    /// 3 4 5
    /// 6 7 8
    /// </code>
    /// </summary>
    public static class Lines
    {
        public const int CellCount = 9;
        public const int CenterCell = 4;

        public static readonly IReadOnlyList<LineFamily> Families = new[] { LineFamily.Row, LineFamily.Column, LineFamily.Slash };

        static readonly Line[][] byFamily =
        {
            new[]
            {
                new Line(LineFamily.Row, 0, 0, 1, 2),
                new Line(LineFamily.Row, 1, 3, 4, 5),
                new Line(LineFamily.Row, 2, 6, 7, 8),
            },
            new[]
            {
                new Line(LineFamily.Column, 0, 0, 3, 6),
                new Line(LineFamily.Column, 1, 1, 4, 7),
                new Line(LineFamily.Column, 2, 2, 5, 8),
            },
            new[]
            {
                new Line(LineFamily.Slash, 0, 0, 4, 8),
                new Line(LineFamily.Slash, 1, 2, 4, 6),
            },
        };

        public static IReadOnlyList<Line> Of(LineFamily family) => byFamily[(int)family];

        /// <summary>指定した種類以外の2種類のうち index 番目（0 か 1）。</summary>
        public static LineFamily Other(LineFamily family, int index)
        {
            if (index < 0 || index > 1)
                throw new ArgumentOutOfRangeException(nameof(index));
            int k = 0;
            foreach (var f in Families)
            {
                if (f == family)
                    continue;
                if (k++ == index)
                    return f;
            }
            throw new ArgumentOutOfRangeException(nameof(family));
        }
    }
}
