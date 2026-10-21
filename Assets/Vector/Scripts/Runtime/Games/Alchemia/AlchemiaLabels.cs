using UnityEngine;
using Vector.Core.Alchemia;
using Vector.Game.UI;

namespace Vector.Game.Alchemia
{
    /// <summary>元素と位の名前と色。</summary>
    public static class AlchemiaLabels
    {
        static readonly string[] elements = { "火", "水", "風", "土", "光" };
        static readonly Color[] colors = { Theme.Hex("FF5A36"), Theme.Hex("3D8BFF"), Theme.Hex("3DDC97"), Theme.Hex("E0AA45"), Theme.Hex("E9D8FF") };
        static readonly string[] tiers = { "", "素", "晶", "霊薬", "秘宝", "賢者の石" };

        public static string Element(Element element) => elements[(int)element];

        public static Color ColorOf(Element element) => colors[(int)element];

        public static string Tier(int tier) => tiers[tier];

        public static string Multiple(long hundredths) => "x" + (hundredths / 100.0).ToString(hundredths % 100 == 0 ? "N0" : "N2");
    }
}
