using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Persistence;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>
    /// フロアの進行役。主人公の移動（矢印キー・WASD・クリック）、人や物への話しかけ、エレベーター・キャッシャー・メニュー、
    /// テーブルへの着席と主の挑戦の開始・決着を扱う。会話や規則の判断はそれぞれの部品に任せる。
    /// </summary>
    public sealed class WorldDirector : MonoBehaviour
    {
        static readonly KeyCode[] InteractKeys = { KeyCode.Space, KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Z };

        static readonly (KeyCode key, int facing)[] MoveKeys =
        {
            (KeyCode.DownArrow, Directions.Down), (KeyCode.S, Directions.Down),
            (KeyCode.LeftArrow, Directions.Left), (KeyCode.A, Directions.Left),
            (KeyCode.RightArrow, Directions.Right), (KeyCode.D, Directions.Right),
            (KeyCode.UpArrow, Directions.Up), (KeyCode.W, Directions.Up),
        };

        readonly Queue<(int x, int y)> route = new Queue<(int x, int y)>();
        StoryProgress story;
        ProfileService profiles;
        WorldScreen screen;
        Conversation conversation;
        FloorDef floor;
        FloorView view;
        FloorActors actors;

        /// <summary>歩いた先で話しかける相手や物（クリックで選んだとき）。</summary>
        (int x, int y)? routeTarget;

        bool busy;
        float nextStepTime;

        RpgState State => story.State;

        /// <summary>補充の基準にするミニマム（いちばん低いテーブルのミニマム）。</summary>
        static long TableMinimum => GameContext.Rules.MinBet;

        public void Begin(StoryProgress story, ProfileService profiles, WorldScreen screen)
        {
            this.story = story;
            this.profiles = profiles;
            this.screen = screen;
            conversation = new Conversation(story, screen.Dialogue, screen.Toast, StartChallenge);
            screen.Hud.TitleClicked += () =>
            {
                if (!busy)
                    Leave(SceneNames.Lobby);
            };
            screen.Hud.MenuClicked += OpenMenu;

            var id = (FloorId)State.floor;
            ShowFloor(story.Session.IsFloorOpen(id) ? id : FloorId.Lobby, null);
            StartCoroutine(Run(Arrive()));
        }

        IEnumerator Arrive()
        {
            if (!State.Has(StoryFlags.Opening))
            {
                foreach (var line in Cast.Invitation)
                    yield return screen.Dialogue.Say("INVITATION", "", null, line);
                State.Set(StoryFlags.Opening);
                story.Save();
            }
            var run = GameContext.ChallengeResult;
            GameContext.ChallengeResult = null;
            if (run != null)
            {
                actors.TurnToPlayer(run.Spec.Master);
                yield return conversation.Conclude(run);
            }
            else
            {
                screen.Toast.Show("移動: 矢印キー・WASD・クリック　　話す・調べる: Space　　メニュー: Esc");
            }
        }

        // ------------------------------------------------------------------
        // 入力
        // ------------------------------------------------------------------

        void Update()
        {
            if (busy || Dialog.IsAnyOpen || actors == null)
                return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OpenMenu();
                return;
            }
            if (Time.unscaledTime < nextStepTime)
                return;
            if (AnyKeyDown(InteractKeys))
            {
                ClearRoute();
                Interact(actors.FacingTile);
                return;
            }
            int facing = HeldDirection();
            if (facing >= 0)
            {
                ClearRoute();
                Step(facing);
            }
            else if (route.Count > 0)
            {
                FollowRoute();
            }
            else if (routeTarget.HasValue)
            {
                var target = routeTarget.Value;
                routeTarget = null;
                actors.Face(target);
                Interact(target);
            }
        }

        static bool AnyKeyDown(KeyCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }
            return false;
        }

        static int HeldDirection()
        {
            foreach (var (key, facing) in MoveKeys)
            {
                if (Input.GetKey(key))
                    return facing;
            }
            return -1;
        }

        void OnMapClicked(Vector2 local)
        {
            if (busy || Dialog.IsAnyOpen)
                return;
            var tile = view.TileAt(local);
            if (!floor.Map.InBounds(tile.x, tile.y))
                return;
            var path = Pathfinder.Find(floor.Map, actors.PlayerTile, tile);
            if (path == null)
                return;
            ClearRoute();
            foreach (var cell in path)
                route.Enqueue(cell);
            if (actors.PersonAt(tile) != null || TileMap.IsInteractive(floor.Map[tile.x, tile.y]))
                routeTarget = tile;
        }

        void FollowRoute()
        {
            var next = route.Dequeue();
            var at = actors.PlayerTile;
            if (!Pathfinder.IsAdjacent(at, next) || !Step(Directions.Of(next.x - at.x, next.y - at.y)))
                ClearRoute();
        }

        void ClearRoute()
        {
            route.Clear();
            routeTarget = null;
        }

        bool Step(int facing)
        {
            bool moved = actors.TryStep(facing);
            nextStepTime = Time.unscaledTime + (moved ? FloorActors.StepSeconds : 0.08f);
            return moved;
        }

        // ------------------------------------------------------------------
        // 話しかける・調べる
        // ------------------------------------------------------------------

        void Interact((int x, int y) tile)
        {
            string person = actors.PersonAt(tile);
            if (person != null)
            {
                actors.TurnToPlayer(person);
                StartCoroutine(Run(conversation.Talk(person)));
                return;
            }
            switch (floor.Map[tile.x, tile.y])
            {
                case Tile.Table:
                    StartCoroutine(Run(OfferSeat()));
                    break;
                case Tile.Elevator:
                    Sfx.Play(SfxKind.Button);
                    ElevatorPanel.Open(screen.Root, story.Session, floor.Id, GoToFloor);
                    break;
                case Tile.Cashier:
                    Sfx.Play(SfxKind.Button);
                    CashierDesk.Open(screen.Root, story, profiles, TableMinimum, Refresh);
                    break;
            }
        }

        IEnumerator OfferSeat()
        {
            var game = GameCatalog.Of(floor.Game);
            int choice = -1;
            yield return screen.Dialogue.Choose(game.Title, game.Genre, null, $"{game.Title} のテーブルです。\n{game.Pitch}", new[] { "座る", "やめる" }, c => choice = c);
            if (choice == 0)
                Sit(floor.Game);
        }

        void OpenMenu()
        {
            if (busy)
                return;
            Sfx.Play(SfxKind.Button);
            MemberMenu.Open(screen.Root, story, profiles, Refresh);
        }

        /// <summary>会話などを進める間は移動やほかの話しかけを止め、終わったら画面を最新にする。</summary>
        IEnumerator Run(IEnumerator flow)
        {
            busy = true;
            ClearRoute();
            yield return flow;
            screen.Dialogue.Close();
            busy = false;
            Refresh();
        }

        void Refresh()
        {
            screen.Hud.Refresh(State, profiles.Profile.balance);
            foreach (var id in actors.People)
                actors.SetMark(id, conversation.MarkFor(id));
        }

        // ------------------------------------------------------------------
        // フロアの移動とシーンの切り替え
        // ------------------------------------------------------------------

        /// <summary>フロアを作り直す。arrival が null なら保存した位置（使えなければ出発地点）に立つ。</summary>
        void ShowFloor(FloorId id, (int x, int y)? arrival)
        {
            if (view != null)
                Destroy(view.Root.gameObject);
            ClearRoute();
            floor = Floors.Of(id);
            view = FloorView.Build(screen.Stage, floor);
            var hit = Ui.Image(view.Root, "ClickArea", null, Color.clear, Vector2.zero, view.Root.sizeDelta, true);
            hit.transform.SetAsFirstSibling();
            hit.gameObject.AddComponent<MapClickArea>().Clicked += OnMapClicked;

            var start = arrival ?? SavedTile();
            actors = new FloorActors(view, floor, start, arrival.HasValue ? Directions.Down : State.facing);
            screen.Hud.SetFloor(id);
            Refresh();
        }

        (int x, int y) SavedTile()
        {
            var map = floor.Map;
            return map.IsFree(State.x, State.y) ? (State.x, State.y) : map.Spawn;
        }

        void GoToFloor(FloorId id)
        {
            Sfx.Play(SfxKind.Chip);
            ShowFloor(id, ElevatorExit(Floors.Of(id).Map));
            SavePosition();
            screen.Toast.Show($"{StoryNames.Floor(id)}  ·  {StoryNames.FloorJapanese(id)}", Theme.GoldBright);
        }

        /// <summary>エレベーターを降りたところ（エレベーターの隣の空いたマス）。</summary>
        static (int x, int y) ElevatorExit(TileMap map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (map[x, y] != Tile.Elevator)
                        continue;
                    foreach (var (dx, dy) in Directions.Steps)
                    {
                        if (map.IsFree(x + dx, y + dy))
                            return (x + dx, y + dy);
                    }
                }
            }
            return map.Spawn;
        }

        void StartChallenge(ChallengeSpec spec)
        {
            GameContext.Challenge = story.Session.StartChallenge(spec);
            Sit(spec.Game);
        }

        void Sit(GameId game)
        {
            GameContext.ReturnScene = SceneNames.World;
            if (game == GameId.Vector)
                GameContext.SelectedMode = GameMode.Player;
            Leave(GameCatalog.Of(game).Scene);
        }

        void Leave(string scene)
        {
            SavePosition();
            SceneManager.LoadScene(scene);
        }

        void SavePosition()
        {
            if (actors == null)
                return;
            State.floor = (int)floor.Id;
            (State.x, State.y) = actors.PlayerTile;
            State.facing = actors.Player.Facing;
            story.Save();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
                SavePosition();
        }

        void OnApplicationQuit() => SavePosition();
    }
}
