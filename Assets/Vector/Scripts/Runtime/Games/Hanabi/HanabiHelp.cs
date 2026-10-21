using System.Collections.Generic;
using System.Text;
using Vector.Core.Hanabi;
using Vector.Game.UI;

namespace Vector.Game.Hanabi
{
    /// <summary>HANABI の遊び方（数値はルールから生成する）。</summary>
    public static class HanabiHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(HanabiRules rules, IReadOnlyList<int> autoTargets)
        {
            var table = new StringBuilder();
            foreach (int m in autoTargets)
            {
                if (m > 0)
                    table.Append($"{HanabiLabels.Multiple(m)} で開く  開ける確率 {rules.ProbabilityAtLeast(m):P2}  RTP {rules.ReturnAt(m):P2}\n");
            }
            return new[]
            {
                ("HANABI とは",
                    "打ち上げた花火の玉は、昇るほど倍率が上がっていきます。<b>弾ける前に「開く」</b>を押せば、そのときの倍率で払い戻しを受けられます。\n\n" +
                    "1. チップを置いて LAUNCH\n" +
                    "2. 玉が昇り、倍率が x1.00 から上がっていく\n" +
                    "3. 好きなところで「開く」（Space でも開けます）。開く前に弾けたら賭け金は没収です\n\n" +
                    "「自動で開く倍率」を決めておくと、その倍率に届いた瞬間に自動で開きます。"),
                ("確率と RTP",
                    $"弾ける高さ（暴発点）は、打ち上げた瞬間に暗号論的乱数で決まります。倍率 m まで昇る確率はほぼ <b>{HanabiRules.ReturnPercent}% ÷ m</b> なので、" +
                    $"<b>どこで開いても RTP は {HanabiRules.ReturnPercent}% を超えません</b>。いつ開くかで変わるのは、当たりやすさと大きさだけです。\n\n" +
                    $"打ち上げてすぐ（x1.01 に届く前に）弾けるのは {rules.InstantBurst:P2}。上限は x{rules.MaxHundredths / HanabiRules.One:N0} です。\n\n" + table),
                ("テーブルについて",
                    $"・ミニマム {Labels.Money(rules.MinBet)}・マキシマム {Labels.Money(rules.MaxBet)}\n" +
                    $"・ベットの持ち時間は {rules.BettingSeconds:0}秒（チップを置くと時間が動きます）\n" +
                    "・払い戻しの端数は切り捨てです\n" +
                    "・右上の履歴は、これまでの暴発点です（次の結果には関係しません）\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
