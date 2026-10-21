using System.Collections.Generic;
using Vector.Core.Derby;
using Vector.Game.UI;

namespace Vector.Game.Derby
{
    /// <summary>FOUR BEASTS DERBY の遊び方（数値はルールから生成する）。</summary>
    public static class DerbyHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(DerbyRules rules)
        {
            return new[]
            {
                ("FOUR BEASTS DERBY とは",
                    "四神（<b>青龍・朱雀・白虎・玄武</b>）がトランプで走るレースに賭けるゲームです。\n\n" +
                    "・走者はスートと対応しています（♠ 青龍 / ♥ 朱雀 / ♦ 白虎 / ♣ 玄武）\n" +
                    "・1組 52 枚をよく混ぜ、1枚ずつめくります。めくったカードのスートの神獣が1歩進みます\n" +
                    "・ゴールに先に着いた順に 1着・2着\n\n" +
                    $"レースごとに<b>距離（{rules.MinDistance}〜{rules.MaxDistance}歩）</b>と、各走者の<b>ゲート（0〜{rules.MaxGate}歩前からスタート）</b>を抽選します。"),
                ("馬券の種類",
                    $"・<b>WIN（単勝）</b> 1着を当てる  {Labels.Money(rules.WinMinBet)}〜{Labels.Money(rules.WinMaxBet)}\n" +
                    $"・<b>EXACTA（馬単）</b> 1着と2着を着順どおりに当てる  {Labels.Money(rules.ExactaMinBet)}〜{Labels.Money(rules.ExactaMaxBet)}\n\n" +
                    "オッズ表のマスをクリックしてチップを置き、START でレース開始。\n" +
                    "オッズは<b>払い戻しの倍率</b>（賭け金を含む）です。3.4 のマスに $100 賭けて的中すると $340 が戻ります。"),
                ("オッズの決め方",
                    "出走表（距離とゲート）が決まると、残りのカードの枚数から<b>着順の確率を厳密に計算</b>できます。\n" +
                    $"オッズは「払戻率 {rules.TargetReturn:P0} ÷ 的中確率」を 0.1倍単位で切り捨てた値なので、どの馬券の RTP も {rules.TargetReturn:P0} 以下です。\n" +
                    $"{rules.MinOddsTenths / 10.0:0.0}倍に届かない馬券は発売しません。\n\n" +
                    "レース中は各レーンの右に、その時点からの勝つ確率（ライブ）が表示されます。"),
                ("テーブルについて",
                    "・カードの順番は暗号論的乱数でシャッフルしています\n" +
                    "・ゴール済みの神獣のカードがめくられても、何も起こりません\n" +
                    "・2着が決まった時点でレースは終了です\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
