using System;
using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>
    /// ダイスの手の強さ。役が同じなら、組になっている目の強さで比べる（枚数の多い組から順に）。
    /// 目の強さは 1 &lt; 2 &lt; … &lt; 6 で、そのラウンドの<b>王冠の目は 6 より強い</b>。
    /// ストレートは王冠に関係なく 2-3-4-5-6 が 1-2-3-4-5 より強い。完全に同じ目の組み合わせだけが引き分けになる。
    /// </summary>
    public readonly struct DiceRank : IComparable<DiceRank>, IEquatable<DiceRank>
    {
        public const int Faces = 6;
        public const int DiceCount = 5;
        const int Base = 8;
        const int TieBreakSpan = Base * Base * Base * Base * Base;

        public readonly DiceHandKind Kind;

        /// <summary>大きいほど強い比較用の値。</summary>
        public readonly int Key;

        /// <summary>組の中心になっている目（ヨット・フォー・スリー・フルハウスの3個、ペアの高い方）。役なしとストレートは 0。</summary>
        public readonly int MainFace;

        /// <summary>2つ目の組の目（フルハウスのペア、ツーペアの低い方）。それ以外の役は 0。</summary>
        public readonly int SecondFace;

        DiceRank(DiceHandKind kind, int key, int mainFace, int secondFace)
        {
            Kind = kind;
            Key = key;
            MainFace = mainFace;
            SecondFace = secondFace;
        }

        /// <summary>目の強さ。王冠の目は 7（どの目より強い）。</summary>
        public static int Strength(int face, int crown) => face == crown ? Faces + 1 : face;

        /// <summary>ダイスの目（1〜6）の並びから。</summary>
        public static DiceRank Of(IReadOnlyList<int> dice, int crown) => OfCounts(DiceTables.CountsOf(dice), crown);

        /// <summary>目ごとの個数（添字 0 が「1の目」）から。</summary>
        public static DiceRank OfCounts(int[] counts, int crown)
        {
            if (counts == null || counts.Length != Faces)
                throw new ArgumentException("目ごとの個数は6つ必要です。", nameof(counts));
            if (crown < 1 || crown > Faces)
                throw new ArgumentOutOfRangeException(nameof(crown));

            // 組を「個数の多い順 → 強い順」に並べる
            var groups = new List<int>(DiceCount);
            int total = 0;
            for (int f = 1; f <= Faces; f++)
            {
                if (counts[f - 1] > 0)
                    groups.Add(f);
                total += counts[f - 1];
            }
            if (total != DiceCount)
                throw new ArgumentException("ダイスは5個必要です。", nameof(counts));
            groups.Sort((a, b) =>
            {
                int byCount = counts[b - 1].CompareTo(counts[a - 1]);
                return byCount != 0 ? byCount : Strength(b, crown).CompareTo(Strength(a, crown));
            });

            int first = counts[groups[0] - 1];
            int second = groups.Count > 1 ? counts[groups[1] - 1] : 0;
            bool straight = groups.Count == DiceCount && (counts[0] == 0 || counts[Faces - 1] == 0);

            DiceHandKind kind;
            if (first == 5) kind = DiceHandKind.Yacht;
            else if (first == 4) kind = DiceHandKind.FourOfAKind;
            else if (first == 3 && second == 2) kind = DiceHandKind.FullHouse;
            else if (straight) kind = DiceHandKind.Straight;
            else if (first == 3) kind = DiceHandKind.ThreeOfAKind;
            else if (first == 2 && second == 2) kind = DiceHandKind.TwoPair;
            else if (first == 2) kind = DiceHandKind.OnePair;
            else kind = DiceHandKind.NoHand;

            if (kind == DiceHandKind.Straight)
                return new DiceRank(kind, (int)kind * TieBreakSpan + (counts[Faces - 1] > 0 ? 2 : 1), 0, 0);

            int tieBreak = 0;
            foreach (int face in groups)
                tieBreak = tieBreak * Base + Strength(face, crown);
            for (int n = groups.Count; n < DiceCount; n++)
                tieBreak *= Base;
            int main = kind == DiceHandKind.NoHand ? 0 : groups[0];
            int secondFace = kind == DiceHandKind.FullHouse || kind == DiceHandKind.TwoPair ? groups[1] : 0;
            return new DiceRank(kind, (int)kind * TieBreakSpan + tieBreak, main, secondFace);
        }

        /// <summary>王冠の目のヨット（ボーナス対象）。</summary>
        public bool IsCrownYachtFor(int crown) => Kind == DiceHandKind.Yacht && MainFace == crown;

        public int CompareTo(DiceRank other) => Key.CompareTo(other.Key);
        public bool Equals(DiceRank other) => Key == other.Key;
        public override bool Equals(object obj) => obj is DiceRank other && Equals(other);
        public override int GetHashCode() => Key;
        public static bool operator >(DiceRank a, DiceRank b) => a.Key > b.Key;
        public static bool operator <(DiceRank a, DiceRank b) => a.Key < b.Key;
        public static bool operator ==(DiceRank a, DiceRank b) => a.Key == b.Key;
        public static bool operator !=(DiceRank a, DiceRank b) => a.Key != b.Key;

        public override string ToString() => $"{Kind}({MainFace})#{Key}";
    }
}
