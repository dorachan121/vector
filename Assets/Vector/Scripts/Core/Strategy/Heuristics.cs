using System;

namespace Vector.Core
{
    /// <summary>
    /// 盤面の形勢評価。空きマスには平均点（7）のカードが入ると見なし、各ラインの見込み合計を
    /// log-sum-exp で滑らかに最大化した値を「見込み得点」とする。盤面が埋まっていれば実際の得点差を返す。
    /// </summary>
    public static class Heuristics
    {
        /// <summary>シューから引かれるカードの平均点数（A〜Kの平均）。</summary>
        const double MeanCardValue = 7.0;

        /// <summary>最大値をどれだけ滑らかにするか。有望なラインが複数あるほど少し高く評価する。</summary>
        const double Temperature = 1.5;

        /// <summary>プレイヤー視点の形勢。正ならプレイヤー有利。</summary>
        public static double PlayerAdvantage(int[] values, LineFamily playerFamily)
        {
            if (Scoring.EmptyCount(values) == 0)
                return Scoring.FamilyScore(values, playerFamily) - Scoring.DealerScore(values, playerFamily);
            return FamilyPotential(values, playerFamily) - DealerPotential(values, playerFamily);
        }

        /// <summary>ディーラー視点の形勢。正ならディーラー有利。</summary>
        public static double DealerAdvantage(int[] values, LineFamily playerFamily) => -PlayerAdvantage(values, playerFamily);

        static double DealerPotential(int[] values, LineFamily playerFamily)
            => Math.Max(FamilyPotential(values, Lines.Other(playerFamily, 0)), FamilyPotential(values, Lines.Other(playerFamily, 1)));

        static double FamilyPotential(int[] values, LineFamily family)
        {
            var lines = Lines.Of(family);
            double max = double.MinValue;
            foreach (var line in lines)
                max = Math.Max(max, LinePotential(values, line));
            double acc = 0;
            foreach (var line in lines)
                acc += Math.Exp((LinePotential(values, line) - max) / Temperature);
            return max + Temperature * Math.Log(acc);
        }

        static double LinePotential(int[] values, Line line)
            => CellPotential(values[line.A]) + CellPotential(values[line.B]) + CellPotential(values[line.C]);

        static double CellPotential(int value) => value > 0 ? value : MeanCardValue;
    }
}
