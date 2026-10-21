using UnityEngine;

namespace Vector.Game.UI.Widgets
{
    /// <summary>報酬の表示1つ分（アイコン・色・数量の文言）。</summary>
    public readonly struct Reward
    {
        public readonly Sprite Icon;
        public readonly Color Tint;
        public readonly string Amount;

        public Reward(Sprite icon, Color tint, string amount)
        {
            Icon = icon;
            Tint = tint;
            Amount = amount;
        }
    }
}
