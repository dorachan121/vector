using System.Collections.Generic;
using System.Text;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>VECTOR の遊び方（数値はルールから生成する）。</summary>
    public static class VectorHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages(TableRules rules)
        {
            var bonus = new StringBuilder();
            foreach (var pair in rules.BonusPays)
                bonus.Append($"{Labels.HandName(pair.Key)}  <color=#FFE7A3>{pair.Value}</color>\n");

            return new[]
            {
                ("VECTOR とは",
                    "3×3 の盤面にカードを置き、<b>選んだ種類のラインの合計</b>でディーラーと勝負するカジノゲームです。\n\n" +
                    "・カードの点数は A=1、2〜10 は数字どおり、J=11、Q=12、K=13\n" +
                    "・ラインは 横(ROW)3本・縦(COLUMN)3本・斜め(SLASH)2本\n" +
                    "・あなたは ROW / COLUMN / SLASH のどれか1種類を選び、その中で<b>最も合計が大きいライン</b>が得点\n" +
                    "・ディーラーは<b>残り2種類のうち高い方</b>が得点\n\n" +
                    "得点が高い方の勝ち。同点は引き分け（PUSH）でメインベットは返金されます。"),
                ("ラウンドの流れ",
                    $"1. ベット: {rules.BettingSeconds:0}秒以内にチップを置き、DEAL を押します（チップを置くとタイマーが動きます）\n" +
                    "2. 配札: 盤面の中央に<b>ベクターカード</b>、あなたの手元に<b>手札4枚</b>が配られます\n" +
                    "3. ライン選択: 手札を見て ROW / COLUMN / SLASH を選びます\n" +
                    "4. 配置: あなたが手札から1枚置き、ディーラーがシューから1枚引いて置く、を4回くり返します\n" +
                    "5. 勝負: 盤面が埋まったら得点を比べて精算します\n\n" +
                    $"ライン選択と配置には {rules.DecisionSeconds:0}秒の持ち時間があり、時間切れは戦略ヒントの手で自動的に進みます。"),
                ("勝つためのコツ",
                    "・同じマスは「1本の横ライン」と「1本の縦ライン」の両方に効きます。\n" +
                    "　大きいカードを自分のラインに置くと、ディーラーのラインも伸びることに注意しましょう\n" +
                    "・小さいカードはディーラーの有望なラインに置いて邪魔をします\n" +
                    "・SLASH は2本しかないぶん、中央のベクターカードが大きいときに強くなります\n" +
                    "・ディーラーは決められた手順（ハウスウェイ）で、ディーラーが最も勝ちやすいマスに置きます\n\n" +
                    "設定で<b>戦略ヒント</b>を ON にすると、各ラインの勝率と推奨の手が表示されます。"),
                ("観戦席（バックベット）",
                    "観戦席では AI プレイヤーとディーラーの勝負に賭けます。\n\n" +
                    $"・PLAYER  {rules.BackPlayerPays}（引き分けは返金）\n" +
                    $"・DEALER  {rules.BackDealerPays}（引き分けは返金）\n" +
                    $"・TIE       {rules.BackTiePays}\n\n" +
                    "盤面の左のスコアボードには、このシューの勝敗の履歴が並びます。"),
                ("ボーナスベット",
                    "中央のベクターカードと手札4枚の<b>5枚でできるポーカーの役</b>に賭けるサイドベットです（両方の席で使えます）。\n" +
                    $"ミニマム {Labels.Money(rules.BonusMinBet)} / マキシマム {Labels.Money(rules.BonusMaxBet)}\n\n" + bonus),
                ("テーブルについて",
                    $"・{rules.DeckCount}デッキのシューを使い、カットカードが出たら次のラウンド前にシャッフルします\n" +
                    "・カードの順番は暗号論的乱数でシャッフルしています\n" +
                    "・配当は付属のシミュレーターで検証しており、最善に近い打ち方でもハウス（カジノ側）がわずかに有利です\n" +
                    "・ラウンドの途中でアプリが終了した場合、次回起動時にそのラウンドの賭け金を返金します\n\n" +
                    "<color=#FF7B7B>このゲームのチップは遊戯用で、現金や景品とは交換できません。</color>"),
            };
        }
    }
}
