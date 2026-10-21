using System.Collections.Generic;
using System.Text;
using Vector.Core.Harpoon;
using Vector.Game.UI;

namespace Vector.Game.Harpoon
{
    /// <summary>HARPOON の遊び方（数値はルールから生成する）。</summary>
    public static class HarpoonHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(HarpoonRules rules)
        {
            var table = new StringBuilder();
            for (int q = 0; q < Quarry.Count; q++)
            {
                table.Append($"{HarpoonLabels.Name(q)}  <color=#FFE7A3>{HarpoonLabels.Multiple(rules, q)}</color>  捕獲率 {HarpoonLabels.Chance(rules, q)}\n");
            }
            var jelly = new StringBuilder();
            int weights = 0;
            foreach (var p in rules.PrizesOf(Quarry.Jellyfish))
                weights += p.Weight;
            foreach (var p in rules.PrizesOf(Quarry.Jellyfish))
                jelly.Append($"x{p.Multiple} {p.Weight * 100.0 / weights:0.#}%  ");

            return new[]
            {
                ("HARPOON とは",
                    "深海を泳ぐ獲物を選んで<b>モリを撃つ</b>ゲームです。\n\n" +
                    "1. 下の − / + で 1発の賭け金（BET）を選ぶ\n" +
                    "2. 泳いでいる獲物を押すと、その獲物へモリを撃つ（1発ごとに BET がかかります）\n" +
                    "3. 捕まえたら <b>BET × 倍率</b> が戻ります。逃げられたら BET は没収です\n\n" +
                    "・大きな獲物ほど倍率が高く、そのぶん逃げやすくなります\n" +
                    "・LOCK を押すと、最後に狙った種類が現れるたびに自動で撃ち続けます（もう一度押すと解除）"),
                ("獲物と捕獲率", table +
                    $"\n金のクラゲは、捕まえてから倍率が決まります。\n{jelly}"),
                ("どれを狙っても同じ",
                    $"捕獲率は <b>{HarpoonRules.ReturnPercent}% ÷ 倍率</b> で決めてあり、<b>どの獲物を狙っても RTP は {HarpoonRules.ReturnPercent}%</b> です。\n\n" +
                    "違うのは、当たりやすさと当たったときの大きさだけ。イワシでこつこつ増やすか、300倍のリヴァイアサンを待つかは、あなた次第です。\n\n" +
                    "獲物の泳ぎ方や現れる順番は演出で、結果には関係しません。結果は撃った瞬間に、狙った獲物の種類だけで暗号論的乱数によって決まります。"),
                ("テーブルについて",
                    $"・1発の BET {Labels.Money(rules.BetLevels[0])}〜{Labels.Money(rules.BetLevels[rules.BetLevels.Count - 1])}\n" +
                    "・倍率は賭け金を含む払い戻しです（x2 なら BET の2倍が戻ります）\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
