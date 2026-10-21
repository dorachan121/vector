using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// テーブルで精算した1ラウンドの知らせ（経験値・コンプ・クエストの集計用）。
    /// Tags はゲームごとの出来事（例: "crown.yacht"）で、クエストの条件に使う（<see cref="RoundTags"/>）。
    /// </summary>
    public readonly struct RoundEvent
    {
        public readonly GameId Game;
        public readonly long Stake;
        public readonly long Return;
        public readonly IReadOnlyCollection<string> Tags;

        public RoundEvent(GameId game, long stake, long totalReturn, IReadOnlyCollection<string> tags)
        {
            Game = game;
            Stake = stake;
            Return = totalReturn;
            Tags = tags ?? new string[0];
        }

        public bool Won => Return > Stake;

        public bool Has(string tag)
        {
            foreach (var t in Tags)
            {
                if (t == tag)
                    return true;
            }
            return false;
        }
    }
}
