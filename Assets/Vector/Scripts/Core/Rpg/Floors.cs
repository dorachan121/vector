using System.Collections.Generic;
using Vector.Core.Colosseum;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// 全フロアの定義（見取り図・ゲーム・人・主の挑戦）。見取り図は 22×12 マスで、画面にちょうど収まる。
    /// 挑戦の数値は Tools/Simulator の challenge コマンドで、勝てる確率がほどよくなるように決めた。
    /// </summary>
    public static class Floors
    {
        static readonly FloorDef[] floors =
        {
            new FloorDef
            {
                Id = FloorId.Lobby,
                Game = GameId.Vector,
                UnlockedBy = null,
                People = new Dictionary<char, string> { { 'a', "lena" }, { 'b', "mabel" }, { 'c', "gen" }, { 'e', "jin" }, { 'M', "kai" } },
                Map = new TileMap(new[]
                {
                    "######################",
                    "#E...P....,,,,....P.C#",
                    "#.a..................#",
                    "#.TTT...TTT...TTT..b.#",
                    "#.TTT...TTT...TTT....#",
                    "#......c.............#",
                    "#O........,,M,......O#",
                    "#.....====,,,,====...#",
                    "#.BBBB....,,,,.......#",
                    "#..e......====.......#",
                    "#P.........@........P#",
                    "######################",
                }),
                Master = new ChallengeSpec
                {
                    Master = "kai", Floor = FloorId.Lobby, Game = GameId.Vector, RequiredLevel = 2,
                    EntryFee = 200, Stack = 1000, Target = 1500, Rounds = 8, Prize = 1500, XpReward = 200,
                    Trophy = ItemCatalog.TrophyLobby,
                },
            },
            new FloorDef
            {
                Id = FloorId.DiceParlor,
                Game = GameId.Crown,
                UnlockedBy = "kai",
                People = new Dictionary<char, string> { { 'a', "pip" }, { 'b', "sora" }, { 'c', "ume" }, { 'M', "reina" } },
                Map = new TileMap(new[]
                {
                    "######################",
                    "#E..P.......,,....P.C#",
                    "#....................#",
                    "#..TT....TT....TT....#",
                    "#..TT....TT....TT..a.#",
                    "#....................#",
                    "#..b.......M.........#",
                    "#.....,,,,,,,,,,.....#",
                    "#..TT....TT....TT....#",
                    "#..TT....TT....TT.c..#",
                    "#P.........@........P#",
                    "######################",
                }),
                Master = new ChallengeSpec
                {
                    Master = "reina", Floor = FloorId.DiceParlor, Game = GameId.Crown, RequiredLevel = 3,
                    EntryFee = 300, Stack = 1000, Target = 1600, Rounds = 8, Prize = 2500, XpReward = 300,
                    Trophy = ItemCatalog.TrophyDice,
                },
            },
            new FloorDef
            {
                Id = FloorId.Arena,
                Game = GameId.Colosseum,
                UnlockedBy = "reina",
                People = new Dictionary<char, string> { { 'a', "ryo" }, { 'b', "fen" }, { 'c', "nana" }, { 'M', "gald" } },
                Map = new TileMap(new[]
                {
                    "######################",
                    "#E..O.....,,.....O..C#",
                    "#....................#",
                    "#..TTTT........TTTT..#",
                    "#..TTTT...a....TTTT..#",
                    "#O.......,,,,.......O#",
                    "#....b...,M,,....c...#",
                    "#........,,,,........#",
                    "#O..................O#",
                    "#.....~~~~~~~~~~.....#",
                    "#P.........@........P#",
                    "######################",
                }),
                Master = new ChallengeSpec
                {
                    Master = "gald", Floor = FloorId.Arena, Game = GameId.Colosseum, RequiredLevel = 5,
                    EntryFee = 400, Stack = 1000, Target = 1700, Rounds = 6, Prize = 3500, XpReward = 400,
                    Trophy = ItemCatalog.TrophyArena,
                },
            },
            new FloorDef
            {
                Id = FloorId.BeastTrack,
                Game = GameId.Derby,
                UnlockedBy = "gald",
                People = new Dictionary<char, string> { { 'a', "dash" }, { 'b', "hiro" }, { 'c', "momo" }, { 'M', "toki" } },
                Map = new TileMap(new[]
                {
                    "######################",
                    "#E.TTTT..TTTT..TTTT.C#",
                    "#....................#",
                    "#.a.......M........b.#",
                    "#....................#",
                    "#.~~~~~~~~~~~~~~~~~~.#",
                    "#.~~~~~~~~~~~~~~~~~~.#",
                    "#....................#",
                    "#..c.....,,,,........#",
                    "#....P...,,,,...P....#",
                    "#P.........@........P#",
                    "######################",
                }),
                Master = new ChallengeSpec
                {
                    Master = "toki", Floor = FloorId.BeastTrack, Game = GameId.Derby, RequiredLevel = 7,
                    EntryFee = 500, Stack = 1000, Target = 1800, Rounds = 6, Prize = 5000, XpReward = 500,
                    Trophy = ItemCatalog.TrophyTrack,
                },
            },
            new FloorDef
            {
                Id = FloorId.AlchemyHall,
                Game = GameId.Alchemia,
                UnlockedBy = "toki",
                People = new Dictionary<char, string> { { 'a', "nico" }, { 'b', "vivi" }, { 'c', "rue" }, { 'M', "mira" } },
                Map = new TileMap(new[]
                {
                    "######################",
                    "#E.T.T.T.T..T.T.T.T.C#",
                    "#....................#",
                    "#.a..................#",
                    "#.T.T.T.....T.T.T..b.#",
                    "#....................#",
                    "#..........M.........#",
                    "#.T.T.T..~~~~..T.T.T.#",
                    "#........~~~~........#",
                    "#..c.................#",
                    "#P.........@........P#",
                    "######################",
                }),
                Master = new ChallengeSpec
                {
                    Master = "mira", Floor = FloorId.AlchemyHall, Game = GameId.Alchemia, RequiredLevel = 9,
                    EntryFee = 600, Stack = 1000, Target = 1500, Rounds = 50, Prize = 7000, XpReward = 600,
                    Trophy = ItemCatalog.TrophyAlchemy,
                },
            },
            new FloorDef
            {
                Id = FloorId.Undervault,
                Game = GameId.Delve,
                UnlockedBy = "mira",
                People = new Dictionary<char, string> { { 'a', "kit" }, { 'b', "rook" }, { 'c', "ivy" }, { 'M', "moss" } },
                Map = new TileMap(new[]
                {
                    "######################",
                    "#E....O.......O.....C#",
                    "#....................#",
                    "#.TT....a......TT....#",
                    "#.TT...........TT..b.#",
                    "#~~~~~.....~~~~~~~~..#",
                    "#..........M.........#",
                    "#.....O........O.....#",
                    "#..TT..........TT....#",
                    "#..TT...c......TT....#",
                    "#P.........@........P#",
                    "######################",
                }),
                Master = new ChallengeSpec
                {
                    Master = "moss", Floor = FloorId.Undervault, Game = GameId.Delve, RequiredLevel = 11,
                    EntryFee = 800, Stack = 1000, Target = 2500, Rounds = 8, Prize = 9000, XpReward = 700,
                    Trophy = ItemCatalog.TrophyVault,
                },
            },
            new FloorDef
            {
                Id = FloorId.Penthouse,
                Game = GameId.Colosseum,
                UnlockedBy = "moss",
                People = new Dictionary<char, string> { { 'a', "sebas" }, { 'M', "zero" } },
                Map = new TileMap(new[]
                {
                    "######################",
                    "#E.....P......P......#",
                    "#....................#",
                    "#..O....,,,,,,....O..#",
                    "#.......,TTTT,.......#",
                    "#.......,TTTT,..a....#",
                    "#..O....,,M,,,....O..#",
                    "#....................#",
                    "#.......======.......#",
                    "#.......======.......#",
                    "#P.........@........P#",
                    "######################",
                }),
                Master = new ChallengeSpec
                {
                    Master = "zero", Floor = FloorId.Penthouse, Game = GameId.Colosseum, RequiredLevel = 13,
                    EntryFee = 1000, Stack = 1000, Target = 2000, Rounds = 1, Prize = 30000, XpReward = 1500,
                    Trophy = ItemCatalog.BlackCard,
                    Opponent = new BattleSetup(30, 44, 3),
                },
            },
        };

        public static IReadOnlyList<FloorDef> All => floors;

        public static FloorDef Of(FloorId id) => floors[(int)id];

        /// <summary>主の id から、その主のいるフロア。</summary>
        public static FloorDef OfMaster(string master)
        {
            foreach (var floor in floors)
            {
                if (floor.Master.Master == master)
                    return floor;
            }
            return null;
        }
    }
}
