using System;
using System.Collections.Generic;

namespace Vector.Core.Sugoroku
{
    /// <summary>
    /// SUGOROKU のルール・盤面・配当表（不変）。
    /// <para>賭けたら振り出しから始め、毎回2つのサイコロを振って<b>どちらか1つの目</b>だけ進む。決められた回数のうちに上がりへ着けば、
    /// 道中で集めた倍率の段に応じて払い戻し、着けなければ没収。止まったマスの効果（小判・宝・凶・橋・落とし穴）で段や位置が変わる。</para>
    /// 配当は <see cref="SugorokuSolver"/> が最善手で厳密に求めた RTP から決めた（README 参照）。
    /// </summary>
    public sealed class SugorokuRules : IBetLimits
    {
        public const int Main = 0;

        /// <summary>
        /// 盤面（添字がマスの番号。0 が振り出しで、最後のマスの次が上がり）。
        /// C 小判・T 宝・O 凶・B 橋・P 落とし穴・. 何もなし。
        /// </summary>
        public const string StandardLayout = "...C.O..C.B.O.T.P..C.O.C.P.OC.";

        public static readonly SugorokuRules Standard = new SugorokuRules(StandardLayout, new long[] { 0, 40, 70, 100, 160, 260, 600, 2_500 }, 8);

        readonly Square[] board;
        readonly long[] payHundredths;

        /// <param name="layout">盤面（<see cref="StandardLayout"/> と同じ書き方）。</param>
        /// <param name="payHundredths">上がったときの払い戻しの倍率（賭け金を含む）を 100 倍した値。添字が倍率の段。</param>
        /// <param name="turns">サイコロを振れる回数。</param>
        public SugorokuRules(string layout, long[] payHundredths, int turns)
        {
            if (string.IsNullOrEmpty(layout) || layout.Length < 2)
                throw new ArgumentException("盤面が必要です。", nameof(layout));
            if (payHundredths == null || payHundredths.Length < 2)
                throw new ArgumentException("段ごとの配当が必要です。", nameof(payHundredths));
            if (turns < 1)
                throw new ArgumentOutOfRangeException(nameof(turns));
            this.payHundredths = (long[])payHundredths.Clone();
            board = Parse(layout);
            Turns = turns;
        }

        /// <summary>サイコロを振れる回数。</summary>
        public int Turns { get; }

        /// <summary>上がりのマスの番号（ここに着くか、越えたら上がり）。</summary>
        public int Goal => board.Length;

        public int BridgeSteps { get; } = 3;
        public int PitSteps { get; } = 3;

        /// <summary>倍率の段の上限。</summary>
        public int MaxTier => payHundredths.Length - 1;

        /// <summary>振り出しの倍率の段。</summary>
        public int StartTier { get; } = 0;

        public long MinBet { get; } = 10;
        public long MaxBet { get; } = 500;
        public float BettingSeconds { get; } = 20f;

        public IReadOnlyList<Square> Board => board;

        /// <summary>段ごとの、上がったときの払い戻しの倍率（100倍した値）。</summary>
        public IReadOnlyList<long> PayHundredths => payHundredths;

        public long PayFor(int tier) => payHundredths[Math.Max(0, Math.Min(MaxTier, tier))];

        public Square At(int position) => position >= 0 && position < board.Length ? board[position] : Square.Plain;

        /// <summary>position から steps 進んで止まったあとの位置と段（橋・落とし穴の先では効果は続かない）。</summary>
        public (int position, int tier) Land(int position, int tier, int steps)
        {
            int to = position + steps;
            if (to >= Goal)
                return (Goal, tier);
            switch (board[to])
            {
                case Square.Coin: return (to, Math.Min(MaxTier, tier + 1));
                case Square.Treasure: return (to, Math.Min(MaxTier, tier + 2));
                case Square.Omen: return (to, Math.Max(0, tier - 1));
                case Square.Bridge: return (Math.Min(Goal, to + BridgeSteps), tier);
                case Square.Pit: return (Math.Max(0, to - PitSteps), tier);
                default: return (to, tier);
            }
        }

        public bool IsSpotAvailable(int spot) => spot == Main;
        public long MinFor(int spot) => MinBet;
        public long MaxFor(int spot) => MaxBet;
        public bool IsPrimary(int spot) => spot == Main;

        static Square[] Parse(string layout)
        {
            var squares = new Square[layout.Length];
            for (int i = 0; i < layout.Length; i++)
            {
                switch (layout[i])
                {
                    case 'C': squares[i] = Square.Coin; break;
                    case 'T': squares[i] = Square.Treasure; break;
                    case 'O': squares[i] = Square.Omen; break;
                    case 'B': squares[i] = Square.Bridge; break;
                    case 'P': squares[i] = Square.Pit; break;
                    default: squares[i] = Square.Plain; break;
                }
            }
            return squares;
        }
    }
}
