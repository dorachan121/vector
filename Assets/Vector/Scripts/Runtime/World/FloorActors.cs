using System.Collections.Generic;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;

namespace Vector.Game.World
{
    /// <summary>
    /// フロアにいる人（主人公と登場人物）の表示。主人公の位置と向き、1マスずつの移動、手前と奥の重なり順を扱う。
    /// </summary>
    public sealed class FloorActors
    {
        /// <summary>1マス進むのにかかる時間（秒）。</summary>
        public const float StepSeconds = 0.15f;

        const float PersonSize = 90f;

        readonly FloorView view;
        readonly TileMap map;
        readonly IReadOnlyDictionary<char, string> ids;
        readonly Dictionary<string, CharacterView> people = new Dictionary<string, CharacterView>();
        readonly Dictionary<string, (int x, int y)> spots = new Dictionary<string, (int x, int y)>();

        public FloorActors(FloorView view, FloorDef floor, (int x, int y) start, int facing)
        {
            this.view = view;
            map = floor.Map;
            ids = floor.People;
            foreach (var entry in map.People)
            {
                string id = ids[entry.Key];
                var person = CharacterView.Create(view.People, id, Cast.Of(id).Look, PersonSize, view.TileCenter(entry.Value.x, entry.Value.y));
                person.SetName(StoryNames.Person(id));
                people[id] = person;
                spots[id] = entry.Value;
            }
            Player = CharacterView.Create(view.People, "Player", Cast.Player, PersonSize, view.TileCenter(start.x, start.y));
            Player.SetFacing(facing);
            PlayerTile = start;
            OnPlayerMoved();
        }

        public CharacterView Player { get; }
        public (int x, int y) PlayerTile { get; private set; }

        /// <summary>このフロアにいる人の id。</summary>
        public IEnumerable<string> People => people.Keys;

        /// <summary>主人公の向いている先のマス。</summary>
        public (int x, int y) FacingTile
        {
            get
            {
                var (dx, dy) = Directions.Step(Player.Facing);
                return (PlayerTile.x + dx, PlayerTile.y + dy);
            }
        }

        /// <summary>そのマスに立っている人の id（いなければ null）。</summary>
        public string PersonAt((int x, int y) tile)
        {
            char c = map.PersonAt(tile.x, tile.y);
            return c != '\0' ? ids[c] : null;
        }

        public void SetMark(string id, string mark) => people[id].SetBubble(mark);

        /// <summary>その向きへ1マス進む。進めなければ向きだけ変えて false。</summary>
        public bool TryStep(int facing)
        {
            Player.SetFacing(facing);
            var (dx, dy) = Directions.Step(facing);
            var to = (x: PlayerTile.x + dx, y: PlayerTile.y + dy);
            if (!map.IsFree(to.x, to.y))
                return false;
            PlayerTile = to;
            Tween.Move(Player.Rect, view.TileCenter(to.x, to.y), StepSeconds, Ease.Linear);
            Player.Bob(StepSeconds);
            OnPlayerMoved();
            return true;
        }

        /// <summary>主人公を隣のマスのほうへ向ける。</summary>
        public void Face((int x, int y) tile) => Player.SetFacing(Directions.Of(tile.x - PlayerTile.x, tile.y - PlayerTile.y));

        /// <summary>その人を主人公のほうへ向ける（話しかけられたとき）。</summary>
        public void TurnToPlayer(string id)
        {
            if (!people.TryGetValue(id, out var person))
                return;
            var at = spots[id];
            person.SetFacing(Directions.Of(PlayerTile.x - at.x, PlayerTile.y - at.y));
        }

        void OnPlayerMoved()
        {
            // 隣にいる人だけ名前を出す
            foreach (var entry in people)
                entry.Value.ShowName(Pathfinder.IsAdjacent(spots[entry.Key], PlayerTile));
            SortByDepth();
        }

        /// <summary>画面の下（手前）にいる人ほど前面に描く。</summary>
        void SortByDepth()
        {
            var order = new List<(int y, Transform transform)> { (PlayerTile.y, Player.transform) };
            foreach (var entry in people)
                order.Add((spots[entry.Key].y, entry.Value.transform));
            order.Sort((a, b) => a.y.CompareTo(b.y));
            for (int i = 0; i < order.Count; i++)
                order[i].transform.SetSiblingIndex(i);
        }
    }
}
