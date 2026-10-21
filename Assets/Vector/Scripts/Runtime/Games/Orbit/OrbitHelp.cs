using System.Collections.Generic;
using System.Text;
using Vector.Core.Orbit;
using Vector.Game.UI;

namespace Vector.Game.Orbit
{
    /// <summary>ORBIT の遊び方（数値はルールから生成する）。</summary>
    public static class OrbitHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(OrbitRules rules)
        {
            var table = new StringBuilder();
            foreach (var (spot, what) in new[]
            {
                (OrbitSpots.Inner(0), "内惑星の位置（1〜6 のどれか）"),
                (OrbitSpots.Quadrant(0), "外惑星の方角（北東・南東・南西・北西）"),
                (OrbitSpots.Half(0), "外惑星の天地（上半分・下半分）"),
                (OrbitSpots.InnerConjunction, "内合（内惑星と中惑星が一直線）"),
                (OrbitSpots.OuterConjunction, "外合（中惑星と外惑星が一直線）"),
                (OrbitSpots.Alignment, "惑星直列（3つとも一直線）"),
                (OrbitSpots.Comet, "彗星（37 回に 1 回横切る）"),
            })
            {
                table.Append($"{what}\n  <color=#FFE7A3>{OrbitLabels.Odds(rules, spot)}</color>  当たる確率 {rules.ProbabilityOf(spot):P2}  " +
                             $"上限 {Labels.Money(rules.MaxFor(spot))}\n");
            }
            return new[]
            {
                ("ORBIT とは",
                    "天球儀のテーブルで、太陽のまわりを回る3つの惑星が<b>どこに止まり、どう並ぶか</b>に賭けるゲームです。\n\n" +
                    $"・内惑星は {OrbitSpin.InnerSlots}、中惑星は {OrbitSpin.MiddleSlots}、外惑星は {OrbitSpin.OuterSlots} の位置のどこかに止まります\n" +
                    "・太陽から見て一直線に並ぶと<b>合</b>。3つとも並べば<b>惑星直列</b>\n" +
                    $"・{OrbitRules.CometOneIn} 回に 1 回、<b>彗星</b>が横切って惑星が隠れます。そのスピンは彗星への賭けだけが当たりです\n\n" +
                    "1. 賭け先のマスにチップを置いて SPIN（いくつでも同時に賭けられます）\n" +
                    "2. 惑星が回り、内惑星・中惑星・外惑星の順に止まります\n" +
                    "3. 当たったマスに配当が付きます"),
                ("賭け方と配当", table.ToString()),
                ("どこに賭けても同じ",
                    "惑星への賭けは、彗星が来なかったときの確率どおりの<b>真のオッズ</b>で払います。\n" +
                    $"ハウスの取り分は {OrbitRules.CometOneIn} 回に 1 回の彗星だけなので、" +
                    $"<b>どの賭け方をしても RTP は 36/37 ≒ {rules.ReturnOf(OrbitSpots.Comet):P2}</b>（シングルゼロのルーレットと同じ）です。\n\n" +
                    "違うのは、当たる確率と当たったときの大きさだけ。天地でこつこつ増やすか、287倍の惑星直列を夢見るかは、あなた次第です。\n" +
                    "結果は SPIN を押した瞬間に暗号論的乱数で決まり、惑星が回る演出はそれをなぞるだけです。"),
                ("テーブルについて",
                    $"・ミニマム {Labels.Money(rules.MinBet)}（上限は賭け先ごとに違います）\n" +
                    $"・ベットの持ち時間は {rules.BettingSeconds:0}秒（チップを置くと時間が動きます）\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
