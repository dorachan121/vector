using System.Collections.Generic;
using UnityEngine;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Game.UI;

namespace Vector.Game.World
{
    /// <summary>
    /// フロアの見取り図を描く（床・壁・テーブル・エレベーター・キャッシャー・装飾）。1マスは 80px で、22×12 マスが画面に収まる。
    /// </summary>
    public sealed class FloorView
    {
        public const float TileSize = 80f;

        /// <summary>見取り図の中心の画面上の位置（上部のバーのぶん下げる）。</summary>
        public static readonly Vector2 Center = new Vector2(0, -42);

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
            Root = Ui.Rect(parent, "Floor", Center, new Vector2(map.Width * TileSize, map.Height * TileSize));
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
