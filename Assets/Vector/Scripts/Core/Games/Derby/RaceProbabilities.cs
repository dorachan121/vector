using System;
using System.Collections.Generic;

namespace Vector.Core.Derby
{
    /// <summary>
    /// 着順の確率の厳密計算。1組 52 枚のデッキから1枚ずつ引き、引いたスートの走者が1歩進む。
    /// 局面は「各走者があと何歩でゴールか」だけで決まる（引いた枚数 = 歩いた数なので、各スートの残り枚数もそこから分かる）。
    /// ゴール済みの走者のカードは何も起こさないので、残りの走者のカードを引く確率に条件付けて数える。
    /// 途中の局面からも計算できる（ライブオッズ）。局面数は高々 13^4 なので、メモ化した再帰ですぐ終わる。
    /// </summary>
    public sealed class RaceProbabilities
    {
        public const int SuitSize = 13;
        const int Runners = DerbyTickets.Runners;

        RaceProbabilities(double[] win, double[,] exacta)
        {
            Win = win;
            Exacta = exacta;
        }

        /// <summary>走者ごとの1着の確率。</summary>
        public IReadOnlyList<double> Win { get; }

        /// <summary>[1着, 2着] の確率。</summary>
        public double[,] Exacta { get; }

        public double Of(int ticket)
            => DerbyTickets.IsWin(ticket) ? Win[ticket] : Exacta[DerbyTickets.First(ticket), DerbyTickets.Second(ticket)];

        /// <param name="needed">各走者がゴールまでに必要な残りの歩数（1着の走者は 0）。</param>
        /// <param name="remaining">デッキに残っている各スートの枚数。</param>
        /// <param name="first">すでに1着が決まっていればその走者、まだなら -1。</param>
        public static RaceProbabilities Compute(IReadOnlyList<int> needed, IReadOnlyList<int> remaining, int first = -1)
        {
            if (needed == null || needed.Count != Runners || remaining == null || remaining.Count != Runners)
                throw new ArgumentException("走者4人ぶんの値が必要です。");
            var need = new int[Runners];
            var rem = new int[Runners];
            for (int r = 0; r < Runners; r++)
            {
                need[r] = needed[r];
                rem[r] = remaining[r];
                if (need[r] < 0 || rem[r] < 0 || rem[r] > SuitSize)
                    throw new ArgumentOutOfRangeException(nameof(needed));
                if (r == first)
                    continue;
                if (need[r] == 0)
                    throw new ArgumentException("1着以外にゴール済みの走者がいます。", nameof(needed));
                if (need[r] > rem[r])
                    throw new ArgumentException("残りのカードではゴールできない走者がいます。", nameof(needed));
            }

            var solver = new Solver(need, rem);
            var win = new double[Runners];
            var exacta = new double[Runners, Runners];
            if (first >= 0)
            {
                win[first] = 1;
                int at = solver.SecondPlace(first, solver.StartIndex);
                for (int r = 0; r < Runners; r++)
                    exacta[first, r] = solver.Second[first][at + r];
            }
            else
            {
                int at = solver.FirstPlace(solver.StartIndex);
                for (int r = 0; r < Runners; r++)
                {
                    win[r] = solver.First[at + r];
                    for (int s = 0; s < Runners; s++)
                        exacta[r, s] = solver.First[at + Runners + r * Runners + s];
                }
            }
            return new RaceProbabilities(win, exacta);
        }

        /// <summary>
        /// メモ化した再帰の作業領域。局面の番号は「残り歩数」の混合基数表現で、各局面の結果を平らな配列に詰める。
        /// </summary>
        sealed class Solver
        {
            const int FirstWidth = Runners + Runners * Runners;

            readonly int[] start;
            readonly int[] remainingAtStart;
            readonly int[] stride = new int[Runners];
            readonly int[] radix = new int[Runners];
            readonly bool[] firstDone;
            readonly bool[][] secondDone = new bool[Runners][];
            readonly int count;

            public readonly double[] First;
            public readonly double[][] Second = new double[Runners][];

            public Solver(int[] start, int[] remaining)
            {
                this.start = start;
                remainingAtStart = remaining;
                count = 1;
                for (int r = Runners - 1; r >= 0; r--)
                {
                    radix[r] = start[r] + 1;
                    stride[r] = count;
                    count *= radix[r];
                }
                First = new double[count * FirstWidth];
                firstDone = new bool[count];
                for (int r = 0; r < Runners; r++)
                    StartIndex += start[r] * stride[r];
            }

            public int StartIndex { get; }

            /// <summary>1着が決まる前の局面の [単勝4つ, 馬単16個] を計算し、First の中の位置を返す。</summary>
            public int FirstPlace(int state)
            {
                int at = state * FirstWidth;
                if (firstDone[state])
                    return at;
                int pool = 0;
                for (int r = 0; r < Runners; r++)
                    pool += Remaining(state, r);
                for (int r = 0; r < Runners; r++)
                {
                    int rem = Remaining(state, r);
                    if (rem == 0)
                        continue;
                    double p = rem / (double)pool;
                    int after = state - stride[r];
                    if (Need(state, r) == 1)
                    {
                        First[at + r] += p;
                        int sAt = SecondPlace(r, after);
                        for (int s = 0; s < Runners; s++)
                            First[at + Runners + r * Runners + s] += p * Second[r][sAt + s];
                        continue;
                    }
                    int sub = FirstPlace(after);
                    for (int k = 0; k < FirstWidth; k++)
                        First[at + k] += p * First[sub + k];
                }
                firstDone[state] = true;
                return at;
            }

            /// <summary>1着が first に決まった後の局面の2着の確率を計算し、Second[first] の中の位置を返す。</summary>
            public int SecondPlace(int first, int state)
            {
                var table = Second[first] ??= new double[count * Runners];
                var done = secondDone[first] ??= new bool[count];
                int at = state * Runners;
                if (done[state])
                    return at;
                int pool = 0;
                for (int r = 0; r < Runners; r++)
                {
                    if (r != first)
                        pool += Remaining(state, r);
                }
                for (int r = 0; r < Runners; r++)
                {
                    if (r == first)
                        continue;
                    int rem = Remaining(state, r);
                    if (rem == 0)
                        continue;
                    double p = rem / (double)pool;
                    if (Need(state, r) == 1)
                    {
                        table[at + r] += p;
                        continue;
                    }
                    int sub = SecondPlace(first, state - stride[r]);
                    for (int s = 0; s < Runners; s++)
                        table[at + s] += p * table[sub + s];
                }
                done[state] = true;
                return at;
            }

            int Need(int state, int r) => state / stride[r] % radix[r];

            /// <summary>局面での走者 r の残り枚数（歩いた分だけ引かれている）。</summary>
            int Remaining(int state, int r) => remainingAtStart[r] - (start[r] - Need(state, r));
        }
    }
}
