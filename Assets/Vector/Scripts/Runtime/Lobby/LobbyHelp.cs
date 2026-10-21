using System.Collections.Generic;

namespace Vector.Game.Lobby
{
    /// <summary>ロビーの「HOW TO PLAY」（カジノ全体の案内）。各ゲームのルールはテーブルの HOW TO PLAY で見られる。</summary>
    public static class LobbyHelp
    {
        public static IReadOnlyList<(string title, string body)> Pages()
        {
            var games = new System.Text.StringBuilder();
            foreach (var entry in GameCatalog.All)
                games.Append($"<b>{entry.Title}</b>  <color=#A7B5AC>{entry.Genre}</color>\n{entry.Pitch}\n\n");
            return new[]
            {
                ("VECTOR CASINO RESORT へようこそ",
                    "ここは、10のオリジナルのカジノゲームがそろうカジノリゾートです。\n\n" +
                    "・<b>プレイ</b> では、好きなテーブルにすぐ座れます\n" +
                    "・<b>ストーリー</b> では、カジノの中を歩いてフロアを攻略し、各フロアの主に挑みます\n" +
                    "・<b>ミッション</b> では、毎日のミッションとログインボーナスを受け取れます\n" +
                    "・<b>ショップ</b> では、コンプポイントを景品と交換できます\n\n" +
                    "ホームとストーリーは縦向き、ゲームのテーブルは横向きで遊びます。\n\n" +
                    "どのゲームも、配当は付属のシミュレーターで検証しており（厳密計算または大量の試行）、" +
                    "最善に近い打ち方でもハウス（カジノ側）がわずかに有利になるように作っています。"),
                ("ゲーム一覧", games.ToString()),
                ("遊ぶうえでの約束",
                    "・チップは<b>遊戯用</b>です。現金・景品とは交換できません\n" +
                    "・手持ちがテーブルの最低額を下回ったら、ショップ（フロアではキャッシャー）で遊戯用チップを補充できます\n" +
                    "・30分ごとにプレイ時間と収支をお知らせします（リアリティチェック）\n" +
                    "・勝負の途中でアプリが終了した場合、次回起動時にその賭け金を返金します\n\n" +
                    "<color=#FF7B7B>PLAY MONEY ONLY — NO REAL-MONEY GAMBLING.</color>"),
            };
        }
    }
}
