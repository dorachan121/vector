using System.Collections.Generic;
using UnityEngine;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Game.UI;

namespace Vector.Game.World
{
    /// <summary>
    /// フロアの見取り図を描く（床・壁・テーブル・エレベーター・キャッシャー・装飾）。1マスは 90px で、12マスの横幅が縦画面にちょうど収まる。
    /// 縦は画面に収まらないので、<see cref="MapFollow"/> で主人公に合わせて流す。
    /// </summary>
    public sealed class FloorView
    {
        public const float TileSize = 90f;


        readonly TileMap map;
        readonly FloorTheme theme;
        readonly FloorDef floor;

        public RectTransform Root { get; }
        public RectTransform Ground { get; }
        public RectTransform Objects { get; }
        public RectTransform People { get; }

        FloorView(Transform parent, FloorDef floor)
        {
            this.floor = floor;
            map = floor.Map;
            theme = FloorTheme.Of(floor.Id);
            Root = Ui.Rect(parent, "Floor", Vector2.zero, new Vector2(map.Width * TileSize, map.Height * TileSize));
            Ground = Ui.Rect(Root, "Ground", Vector2.zero, Vector2.zero);
            Objects = Ui.Rect(Root, "Objects", Vector2.zero, Vector2.zero);
            People = Ui.Rect(Root, "People", Vector2.zero, Vector2.zero);
        }

        public static FloorView Build(Transform parent, FloorDef floor)
        {
            var view = new FloorView(parent, floor);
            view.DrawGround();
            view.DrawObjects();
            return view;
        }

        /// <summary>マス (x, y) の中心（Root の座標）。</summary>
        public Vector2 TileCenter(int x, int y) => new Vector2((x - (map.Width - 1) / 2f) * TileSize, ((map.Height - 1) / 2f - y) * TileSize);

        /// <summary>Root の座標からマス。</summary>
        public (int x, int y) TileAt(Vector2 local)
        {
            int x = Mathf.FloorToInt(local.x / TileSize + map.Width / 2f);
            int y = Mathf.FloorToInt(map.Height / 2f - local.y / TileSize);
            return (x, y);
        }

        void DrawGround()
        {
            var floorTile = Art.Carpet(theme.Ground, theme.Motif);
            var rugTile = Art.Carpet(theme.Rug, theme.Motif);
            var carpetTile = Art.Carpet(theme.Carpet, theme.Trim);
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var tile = map[x, y];
                    var at = TileCenter(x, y);
                    if (tile == Tile.Wall)
                    {
                        Ui.Image(Ground, "Wall", null, theme.Wall, at, Vector2.one * TileSize);
                        // 床に面した壁の下端に金の見切り
                        if (y + 1 < map.Height && map[x, y + 1] != Tile.Wall)
                            Ui.Image(Ground, "Trim", null, theme.Trim.WithAlpha(0.8f), at + new Vector2(0, -TileSize / 2 + 3), new Vector2(TileSize, 6));
                        continue;
                    }
                    var sprite = tile == Tile.Rug ? rugTile : tile == Tile.Carpet ? carpetTile : floorTile;
                    Ui.Image(Ground, "Floor", sprite, Color.white, at, Vector2.one * TileSize);
                }
            }
        }

        void DrawObjects()
        {
            var done = new HashSet<(int, int)>();
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var tile = map[x, y];
                    if (done.Contains((x, y)) || tile == Tile.Wall || TileMap.IsWalkable(tile))
                        continue;
                    var group = map.GroupAt(x, y);
                    foreach (var cell in group)
                        done.Add(cell);
                    switch (tile)
                    {
                        case Tile.Table: DrawTable(group); break;
                        case Tile.Bar: DrawBar(group); break;
                        case Tile.Water: DrawWater(group); break;
                        default:
                            foreach (var (cx, cy) in group)
                                DrawSingle(tile, TileCenter(cx, cy));
                            break;
                    }
                }
            }
        }

        (Vector2 center, Vector2 size) Bounds(List<(int x, int y)> group)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var (x, y) in group)
            {
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
            var center = (TileCenter(minX, minY) + TileCenter(maxX, maxY)) / 2;
            return (center, new Vector2((maxX - minX + 1) * TileSize, (maxY - minY + 1) * TileSize));
        }

        void DrawTable(List<(int x, int y)> group)
        {
            var (center, size) = Bounds(group);
            string title = floor.Id == FloorId.Penthouse ? "THE HOUSE" : GameCatalog.Of(floor.Game).Title;
            switch (floor.Game)
            {
                case GameId.Alchemia:
                {
                    // スロットマシンの筐体
                    var cabinet = Ui.Panel(Objects, "Slot", center + new Vector2(0, 4), new Vector2(62, 74), Theme.Hex("2A1D52"), 10);
                    Ui.Outline(cabinet.transform, "Trim", Vector2.zero, new Vector2(62, 74), Theme.Hex("C79BFF").WithAlpha(0.8f), 10);
                    Ui.Panel(cabinet.transform, "Screen", new Vector2(0, 8), new Vector2(46, 34), Theme.Hex("0B0820"), 6);
                    var gem = Ui.Image(cabinet.transform, "Gem", Art.Gem(5), Theme.Hex("FF5A36"), new Vector2(0, 8), new Vector2(28, 28));
                    Flicker.Add(gem);
                    Ui.Panel(cabinet.transform, "Lever", new Vector2(36, 6), new Vector2(6, 30), Theme.Gold, 3);
                    return;
                }
                case GameId.Delve:
                {
                    // 迷宮への門
                    Ui.Image(Objects, "Gate", Art.Door, Color.white, center, new Vector2(size.y * 0.64f, size.y) * 0.92f);
                    Ui.Text(Objects, "Label", "DELVE", Fonts.Label, 18, Theme.GoldBright, center + new Vector2(0, -size.y / 2 + 12), new Vector2(160, 22));
                    return;
                }
                case GameId.Orbit:
                    DrawOrrery(center, Mathf.Min(size.x, size.y) - 16);
                    return;
                case GameId.Harpoon:
                    DrawTank(center, size - new Vector2(16, 16));
                    return;
                case GameId.Sugoroku:
                    DrawBoardGame(center, size - new Vector2(16, 16));
                    return;
                case GameId.Hanabi:
                    DrawLaunchStand(center, size - new Vector2(16, 16));
                    return;
            }
            var felt = floor.Id == FloorId.Penthouse
                ? Art.TableIn(Theme.Hex("3A0E16"), Theme.Hex("12040A"))
                : Art.TableIn(theme.FeltCenter, theme.FeltEdge);
            var inset = size - new Vector2(14, 14);
            Ui.Panel(Objects, "Shadow", center + new Vector2(4, -6), inset, new Color(0, 0, 0, 0.4f), 26);
            Ui.Image(Objects, "Table", felt, Color.white, center, inset);
            var label = Ui.Text(Objects, "Title", title, Fonts.Display, Mathf.Min(30, inset.y * 0.28f), Theme.Gold.WithAlpha(0.9f), center, new Vector2(inset.x, 40));
            label.characterSpacing = 4;
            if (floor.Game == GameId.Derby)
                Ui.Text(Objects, "Window", "BETTING WINDOW", Fonts.Label, 14, Theme.FeltPrint, center + new Vector2(0, -20), new Vector2(inset.x, 20));
        }

        /// <summary>天球儀の円卓（太陽のまわりを3つの惑星がゆっくり回る）。</summary>
        void DrawOrrery(Vector2 center, float diameter)
        {
            Ui.Image(Objects, "Shadow", Art.Circle, new Color(0, 0, 0, 0.4f), center + new Vector2(4, -6), Vector2.one * diameter);
            Ui.Image(Objects, "Table", Art.Circle, theme.FeltEdge, center, Vector2.one * diameter);
            Ui.Image(Objects, "Felt", Art.Circle, theme.FeltCenter, center, Vector2.one * (diameter - 12));
            Ui.Image(Objects, "Rim", Art.Ring(4f / diameter), theme.Trim.WithAlpha(0.7f), center, Vector2.one * diameter);
            var colors = new[] { Orbit.OrbitLabels.InnerColor, Orbit.OrbitLabels.MiddleColor, Orbit.OrbitLabels.OuterColor };
            for (int p = 0; p < colors.Length; p++)
            {
                float radius = diameter * (0.17f + 0.11f * p);
                Ui.Image(Objects, "Orbit", Art.Ring(3f / (radius * 2)), colors[p].WithAlpha(0.5f), center, Vector2.one * radius * 2);
                // 内側ほど速く回る
                var arm = Ui.Rect(Objects, "Arm", center, Vector2.zero);
                arm.localEulerAngles = new Vector3(0, 0, 90 + 130 * p);
                Ui.Image(arm, "Planet", Art.Circle, colors[p], new Vector2(radius, 0), Vector2.one * (10 + 2 * p));
                Spin.Add(arm, -36f / (p + 1));
            }
            var sun = Ui.Image(Objects, "Sun", Art.Circle, Palette.Gold, center, Vector2.one * 22);
            Pulse.Add(sun, 0.08f, 1.6f);
        }

        /// <summary>HARPOON の卓（魚の泳ぐ水槽と、縁に据えたモリの発射台）。</summary>
        void DrawTank(Vector2 center, Vector2 size)
        {
            Ui.Panel(Objects, "Shadow", center + new Vector2(4, -6), size, new Color(0, 0, 0, 0.4f), 20);
            Ui.Image(Objects, "Water", Chrome.Gradient(Theme.Hex("3FA9C9"), theme.FeltEdge), Color.white, center, size);
            Ui.Outline(Objects, "Rim", center, size, theme.Trim.WithAlpha(0.8f), 20);
            var fish = new[] { Core.Harpoon.Quarry.Clownfish, Core.Harpoon.Quarry.Sardine, Core.Harpoon.Quarry.Puffer };
            for (int i = 0; i < fish.Length; i++)
            {
                var spec = Harpoon.SwimSpec.Of(fish[i]);
                var swimmer = Ui.Image(Objects, "Fish", Harpoon.MarineArt.Of(fish[i]), Color.white,
                    center + new Vector2((i - 1) * size.x * 0.28f, size.y * (0.22f - 0.2f * i)), spec.Size * 0.42f);
                Float.Add(swimmer.rectTransform, 6f, 1.2f + i * 0.3f);
            }
            var gun = Ui.Image(Objects, "Harpoon", Harpoon.MarineArt.Harpoon, Palette.GoldLight, center + new Vector2(0, -size.y / 2 + 14), new Vector2(70, 15));
            gun.rectTransform.localEulerAngles = new Vector3(0, 0, 70);
        }

        /// <summary>SUGOROKU の卓（漆の台に広げた双六の盤と、2つのサイコロ）。</summary>
        void DrawBoardGame(Vector2 center, Vector2 size)
        {
            Ui.Panel(Objects, "Shadow", center + new Vector2(4, -6), size, new Color(0, 0, 0, 0.4f), 16);
            Ui.Panel(Objects, "Lacquer", center, size, theme.FeltCenter, 16);
            var paper = size - new Vector2(20, 20);
            Ui.Panel(Objects, "Board", center, paper, Sugoroku.SugorokuLabels.Paper, 8);
            // 蛇行する道のマス（3段）
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var at = center + new Vector2((col - 1.5f) * paper.x / 4.4f, (row - 1) * paper.y / 3.4f);
                    var color = (row * 4 + col) % 5 == 2 ? Theme.Hex("C9962E") : (row * 4 + col) % 7 == 3 ? Sugoroku.SugorokuLabels.Vermilion : Sugoroku.SugorokuLabels.Ink.WithAlpha(0.25f);
                    Ui.Panel(Objects, "Cell", at, new Vector2(paper.x / 5.2f, paper.y / 4.2f), color, 3);
                }
            }
            for (int i = 0; i < 2; i++)
            {
                var die = Ui.Panel(Objects, "Die", center + new Vector2(-16 + i * 30, -size.y / 2 - 2), new Vector2(24, 24), Theme.CardFace, 5);
                die.rectTransform.localEulerAngles = new Vector3(0, 0, i == 0 ? -12 : 10);
                Ui.Image(die.transform, "Pip", Art.Circle, i == 0 ? Theme.CardRed : Theme.Ink, Vector2.zero, new Vector2(7, 7));
            }
        }

        /// <summary>HANABI の卓（打ち上げ台に並んだ筒と、その上で開いた花火）。</summary>
        void DrawLaunchStand(Vector2 center, Vector2 size)
        {
            Ui.Panel(Objects, "Shadow", center + new Vector2(4, -6), size, new Color(0, 0, 0, 0.4f), 16);
            Ui.Panel(Objects, "Stand", center, size, theme.FeltCenter, 16);
            Ui.Outline(Objects, "Rim", center, size, theme.Trim.WithAlpha(0.7f), 16);
            for (int i = 0; i < 3; i++)
            {
                var tube = center + new Vector2((i - 1) * size.x * 0.28f, -size.y * 0.22f);
                Ui.Panel(Objects, "Tube", tube, new Vector2(18, 40), Theme.Hex("3A2414"), 4);
                Ui.Panel(Objects, "Band", tube + new Vector2(0, 14), new Vector2(22, 6), Palette.GoldDark, 3);
            }
            var bloom = center + new Vector2(0, size.y * 0.16f);
            Ui.Image(Objects, "Glow", Art.Glow, theme.Trim.WithAlpha(0.5f), bloom, Vector2.one * size.x * 0.8f);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI / 6;
                var spark = Ui.Image(Objects, "Spark", Art.Circle, i % 2 == 0 ? theme.Trim : Palette.GoldLight,
                    bloom + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * size.x * 0.26f, Vector2.one * 8);
                Flicker.Add(spark);
            }
        }

        void DrawBar(List<(int x, int y)> group)
        {
            var (center, size) = Bounds(group);
            Ui.Panel(Objects, "Counter", center, size - new Vector2(6, 20), Theme.Hex("3A2414"), 12);
            Ui.Panel(Objects, "Top", center + new Vector2(0, size.y / 2 - 18), new Vector2(size.x - 6, 10), Theme.Gold.WithAlpha(0.7f), 5);
            var bottles = new[] { "2E7D4F", "B8862F", "8E1B2C", "6E8FD6" };
            for (int i = 0; i < group.Count * 2; i++)
            {
                float x = center.x - size.x / 2 + 20 + i * (size.x - 40) / Mathf.Max(1, group.Count * 2 - 1);
                Ui.Panel(Objects, "Bottle", new Vector2(x, center.y + 4), new Vector2(10, 24), Theme.Hex(bottles[i % bottles.Length]), 4);
            }
        }

        void DrawWater(List<(int x, int y)> group)
        {
            foreach (var (x, y) in group)
            {
                var at = TileCenter(x, y);
                Ui.Image(Objects, "Water", null, theme.Water, at, Vector2.one * TileSize);
                var shine = Ui.Image(Objects, "Shine", Art.Glow, Color.white.WithAlpha(0.18f), at + new Vector2((x * 37 % 20) - 10, (y * 53 % 16) - 8), new Vector2(48, 20));
                Flicker.Add(shine);
            }
        }

        void DrawSingle(Tile tile, Vector2 at)
        {
            switch (tile)
            {
                case Tile.Elevator:
                    Ui.Panel(Objects, "Elevator", at, new Vector2(70, 76), Theme.Hex("3A3F46"), 8);
                    Ui.Image(Objects, "Split", null, Theme.Hex("1E2226"), at + new Vector2(0, -2), new Vector2(3, 62));
                    Ui.Outline(Objects, "Frame", at, new Vector2(70, 76), Theme.Gold, 8);
                    Ui.Text(Objects, "Arrows", "▲▼", Fonts.Body, 16, Theme.GoldBright, at + new Vector2(0, 26), new Vector2(60, 20));
                    break;
                case Tile.Cashier:
                    Ui.Panel(Objects, "Cashier", at, new Vector2(74, 70), Theme.Hex("5A3A1E"), 10);
                    Ui.Outline(Objects, "Frame", at, new Vector2(74, 70), Theme.Gold, 10);
                    Ui.Image(Objects, "Chip", Art.Chip(Theme.Hex("1B1B1F"), Theme.Gold), Color.white, at + new Vector2(0, 8), new Vector2(34, 34));
                    Ui.Text(Objects, "Label", "CASHIER", Fonts.Label, 13, Theme.GoldBright, at + new Vector2(0, -22), new Vector2(74, 18));
                    break;
                case Tile.Plant:
                    Ui.Panel(Objects, "Pot", at + new Vector2(0, -20), new Vector2(34, 26), Theme.Hex("6B4426"), 6);
                    Ui.Image(Objects, "Leaves", Art.Circle, Theme.Hex("2E7D4F"), at + new Vector2(-10, 6), new Vector2(40, 40));
                    Ui.Image(Objects, "Leaves", Art.Circle, Theme.Hex("3FA060"), at + new Vector2(10, 12), new Vector2(38, 38));
                    Ui.Image(Objects, "Leaves", Art.Circle, Theme.Hex("2A6B42"), at + new Vector2(0, 24), new Vector2(32, 32));
                    break;
                case Tile.Pillar:
                    Ui.Image(Objects, "Shadow", Art.Circle, new Color(0, 0, 0, 0.4f), at + new Vector2(4, -6), new Vector2(66, 66));
                    Ui.Image(Objects, "Pillar", Art.Circle, Theme.Hex("D9D2C5"), at, new Vector2(62, 62));
                    Ui.Image(Objects, "Ring", Art.Ring(0.12f), Theme.Gold, at, new Vector2(62, 62));
                    break;
            }
        }
    }
}
