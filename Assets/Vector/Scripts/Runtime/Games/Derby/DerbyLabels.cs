using UnityEngine;
using Vector.Core.Derby;
using Vector.Game.UI;

namespace Vector.Game.Derby
{
    /// <summary>四神（走者）の名前と色。走者の番号はスートの番号（♠=青龍, ♥=朱雀, ♦=白虎, ♣=玄武）。</summary>
    public static class DerbyLabels
    {
        static readonly string[] names = { "SEIRYU", "SUZAKU", "BYAKKO", "GENBU" };
        static readonly string[] kanji = { "青龍", "朱雀", "白虎", "玄武" };
        static readonly string[] glyphs = { "龍", "雀", "虎", "亀" };
        static readonly string[] suits = { "♠", "♥", "♦", "♣" };
        static readonly Color[] colors = { Theme.Hex("27A39A"), Theme.Hex("E0462B"), Theme.Hex("ECE7DA"), Theme.Hex("3A4A42") };
        static readonly Color[] inks = { Color.white, Color.white, Theme.Ink, Theme.Hex("E9F2EC") };

        public static string Name(int runner) => names[runner];
        public static string Kanji(int runner) => kanji[runner];
        public static string Glyph(int runner) => glyphs[runner];
        public static string Suit(int runner) => suits[runner];
        public static Color ColorOf(int runner) => colors[runner];

        /// <summary>暗い背景の上で読める文字色（玄武の黒は明るい灰緑にする）。</summary>
        public static Color LabelColorOf(int runner) => runner == 3 ? Theme.Hex("9FB8AA") : colors[runner];
        public static Color InkOf(int runner) => inks[runner];

        /// <summary>馬券の名前（トーストなどに出す）。</summary>
        public static string Ticket(int spot)
        {
            if (DerbyTickets.IsWin(spot))
                return "WIN " + Kanji(spot);
            return $"EXACTA {Kanji(DerbyTickets.First(spot))}→{Kanji(DerbyTickets.Second(spot))}";
        }

        /// <summary>オッズの表記（10倍した値 → 3.4）。発売しないなら —。</summary>
        public static string Odds(int tenths) => tenths > 0 ? (tenths / 10.0).ToString("0.0") : "—";
    }
}
