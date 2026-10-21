using System;
using System.Collections.Generic;

namespace Vector.Core.Crown
{
    /// <summary>
    /// ダイスの厳密計算に使う表。5個の目の組み合わせ（順序を無視した多重集合）は 252 通り、
    /// 残す目の組み合わせ（0〜5個）は 462 通りしかないので、すべてを最初に列挙しておく。
    /// 作成後は読み取り専用で、複数のスレッドから同時に使ってよい。
    /// </summary>
    public static class DiceTables
    {
        /// <summary>5個の目の組み合わせの数。</summary>
        public const int HandCount = 252;

        /// <summary>残す目（0〜5個）の組み合わせの数。</summary>
        public const int HoldCount = 462;

        static readonly int[][] holdCounts = new int[HoldCount][];
        static readonly Dictionary<int, int> holdIndex = new Dictionary<int, int>();
        static readonly int[] handToHold = new int[HandCount];
        static readonly Dictionary<int, int> handIndex = new Dictionary<int, int>();

        /// <summary>各手から残せる目の組み合わせ（残す目の番号）。先頭は「全部残す」。</summary>
        static readonly int[][] holdsOfHand = new int[HandCount][];

        /// <summary>残す目ごとの、振り直した後の手（番号）とその確率。</summary>
        static readonly int[][] outcomeHands = new int[HoldCount][];
        static readonly double[][] outcomeProbabilities = new double[HoldCount][];

        static DiceTables()
        {
            int next = 0;
            int hands = 0;
            for (int size = 0; size <= DiceRank.DiceCount; size++)
            {
                foreach (var counts in Multisets(size))
                {
                    holdCounts[next] = counts;
                    holdIndex[Pack(counts)] = next;
                    if (size == DiceRank.DiceCount)
                    {
                        handToHold[hands] = next;
                        handIndex[Pack(counts)] = hands;
                        hands++;
                    }
                    next++;
                }
            }

            for (int h = 0; h < HoldCount; h++)
            {
                int rolled = DiceRank.DiceCount - Size(holdCounts[h]);
                var hands2 = new List<int>();
                var probs = new List<double>();
                double total = Math.Pow(DiceRank.Faces, rolled);
                foreach (var roll in Multisets(rolled))
                {
                    var after = new int[DiceRank.Faces];
                    double ways = Factorial(rolled);
                    for (int f = 0; f < DiceRank.Faces; f++)
                    {
                        after[f] = holdCounts[h][f] + roll[f];
                        ways /= Factorial(roll[f]);
                    }
                    hands2.Add(handIndex[Pack(after)]);
                    probs.Add(ways / total);
                }
                outcomeHands[h] = hands2.ToArray();
                outcomeProbabilities[h] = probs.ToArray();
            }

            for (int i = 0; i < HandCount; i++)
            {
                var counts = holdCounts[handToHold[i]];
                var subs = new List<int>();
                var sub = new int[DiceRank.Faces];
                void Collect(int f)
                {
                    if (f == DiceRank.Faces)
                    {
                        subs.Add(holdIndex[Pack(sub)]);
                        return;
                    }
                    for (int x = counts[f]; x >= 0; x--)
                    {
                        sub[f] = x;
                        Collect(f + 1);
                    }
                }
                Collect(0);
                holdsOfHand[i] = subs.ToArray();
            }

            InitialProbabilities = new double[HandCount];
            int empty = holdIndex[Pack(new int[DiceRank.Faces])];
            for (int j = 0; j < outcomeHands[empty].Length; j++)
                InitialProbabilities[outcomeHands[empty][j]] += outcomeProbabilities[empty][j];
        }

        /// <summary>5個を全部振ったときの各手の確率。</summary>
        public static double[] InitialProbabilities { get; }

        /// <summary>「全部残す」以外の位置のマスクを、残す個数の多い順に並べたもの（同じ期待値なら多く残す方を選ぶため）。</summary>
        public static IReadOnlyList<int> RerollMasks { get; } = BuildRerollMasks();

        /// <summary>手の番号の目ごとの個数（添字 0 が「1の目」）。返した配列は書き換えないこと。</summary>
        public static int[] CountsOfHand(int hand) => holdCounts[handToHold[hand]];

        public static int HandIndex(int[] counts) => handIndex[Pack(counts)];

        public static int HoldIndex(int[] counts) => holdIndex[Pack(counts)];

        public static IReadOnlyList<int> HoldsOf(int hand) => holdsOfHand[hand];

        /// <summary>目の並び（1〜6）から目ごとの個数へ。</summary>
        public static int[] CountsOf(IReadOnlyList<int> dice)
        {
            var counts = new int[DiceRank.Faces];
            foreach (int d in dice)
            {
                if (d < 1 || d > DiceRank.Faces)
                    throw new ArgumentOutOfRangeException(nameof(dice));
                counts[d - 1]++;
            }
            return counts;
        }

        /// <summary>位置のマスク（ビット i が立っていれば i 番目を残す）で残す目の番号。</summary>
        public static int HoldIndexOf(IReadOnlyList<int> dice, int holdMask)
        {
            var counts = new int[DiceRank.Faces];
            for (int i = 0; i < dice.Count; i++)
            {
                if ((holdMask & (1 << i)) != 0)
                    counts[dice[i] - 1]++;
            }
            return HoldIndex(counts);
        }

        /// <summary>手ごとの値から、残す目ごとに「振り直した後の値の期待値」を求める。</summary>
        public static double[] Expect(double[] handValues)
        {
            if (handValues == null || handValues.Length != HandCount)
                throw new ArgumentException("手ごとの値が必要です。", nameof(handValues));
            var e = new double[HoldCount];
            for (int h = 0; h < HoldCount; h++)
            {
                var hands = outcomeHands[h];
                var probs = outcomeProbabilities[h];
                double sum = 0;
                for (int j = 0; j < hands.Length; j++)
                    sum += probs[j] * handValues[hands[j]];
                e[h] = sum;
            }
            return e;
        }

        /// <summary>5個を全部振る場合の期待値。</summary>
        public static double ExpectInitial(double[] handValues)
        {
            double sum = 0;
            for (int i = 0; i < HandCount; i++)
                sum += InitialProbabilities[i] * handValues[i];
            return sum;
        }

        static int[] BuildRerollMasks()
        {
            var masks = new List<int>();
            for (int held = DiceRank.DiceCount - 1; held >= 0; held--)
            {
                for (int mask = (1 << DiceRank.DiceCount) - 2; mask >= 0; mask--)
                {
                    if (PopCount(mask) == held)
                        masks.Add(mask);
                }
            }
            return masks.ToArray();
        }

        static int PopCount(int x)
        {
            int n = 0;
            for (; x != 0; x &= x - 1)
                n++;
            return n;
        }

        static int Size(int[] counts)
        {
            int n = 0;
            foreach (int c in counts)
                n += c;
            return n;
        }

        static int Pack(int[] counts)
        {
            int key = 0;
            foreach (int c in counts)
                key = key * 8 + c;
            return key;
        }

        static double Factorial(int n)
        {
            double r = 1;
            for (int i = 2; i <= n; i++)
                r *= i;
            return r;
        }

        static IEnumerable<int[]> Multisets(int size)
        {
            var counts = new int[DiceRank.Faces];
            var result = new List<int[]>();
            void Fill(int f, int left)
            {
                if (f == DiceRank.Faces - 1)
                {
                    counts[f] = left;
                    result.Add((int[])counts.Clone());
                    return;
                }
                for (int x = left; x >= 0; x--)
                {
                    counts[f] = x;
                    Fill(f + 1, left - x);
                }
            }
            Fill(0, size);
            return result;
        }
    }
}
