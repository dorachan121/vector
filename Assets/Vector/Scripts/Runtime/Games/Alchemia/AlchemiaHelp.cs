using System.Collections.Generic;
using Vector.Core.Alchemia;

namespace Vector.Game.Alchemia
{
    /// <summary>ALCHEMIA の遊び方（数値はルールから生成する）。</summary>
    public static class AlchemiaHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(AlchemiaRules rules)
        {
            var multipliers = string.Join(" → ", rules.CascadeMultipliers);
            return new[]
            {
                ("ALCHEMIA とは",
                    $"{rules.Columns}×{rules.Rows} の錬成盤に5つの元素（火・水・風・土・光）が降ってくるビデオスロットです。\n\n" +
                    $"・同じ元素が<b>縦横に{rules.MinCluster}つ以上つながる</b>と、1つに<b>融合</b>します\n" +
                    "・融合したシンボルは、塊の中で最も位の高いマスに<b>位を1つ上げて</b>残ります\n" +
                    "・空いたマスには上のシンボルが落ち、上から新しい「素」が補充されます\n" +
                    "・新しく塊ができる限り連鎖が続きます"),
                ("位と配当",
                    "位は 素(1) → 晶(2) → 霊薬(3) → 秘宝(4) → 賢者の石(5)。\n" +
                    "配当は、位3以上のシンボルを作ったときに付きます（晶までは配当なし）。\n\n" +
                    $"・霊薬 {AlchemiaLabels.Multiple(rules.CreationPay(3))}  ·  秘宝 {AlchemiaLabels.Multiple(rules.CreationPay(4))}  ·  賢者の石 {AlchemiaLabels.Multiple(rules.CreationPay(5))}\n" +
                    $"・塊が4個なら1.5倍、5個なら2倍…と、1個増えるごとに0.5倍ずつ上乗せ\n" +
                    $"・<b>賢者の石がさらに融合すると大錬成</b> {AlchemiaLabels.Multiple(rules.TransmutationPay)}。その元素が盤面からすべて消えます\n\n" +
                    "配当の倍率は賭け金を含む払い戻しの倍率です（x1 なら賭け金がそのまま戻ります）。"),
                ("連鎖倍率",
                    $"連鎖の段ごとに、その段の配当に倍率が掛かります: {multipliers}（以降も x{rules.CascadeMultipliers[rules.CascadeMultipliers.Count - 1]}）\n\n" +
                    "連鎖が深くなるほど、高い位のシンボルができやすく、倍率も上がります。\n" +
                    $"1回のスピンの払い戻しの上限は賭け金の {rules.MaxWinMultiple:N0} 倍です。"),
                ("テーブルについて",
                    "・結果はスピンした瞬間に暗号論的乱数で決まります（演出はそれをなぞるだけで、途中の操作では変わりません）\n" +
                    "・RTP は付属のシミュレーターで 1億2千万回のスピンを回して検証しています（README 参照）\n" +
                    "・AUTO は 10 回まで。大きな当たり（50倍以上）や手持ち不足で自動的に止まります\n" +
                    "・キーボード: Space / Enter で SPIN\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
