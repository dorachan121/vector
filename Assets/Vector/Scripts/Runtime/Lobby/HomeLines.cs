using System;
using System.Collections.Generic;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.World;

namespace Vector.Game.Lobby
{
    /// <summary>ホームのコンシェルジュ（レナ）の台詞。いまの状態に合わせた案内を先に、あとはふだんの台詞。</summary>
    public static class HomeLines
    {
        public static List<string> For(RpgSession session, DateTime now)
        {
            var lines = new List<string> { Greeting(now.Hour) + "VECTOR CASINO RESORT へようこそ。" };
            if (session.Daily.Claimable > 0)
                lines.Add("達成したミッションの報酬が受け取れますよ。下の「ミッション」をのぞいてみてください。");
            foreach (var floor in FloorOrder.BottomUp)
            {
                if (session.StatusOf(floor) == FloorStatus.Ready)
                {
                    var master = Floors.Of(floor).Master.Master;
                    lines.Add($"{StoryNames.FloorJapanese(floor)}の主、{StoryNames.Person(master)}に挑めるようになりました。下の「ストーリー」からどうぞ。");
                    break;
                }
            }
            lines.AddRange(Cast.Of("lena").Idle);
            return lines;
        }

        static string Greeting(int hour) => hour < 5 ? "夜更かしですね。" : hour < 11 ? "おはようございます。" : hour < 18 ? "こんにちは。" : "こんばんは。";
    }
}
