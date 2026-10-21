using System.Collections.Generic;
using System.Text;
using Vector.Core.Sugoroku;
using Vector.Game.UI;

namespace Vector.Game.Sugoroku
{
    /// <summary>SUGOROKU の遊び方（数値はルールと最善手の表から生成する）。</summary>
    public static class SugorokuHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(SugorokuRules rules, SugorokuSolver solver)
        {
            var pays = new StringBuilder();
            for (int tier = 0; tier <= rules.MaxTier; tier++)
                pays.Append($"段 {tier}  <color=#FFE7A3>{SugorokuLabels.Pay(rules, tier)}</color>{(tier == rules.StartTier ? "（振出の段）" : "")}\n");
            var squares = new StringBuilder();
            foreach (var square in new[] { Square.Coin, Square.Treasure, Square.Omen, Square.Bridge, Square.Pit })
                squares.Append($"{SugorokuLabels.Glyph(square)}  {SugorokuLabels.Effect(rules, square)}\n");

            return new[]
            {
                ("SUGOROKU とは",
                    "振出から上りまで、双六の道を旅するゲームです。\n\n" +
                    "1. チップを置いて START\n" +
                    $"2. サイコロを2つ振り、<b>どちらか1つの目</b>だけ進みます（{rules.Turns} 回まで）\n" +
                    "3. 止まったマスで、倍率の段が上がったり下がったり、進んだり戻されたりします（通り過ぎるだけなら何も起きません）\n" +
                    $"4. {rules.Turns} 回のうちに上りへ着けば、<b>そのときの段の配当</b>が戻ります。着けなければ没収です\n\n" +
                    "大きい目で急ぐか、小さい目で小判を拾うか。先を読んで選ぶのがこのゲームの腕の見せどころです。"),
                ("マスと配当", squares + "\n" + pays + "\n払い戻しの端数は切り捨てです。"),
                ("RTP と最善手",
                    $"最善を尽くしたときの <b>RTP は {solver.Return:P2}</b>（全ての状態を後ろ向きに解いた厳密値）です。\n\n" +
                    "いつも大きい目で急ぐと小判が集まらず、RTP は大きく下がります。目先の小判だけを追うのも、凶や落とし穴を踏みやすくなります。\n\n" +
                    "設定の「戦略ヒント」を ON にすると、厳密な最善手の目が光り、それぞれの目の期待値が出ます。"),
                ("テーブルについて",
                    $"・ミニマム {Labels.Money(rules.MinBet)}・マキシマム {Labels.Money(rules.MaxBet)}\n" +
                    $"・ベットの持ち時間は {rules.BettingSeconds:0}秒（チップを置くと時間が動きます）\n" +
                    "・目を選ぶ時間に制限はありません（キーボードなら 1 / 2 か ← / →）\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
