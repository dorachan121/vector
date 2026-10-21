using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>フロアの道順（幅優先探索。上下左右の1歩ずつで最短）。</summary>
    public static class Pathfinder
    {
        /// <summary>
        /// from から to までの道順（from を含まず to を含む）。たどり着けなければ null。
        /// to が歩けないマス（人やテーブル）なら、その隣の一番近いマスまでの道順を返す。
        /// </summary>
        public static List<(int x, int y)> Find(TileMap map, (int x, int y) from, (int x, int y) to)
        {
            if (from == to)
                return new List<(int, int)>();
            bool targetFree = map.IsFree(to.x, to.y);
            var previous = new Dictionary<int, int>();
            var queue = new Queue<(int x, int y)>();
            int Key((int x, int y) p) => p.y * map.Width + p.x;
            queue.Enqueue(from);
            previous[Key(from)] = -1;
            (int x, int y)? reached = null;
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (targetFree ? current == to : IsAdjacent(current, to))
                {
                    reached = current;
                    break;
                }
                foreach (var (dx, dy) in Directions.Steps)
                {
                    var next = (current.x + dx, current.y + dy);
                    if (!map.IsFree(next.Item1, next.Item2) || previous.ContainsKey(Key(next)))
                        continue;
                    previous[Key(next)] = Key(current);
                    queue.Enqueue(next);
                }
            }
            if (reached == null)
                return null;
            var path = new List<(int, int)>();
            for (int k = Key(reached.Value); k != Key(from); k = previous[k])
                path.Add((k % map.Width, k / map.Width));
            path.Reverse();
            return path;
        }

        public static bool IsAdjacent((int x, int y) a, (int x, int y) b)
            => (a.x == b.x && (a.y - b.y == 1 || b.y - a.y == 1)) || (a.y == b.y && (a.x - b.x == 1 || b.x - a.x == 1));
    }
}
