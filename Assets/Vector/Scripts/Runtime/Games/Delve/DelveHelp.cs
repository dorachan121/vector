using System.Collections.Generic;
using System.Text;
using Vector.Core.Delve;
using Vector.Game.UI;

namespace Vector.Game.Delve
{
    /// <summary>DELVE の遊び方（数値はルールから生成する）。</summary>
    public static class DelveHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(DelveRules rules)
        {
            var table = new StringBuilder();
            for (int d = 1; d <= rules.Depths; d++)
            {
                var floor = rules.Floors[d - 1];
                table.Append($"B{d}  扉{floor.Doors}・魔物{floor.Monsters}  <color=#FFE7A3>x{rules.MultiplierHundredths(d) / 100.0:N2}</color>  " +
                             $"（RTP {rules.ReturnIfCashingAt(d):P2}）\n");
            }
            return new[]
            {
                ("DELVE とは",
                    "カジノの地下に広がる迷宮を、扉を選びながら潜っていくゲームです。\n\n" +
                    "1. STAKE にチップを置いて DELVE\n" +
                    "2. 今の深さの扉から1枚を選んで開けます\n" +
                    "3. 宝なら次の深さへ。<b>持ち帰れる倍率が上がります</b>\n" +
                    "4. 魔物がいたら、賭け金は没収\n" +
                    "5. 1つ以上の深さを抜けたら、いつでも <b>CASH OUT</b> でその倍率の払い戻しを受けて引き返せます\n\n" +
                    $"最深部 B{rules.Depths} を抜けると x{rules.MultiplierHundredths(rules.Depths) / 100.0:N2} で自動的に帰還します。"),
                ("深さと倍率", table.ToString()),
                ("どこで引き返すべき？",
                    "倍率は「払戻率 ÷ その深さまで生き残る確率」を 0.01倍単位で切り捨てた値です。\n" +
                    $"そのため<b>どの深さで引き返しても RTP はほぼ同じ（{rules.TargetReturn:P0} 以下）</b>で、打ち方によって有利・不利は生まれません。\n\n" +
                    "違うのは、当たったときの大きさと、当たる確率だけ。浅い深さでこつこつ持ち帰るか、最深部の大きな宝を夢見るかは、あなた次第です。\n" +
                    "魔物の扉は、扉を開けた瞬間に暗号論的乱数で決まります。"),
                ("テーブルについて",
                    $"・テーブルリミット {Labels.Money(rules.MinBet)} 〜 {Labels.Money(rules.MaxBet)}\n" +
                    $"・判断の持ち時間は {rules.DecisionSeconds:0}秒。時間切れは、1つ以上抜けていれば CASH OUT、まだなら扉を1枚無作為に開けます（どの扉も確率は同じ）\n" +
                    "・キーボード: 1〜4 で扉を選ぶ / C で CASH OUT\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
