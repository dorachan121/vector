using System.Collections.Generic;
using Vector.Core.Colosseum;
using Vector.Game.UI;

namespace Vector.Game.Colosseum
{
    /// <summary>COLOSSEUM の遊び方（数値はルールから生成する）。</summary>
    public static class ColosseumHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(BattleRules rules, double optimalReturn)
        {
            var setup = rules.Setup;
            return new[]
            {
                ("COLOSSEUM とは",
                    "闘技場で<b>獣と1対1で戦う</b>カジノゲームです。獣を倒せばメインベットに " + rules.WinPays + " の配当。\n\n" +
                    $"・あなたの体力は <b>{setup.PlayerHp}</b>、獣の体力は <b>{setup.BeastHp}</b>\n" +
                    "・カードの点数は A=1、2〜10 は数字どおり、J=11、Q=12、K=13\n" +
                    "・カードは毎回 52 枚のデッキから1枚を引きます（無限デッキ。前に出たカードは関係ありません）\n\n" +
                    "獣は毎ターン、次の一撃の<b>予告カード</b>を見せてきます。それを見て「斬る」か「受ける」かを選びます。"),
                ("STRIKE（斬る）",
                    "カードを1枚引き、<b>その点数だけ獣にダメージ</b>を与えます。\n" +
                    "・<b>K を引くと会心の一撃</b>で 26 ダメージ\n" +
                    "・獣を倒しきれば、その時点で勝ち（獣の一撃は受けません）\n" +
                    "・倒しきれなかったら、獣の予告どおりのダメージを受けます\n\n" +
                    "予告が小さいときや、獣の体力が残りわずかなときに有効です。"),
                ("GUARD（受ける）",
                    "カードを1枚引き、<b>予告の一撃を受け止めます</b>。\n" +
                    "・引いた点数が予告より大きい → その差だけ獣に反撃\n" +
                    "・引いた点数が予告より小さい → その差だけダメージを受ける\n" +
                    "・同じ点数 → 相殺（何も起こらない）\n\n" +
                    "予告が大きいときに被害を抑えられます。ただし獣の体力はあまり減りません。"),
                ("勝つためのコツ",
                    "・選ぶたびに「この手を選んだときの勝率」が厳密に決まるゲームです\n" +
                    "・設定で<b>戦略ヒント</b>を ON にすると、STRIKE / GUARD それぞれの勝率と推奨（★）が表示されます\n" +
                    "・判断の持ち時間は " + $"{rules.DecisionSeconds:0}" + " 秒。時間切れは推奨の手で自動的に進みます\n" +
                    "・キーボードでは 1 = STRIKE、2 = GUARD\n\n" +
                    "体力の差は、最善の手を選び続けてもわずかにハウス（カジノ側）が有利になるように決めています。"),
                ("配当と RTP",
                    $"・獣を倒す  {rules.WinPays}\n" +
                    $"・倒される  没収\n" +
                    $"・{rules.MaxTurns} 手で決着しなければ引き分けとして返金（ほぼ起こりません）\n\n" +
                    $"最善の手を選び続けた場合の RTP は <color=#FFE7A3>{optimalReturn:P2}</color>（厳密計算）。\n" +
                    $"テーブルリミット {Labels.Money(rules.MinBet)} 〜 {Labels.Money(rules.MaxBet)}\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
