using System.Collections.Generic;
using System.Text;
using Vector.Core.Crown;
using Vector.Game.UI;

namespace Vector.Game.Crown
{
    /// <summary>CROWN DICE の遊び方（数値はルールから生成する）。</summary>
    public static class CrownHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(CrownRules rules, double? optimalReturn)
        {
            var opening = new StringBuilder();
            foreach (var pair in rules.OpeningPays)
                opening.Append($"{CrownLabels.Opening(pair.Key)}  <color=#FFE7A3>{pair.Value}</color>\n");

            return new[]
            {
                ("CROWN DICE とは",
                    "5個のダイスで<b>ディーラーより強い手</b>を作るゲームです。\n\n" +
                    "1. ANTE にチップを置いて ROLL\n" +
                    "2. 金色の<b>王冠のダイス</b>が振られ、このラウンドの「王冠の目」が決まります\n" +
                    $"3. あなたが5個を振り、残す目を選んで<b>最大{rules.RollsPerHand - 1}回まで振り直し</b>ます（いつでも STAND で確定）\n" +
                    $"4. ディーラーがあなたの手を見てから、最大{rules.RollsPerHand}回振ります\n" +
                    "5. 強い方の勝ち。同じ強さは引き分け（PUSH）でアンティは返金"),
                ("役と強さ",
                    "強い順に YACHT（5個同じ）> FOUR OF A KIND > FULL HOUSE > STRAIGHT（1〜5 か 2〜6）> THREE OF A KIND > TWO PAIR > ONE PAIR > NO HAND\n\n" +
                    "同じ役なら、組になっている目の強さで比べます（数の多い組から順に）。\n" +
                    "<color=#FFE7A3>王冠の目は 6 より強い</color>ので、王冠が 2 のラウンドなら 2 のペアが 6 のペアに勝ちます。\n" +
                    "ストレートだけは王冠に関係なく 2-3-4-5-6 が強く、完全に同じ目の組み合わせだけが引き分けです。"),
                ("ディーラーのハウスウェイ",
                    "ディーラーに裁量はありません。あなたの確定した手を見たうえで、<b>あなたに勝つ見込みが最も高くなる目を残して</b>振ります（引き分けは半分の勝ちとして数えます）。\n" +
                    "すでにあなたより強い手なら、そこで止めます。\n\n" +
                    "あなたは先に振るぶん不利ですが、ディーラーの手を気にせず最善の残し方を選べます。"),
                ("ボーナスとオープニング",
                    $"・<b>CROWN YACHT</b>（5個とも王冠の目）で勝負を終えると、勝敗に関係なくアンティに {rules.CrownYachtBonus} のボーナス\n\n" +
                    $"OPENING は最初に振った5個の役に賭けるサイドベットです（{Labels.Money(rules.OpeningMinBet)}〜{Labels.Money(rules.OpeningMaxBet)}）。\n" +
                    "最初の1回で決まるので、振り直しの打ち方には左右されません。\n\n" + opening),
                ("戦略ヒントと RTP",
                    "このゲームは、残す目の全組み合わせを後ろから厳密に計算できます。\n" +
                    "設定で<b>戦略ヒント</b>を ON にすると、最善の残し方が光り、各ボタンに期待値（アンティ1枚あたりの払い戻し）が表示されます。\n\n" +
                    (optimalReturn.HasValue
                        ? $"・最善に打った場合のアンティの RTP は <color=#FFE7A3>{optimalReturn.Value:P2}</color>（厳密計算）\n"
                        : "・最善に打った場合のアンティの RTP は計算中です\n") +
                    $"・判断の持ち時間は {rules.DecisionSeconds:0}秒。時間切れは推奨の手で進みます\n" +
                    "・キーボード: 1〜5 で目を残す / R で振り直し / S で STAND\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
