using UnityEngine;
using Vector.Core;
using Vector.Core.Harpoon;
using Vector.Game.Harpoon;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>ゲームの紋章（タイルやバナーの絵）。ゲームの中身をひと目で表す小さな絵を図形で組む。座標は 200px の原図から比例で決める。</summary>
    public static class GameEmblem
    {
        public static RectTransform Create(Transform parent, Vector2 position, float size, GameId game)
        {
            var rt = Ui.Rect(parent, "Emblem", position, Vector2.one * size);
            float s = size / 200f;
            switch (game)
            {
                case GameId.Vector: Vector(rt, s); break;
                case GameId.Crown: Crown(rt, s); break;
                case GameId.Colosseum: Colosseum(rt, s); break;
                case GameId.Derby: Derby(rt, s); break;
                case GameId.Alchemia: Alchemia(rt, s); break;
                case GameId.Orbit: Orbit(rt, s); break;
                case GameId.Harpoon: Harpoon(rt, s); break;
                case GameId.Sugoroku: Sugoroku(rt, s); break;
                case GameId.Hanabi: Hanabi(rt, s); break;
                case GameId.Delve: Delve(rt, s); break;
                default: throw new System.ArgumentOutOfRangeException(nameof(game), game, "紋章のないゲームです。");
            }
            return rt;
        }

        static void Vector(RectTransform rt, float s)
        {
            var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            for (int i = 0; i < 9; i++)
            {
                var at = new Vector2((i % 3 - 1) * 58, (1 - i / 3) * 70) * s;
                Ui.Panel(rt, "Shadow", at + new Vector2(3, -4) * s, new Vector2(50, 62) * s, new Color(0, 0, 0, 0.4f), 8 * s);
                Ui.Panel(rt, "Card", at, new Vector2(50, 62) * s, Theme.CardFace, 8 * s);
                var suit = suits[i % 4];
                bool red = suit == Suit.Hearts || suit == Suit.Diamonds;
                Ui.Image(rt, "Suit", Art.Suit(suit), red ? Theme.CardRed : Theme.CardBlack, at, Vector2.one * 26 * s);
            }
            var line = Ui.Panel(rt, "Line", Vector2.zero, new Vector2(230, 12) * s, Palette.Gold, 6 * s);
            line.rectTransform.localEulerAngles = new Vector3(0, 0, -50);
            Ui.Image(rt, "Spark", Chrome.Sparkle, Palette.GoldLight, new Vector2(60, -72) * s, Vector2.one * 60 * s);
        }

        static void Crown(RectTransform rt, float s)
        {
            Die(rt, new Vector2(-40, -26) * s, 92 * s, 12, 5);
            Die(rt, new Vector2(44, -36) * s, 92 * s, -10, 6);
            Ui.Image(rt, "Glow", Art.Glow, Palette.Gold.WithAlpha(0.6f), new Vector2(0, 62) * s, Vector2.one * 150 * s);
            Ui.Icon(rt, "Crown", IconKind.Crown, Palette.Gold, new Vector2(0, 66) * s, 96 * s);
        }

        static void Die(RectTransform rt, Vector2 at, float size, float angle, int face)
        {
            var shadow = Ui.Panel(rt, "Shadow", at + new Vector2(4, -6) * (size / 92f), Vector2.one * size, new Color(0, 0, 0, 0.45f), size * 0.2f);
            var die = Ui.Panel(rt, "Die", at, Vector2.one * size, Palette.Ivory, size * 0.2f);
            shadow.rectTransform.localEulerAngles = die.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            var pips = face == 5
                ? new[] { new Vector2(-1, 1), new Vector2(1, 1), Vector2.zero, new Vector2(-1, -1), new Vector2(1, -1) }
                : new[] { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(-1, -1), new Vector2(1, -1) };
            foreach (var p in pips)
                Ui.Image(die.rectTransform, "Pip", Art.Circle, Palette.Ink, p * size * 0.27f, Vector2.one * size * 0.18f);
        }

        static void Colosseum(RectTransform rt, float s)
        {
            Ui.Image(rt, "Glow", Art.Glow, Theme.DealerRed.WithAlpha(0.45f), new Vector2(40, 10) * s, Vector2.one * 190 * s);
            Ui.Image(rt, "Beast", Art.BeastHead, Theme.Hex("6E1420"), new Vector2(44, 14) * s, Vector2.one * 150 * s);
            Ui.Image(rt, "EyeL", Art.Glow, Palette.Gold, new Vector2(30, 8) * s, Vector2.one * 26 * s);
            Ui.Image(rt, "EyeR", Art.Glow, Palette.Gold, new Vector2(58, 8) * s, Vector2.one * 26 * s);
            Ui.Image(rt, "Rim", Art.Shield, Palette.Gold, new Vector2(-46, -18) * s, new Vector2(116, 126) * s);
            Ui.Image(rt, "Shield", Art.Shield, Palette.Sapphire, new Vector2(-46, -18) * s, new Vector2(102, 112) * s);
            Ui.Image(rt, "Star", Art.Gem(4), Palette.GoldLight, new Vector2(-46, -6) * s, Vector2.one * 50 * s);
        }

        static void Derby(RectTransform rt, float s)
        {
            var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            var colors = new[] { Palette.Sapphire, Palette.Ruby, Palette.Amber, Palette.Emerald };
            for (int i = 0; i < 4; i++)
            {
                float y = (1.5f - i) * 42;
                Ui.Panel(rt, "Lane", new Vector2(0, y) * s, new Vector2(190, 32) * s, colors[i].WithAlpha(0.35f), 16 * s);
                float x = -60 + new[] { 70, 20, 96, 40 }[i];
                Ui.Image(rt, "Runner", Art.Circle, colors[i], new Vector2(x, y) * s, Vector2.one * 40 * s);
                Ui.Image(rt, "Suit", Art.Suit(suits[i]), Palette.Ivory, new Vector2(x, y) * s, Vector2.one * 22 * s);
            }
            Ui.Panel(rt, "Goal", new Vector2(96, 0) * s, new Vector2(6, 176) * s, Palette.Ivory, 3 * s);
        }

        static void Alchemia(RectTransform rt, float s)
        {
            Ui.Image(rt, "Glow", Art.Glow, Palette.Amethyst.WithAlpha(0.7f), Vector2.zero, Vector2.one * 210 * s);
            var colors = new[] { Theme.Hex("FF5A36"), Theme.Hex("3C9BFF"), Theme.Hex("3DDC97"), Theme.Hex("E0AA45") };
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI / 2 + Mathf.PI / 4;
                Ui.Image(rt, "Orb", Art.Gem(i % 2 == 0 ? 2 : 3), colors[i], new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 72 * s, Vector2.one * 40 * s);
            }
            Ui.Image(rt, "Stone", Art.Gem(5), Palette.GoldLight, Vector2.zero, Vector2.one * 120 * s);
            Ui.Image(rt, "Spark", Chrome.Sparkle, Color.white, new Vector2(30, 36) * s, Vector2.one * 70 * s);
        }

        static void Delve(RectTransform rt, float s)
        {
            Ui.Image(rt, "TorchL", Art.Glow, Palette.Amber.WithAlpha(0.7f), new Vector2(-74, 20) * s, Vector2.one * 80 * s);
            Ui.Image(rt, "TorchR", Art.Glow, Palette.Amber.WithAlpha(0.7f), new Vector2(74, 20) * s, Vector2.one * 80 * s);
            Ui.Image(rt, "Door", Art.Door, Color.white, new Vector2(0, -6) * s, new Vector2(120, 180) * s);
            Ui.Image(rt, "Light", Art.Glow, Palette.GoldLight.WithAlpha(0.55f), new Vector2(0, -40) * s, new Vector2(80, 60) * s);
        }

        /// <summary>天球儀: 太陽と3つの軌道、一直線に並んだ惑星。</summary>
        static void Orbit(RectTransform rt, float s)
        {
            var colors = new[] { Palette.Cyan, Palette.Amber, Palette.Amethyst };
            var radii = new[] { 34f, 58f, 82f };
            Ui.Image(rt, "Ray", null, Palette.GoldLight.WithAlpha(0.85f), new Vector2(31, 31) * s, new Vector2(8, 88) * s).rectTransform.localEulerAngles = new Vector3(0, 0, -45);
            for (int p = 0; p < 3; p++)
            {
                float d = radii[p] * 2;
                Ui.Image(rt, "Orbit", Art.Ring(4f / d), colors[p].WithAlpha(0.6f), Vector2.zero, Vector2.one * d * s);
                var at = new Vector2(1, 1).normalized * radii[p] * s;
                Ui.Image(rt, "Glow", Art.Glow, colors[p].WithAlpha(0.7f), at, Vector2.one * 40 * s);
                Ui.Image(rt, "Planet", Art.Circle, colors[p], at, Vector2.one * (14 + p * 4) * s);
            }
            Ui.Image(rt, "Corona", Art.Glow, Palette.GoldMid.WithAlpha(0.85f), Vector2.zero, Vector2.one * 90 * s);
            Ui.Image(rt, "Sun", Art.Circle, Palette.Gold, Vector2.zero, Vector2.one * 36 * s);
        }

        /// <summary>打ち上げ花火: 夜空に開いた大輪と、昇っていく玉の光の筋。</summary>
        static void Hanabi(RectTransform rt, float s)
        {
            var center = new Vector2(10, 30) * s;
            Ui.Image(rt, "Glow", Art.Glow, Theme.Hex("FF6FAE").WithAlpha(0.55f), center, Vector2.one * 200 * s);
            var colors = new[] { Theme.Hex("FF6FAE"), Palette.GoldLight, Palette.Cyan };
            for (int ring = 0; ring < 3; ring++)
            {
                float radius = (78 - ring * 22) * s;
                int count = 14 - ring * 3;
                for (int i = 0; i < count; i++)
                {
                    float a = i * Mathf.PI * 2 / count + ring * 0.3f;
                    var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    var spark = Ui.Image(rt, "Spark", null, colors[ring], center + dir * radius * 0.6f, new Vector2(radius * 0.7f, 5 * s));
                    spark.rectTransform.localEulerAngles = new Vector3(0, 0, a * Mathf.Rad2Deg);
                    Ui.Image(rt, "Tip", Art.Circle, colors[ring], center + dir * radius, Vector2.one * 9 * s);
                }
            }
            Ui.Image(rt, "Core", Art.Circle, Color.white, center, Vector2.one * 16 * s);
            Ui.Image(rt, "Trail", Chrome.Gradient(Palette.GoldLight.WithAlpha(0f), Palette.GoldLight), Color.white, new Vector2(-62, -66) * s, new Vector2(6, 70) * s);
            Ui.Image(rt, "Shell", Art.Circle, Palette.GoldLight, new Vector2(-62, -28) * s, Vector2.one * 12 * s);
        }

        /// <summary>双六: 小判の載った道のマスと、2つのサイコロ。</summary>
        static void Sugoroku(RectTransform rt, float s)
        {
            var paper = Theme.Hex("F1E3C2");
            var cells = new[] { new Vector2(-66, -52), new Vector2(0, -52), new Vector2(66, -52), new Vector2(66, 8) };
            Ui.Panel(rt, "Path", new Vector2(0, -52) * s, new Vector2(140, 8) * s, Theme.Hex("2B1D12"), 4 * s);
            Ui.Panel(rt, "PathUp", new Vector2(66, -22) * s, new Vector2(8, 60) * s, Theme.Hex("2B1D12"), 4 * s);
            foreach (var at in cells)
                Ui.Panel(rt, "Cell", at * s, new Vector2(54, 46) * s, paper, 8 * s);
            Ui.Image(rt, "Coin", Chrome.Coin, Color.white, new Vector2(0, -52) * s, Vector2.one * 38 * s);
            Ui.Text(rt, "Goal", "上り", Fonts.Body, 18 * s, Theme.Hex("C8352B"), new Vector2(66, 8) * s, new Vector2(54, 46) * s);
            Die(rt, new Vector2(-50, 46) * s, 64 * s, -12, 5);
            Die(rt, new Vector2(16, 62) * s, 54 * s, 16, 6);
        }

        /// <summary>深海の狩り: 泳ぐサメと、斜めに飛ぶモリと泡。</summary>
        static void Harpoon(RectTransform rt, float s)
        {
            Ui.Image(rt, "Sea", Art.Glow, Theme.Hex("34C6E0").WithAlpha(0.45f), Vector2.zero, Vector2.one * 210 * s);
            Ui.Image(rt, "Shark", MarineArt.Of(Quarry.Shark), Color.white, new Vector2(6, 12) * s, new Vector2(184, 82) * s);
            var spear = Ui.Image(rt, "Harpoon", MarineArt.Harpoon, Palette.GoldLight, new Vector2(-14, -40) * s, new Vector2(170, 36) * s);
            spear.rectTransform.localEulerAngles = new Vector3(0, 0, 28);
            for (int i = 0; i < 3; i++)
                Ui.Image(rt, "Bubble", Art.Ring(0.16f), Color.white.WithAlpha(0.7f), new Vector2(-70 + i * 14, 52 + i * 22) * s, Vector2.one * (12 + i * 6) * s);
        }
    }
}
