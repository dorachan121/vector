using System.Collections.Generic;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Story
{
    /// <summary>受け取った報酬を、報酬ポップアップの表示（アイコンと数量）に直す。</summary>
    public static class RewardDisplay
    {
        public static List<Reward> From(RewardGrant grant)
        {
            var list = new List<Reward>();
            if (grant.Chips > 0)
                list.Add(new Reward(Chrome.Coin, Color.white, Labels.Money(grant.Chips)));
            if (grant.Comps > 0)
                list.Add(new Reward(Chrome.Jewel, Palette.Cyan, $"COMP {grant.Comps}"));
            if (grant.Xp > 0)
                list.Add(new Reward(Icons.Of(IconKind.Star), Palette.Gold, $"XP {grant.Xp}"));
            if (grant.Item != null)
                list.Add(new Reward(Icons.Of(IconKind.Gift), Palette.Magenta, StoryNames.Item(grant.Item)));
            return list;
        }

        /// <summary>報酬の演出の本文。レベルが上がったなら、そのことを書き添える。</summary>
        public static string Message(string message, RewardGrant grant, int level)
            => grant.LevelsGained > 0 ? $"{message}\n<color=#FFE7A3>LEVEL UP!  Lv {level}</color>" : message;
    }
}
