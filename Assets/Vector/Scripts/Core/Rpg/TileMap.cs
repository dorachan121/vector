using System;
using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// フロアの見取り図。文字の行で書き、1文字が1マス（左上が (0, 0)、x は右、y は下へ増える）。
    /// <code>
    /// # 壁   . 床   , 柄の床   = 絨毯   T テーブル   E エレベーター   C キャッシャー
    /// B バー  P 植木  O 柱   ~ 噴水   @ 主人公の出発地点   a〜z と M 人（床の上に立つ）
    /// </code>
    /// </summary>
    public sealed class TileMap
    {
        readonly Tile[] tiles;
        readonly Dictionary<char, (int x, int y)> people = new Dictionary<char, (int, int)>();

        public TileMap(IReadOnlyList<string> rows)
        {
            if (rows == null || rows.Count == 0)
                throw new ArgumentException("見取り図が空です。", nameof(rows));
            Height = rows.Count;
            Width = rows[0].Length;
            tiles = new Tile[Width * Height];
            Spawn = (-1, -1);
            for (int y = 0; y < Height; y++)
            {
                if (rows[y].Length != Width)
                    throw new ArgumentException($"{y} 行目の幅が {rows[y].Length} です（{Width} が必要）。", nameof(rows));
                for (int x = 0; x < Width; x++)
                {
                    char c = rows[y][x];
                    tiles[y * Width + x] = Parse(c);
                    if (c == '@')
                        Spawn = (x, y);
                    else if (char.IsLower(c) || c == 'M')
                    {
                        if (people.ContainsKey(c))
                            throw new ArgumentException($"人 '{c}' が2か所にいます。", nameof(rows));
                        people[c] = (x, y);
                    }
                }
            }
            if (Spawn.x < 0)
                throw new ArgumentException("出発地点 @ がありません。", nameof(rows));
        }

        public int Width { get; }
        public int Height { get; }
        public (int x, int y) Spawn { get; }

        /// <summary>見取り図に書かれた人の位置（文字 → マス）。</summary>
        public IReadOnlyDictionary<char, (int x, int y)> People => people;

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public Tile this[int x, int y] => InBounds(x, y) ? tiles[y * Width + x] : Tile.Wall;

        /// <summary>歩ける地形か（人がいるかどうかは見ない）。</summary>
        public static bool IsWalkable(Tile tile) => tile == Tile.Floor || tile == Tile.Rug || tile == Tile.Carpet;

        /// <summary>話しかけたり使ったりできる地形か。</summary>
        public static bool IsInteractive(Tile tile) => tile == Tile.Table || tile == Tile.Elevator || tile == Tile.Cashier;

        /// <summary>人が立っておらず、歩けるマス。</summary>
        public bool IsFree(int x, int y)
        {
            if (!IsWalkable(this[x, y]))
                return false;
            foreach (var p in people.Values)
            {
                if (p.x == x && p.y == y)
                    return false;
            }
            return true;
        }

        /// <summary>そのマスに立っている人（いなければ '\0'）。</summary>
        public char PersonAt(int x, int y)
        {
            foreach (var p in people)
            {
                if (p.Value.x == x && p.Value.y == y)
                    return p.Key;
            }
            return '\0';
        }

        /// <summary>同じ種類で上下左右につながったマスの塊（テーブル1台ぶんなど）。</summary>
        public List<(int x, int y)> GroupAt(int x, int y)
        {
            var group = new List<(int, int)>();
            var tile = this[x, y];
            var seen = new HashSet<int>();
            var stack = new Stack<(int x, int y)>();
            stack.Push((x, y));
            seen.Add(y * Width + x);
            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Pop();
                group.Add((cx, cy));
                foreach (var (dx, dy) in Directions.Steps)
                {
                    int nx = cx + dx, ny = cy + dy;
                    if (InBounds(nx, ny) && this[nx, ny] == tile && seen.Add(ny * Width + nx))
                        stack.Push((nx, ny));
                }
            }
            return group;
        }

        static Tile Parse(char c)
        {
            switch (c)
            {
                case '#': return Tile.Wall;
                case ',': return Tile.Rug;
                case '=': return Tile.Carpet;
                case 'T': return Tile.Table;
                case 'E': return Tile.Elevator;
                case 'C': return Tile.Cashier;
                case 'B': return Tile.Bar;
                case 'P': return Tile.Plant;
                case 'O': return Tile.Pillar;
                case '~': return Tile.Water;
                case '.':
                case '@':
                    return Tile.Floor;
                default:
                    if (char.IsLower(c) || c == 'M')
                        return Tile.Floor;
                    throw new ArgumentException($"見取り図に知らない文字 '{c}' があります。");
            }
        }
    }
}
