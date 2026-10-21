using System;
using System.Collections.Generic;
using Vector.Core.Colosseum;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// 全フロアの定義（見取り図・ゲーム・人・主の挑戦）。見取り図は縦画面に合わせた 12×18 マスで、
    /// 横幅は画面にちょうど収まり、縦は主人公に合わせて流れる。
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
                    "############",
                    "#E..P..P..C#",
                    "#..........#",
                    "#.a......b.#",
                    "#.TTT..TTT.#",
                    "#.TTT..TTT.#",
                    "#....,,....#",
                    "#O...,,...O#",
                    "#....,M....#",
                    "#..c.,,....#",
                    "#.TTT,,TTT.#",
                    "#.TTT,,TTT.#",
                    "#....==....#",
                    "#.BBBB==...#",
                    "#..e.==....#",
                    "#P...==...P#",
                    "#....@=....#",
                    "############",
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
                    "############",
                    "#E...P....C#",
                    "#..........#",
                    "#.TT..TT.a.#",
                    "#.TT..TT...#",
                    "#..........#",
                    "#.b.,,,,...#",
                    "#...,M,,...#",
                    "#...,,,,...#",
                    "#.TT..TT...#",
                    "#.TT..TT.c.#",
                    "#..........#",
                    "#P.,,,,,,.P#",
                    "#..........#",
                    "#.TT..TT...#",
                    "#.TT..TT...#",
                    "#....@.....#",
                    "############",
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
                    "############",
                    "#E..O..O..C#",
                    "#..........#",
                    "#.TTTT.....#",
                    "#.TTTT..a..#",
                    "#..........#",
                    "#O..,,,,..O#",
                    "#.b.,M,,.c.#",
                    "#...,,,,...#",
                    "#O........O#",
                    "#.....TTTT.#",
                    "#.....TTTT.#",
                    "#..........#",
                    "#.~~~~~~~~.#",
                    "#..........#",
                    "#P........P#",
                    "#....@.....#",
                    "############",
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
                    "############",
                    "#E.TTTT...C#",
                    "#..TTTT....#",
                    "#.a........#",
                    "#.~~~~~~~~.#",
                    "#.~~~~~~~~.#",
                    "#..........#",
                    "#....M...b.#",
                    "#..........#",
                    "#.~~~~~~~~.#",
                    "#.~~~~~~~~.#",
                    "#..........#",
                    "#.c..,,,,..#",
                    "#.TTTT,,...#",
                    "#.TTTT,,.P.#",
                    "#P...,,....#",
                    "#....@.....#",
                    "############",
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
                    "############",
                    "#E.T.T.T..C#",
                    "#..........#",
                    "#.a........#",
                    "#.T.T.T.T..#",
                    "#..........#",
                    "#........b.#",
                    "#...~~~~...#",
                    "#...~~~~...#",
                    "#....M.....#",
                    "#..........#",
                    "#.T.T..T.T.#",
                    "#..........#",
                    "#.c........#",
                    "#.T.T..T.T.#",
                    "#P........P#",
                    "#....@.....#",
                    "############",
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
                    "############",
                    "#E..O..O..C#",
                    "#..........#",
                    "#.TT...a...#",
                    "#.TT.......#",
                    "#~~~~~..~~~#",
                    "#..........#",
                    "#....M...b.#",
                    "#..........#",
                    "#.O......O.#",
                    "#......TT..#",
                    "#..c...TT..#",
                    "#..........#",
                    "#.TT.~~~~..#",
                    "#.TT.~~~~..#",
                    "#P........P#",
                    "#....@.....#",
                    "############",
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
                Id = FloorId.Aquarium,
                Game = GameId.Harpoon,
                UnlockedBy = "moss",
                People = new Dictionary<char, string> { { 'a', "coral" }, { 'b', "finn" }, { 'c', "pearl" }, { 'M', "nemo" } },
                Map = new TileMap(new[]
                {
                    "############",
                    "#E..~~~~..C#",
                    "#...~~~~...#",
                    "#.a........#",
                    "#.TT....TT.#",
                    "#.TT.b..TT.#",
                    "#..........#",
                    "#~~..,,..~~#",
                    "#~~.,M,,.~~#",
                    "#~~..,,..~~#",
                    "#..........#",
                    "#.TT....TT.#",
                    "#.TT..c.TT.#",
                    "#...~~~~...#",
                    "#P..~~~~..P#",
                    "#..........#",
                    "#....@.....#",
                    "############",
                }),
                Master = new ChallengeSpec
                {
                    Master = "nemo", Floor = FloorId.Aquarium, Game = GameId.Harpoon, RequiredLevel = 12,
                    EntryFee = 850, Stack = 1000, Target = 2500, Rounds = 30, Prize = 10000, XpReward = 750,
                    Trophy = ItemCatalog.TrophyAquarium,
                },
            },
            new FloorDef
            {
                Id = FloorId.Observatory,
                Game = GameId.Orbit,
                UnlockedBy = "nemo",
                People = new Dictionary<char, string> { { 'a', "luna" }, { 'b', "orion" }, { 'c', "tess" }, { 'M', "stella" } },
                Map = new TileMap(new[]
                {
                    "############",
                    "#E..P..P..C#",
                    "#..........#",
                    "#.TT..a.TT.#",
                    "#.TT....TT.#",
                    "#..........#",
                    "#O..,,,,..O#",
                    "#..,,,,,,..#",
                    "#..,,M,,,b.#",
                    "#..,,,,,,..#",
                    "#O..,,,,..O#",
                    "#..........#",
                    "#.TT.~~.TT.#",
                    "#.TT.~~.TT.#",
                    "#....c.....#",
                    "#P........P#",
                    "#....@.....#",
                    "############",
                }),
                Master = new ChallengeSpec
                {
                    Master = "stella", Floor = FloorId.Observatory, Game = GameId.Orbit, RequiredLevel = 13,
                    EntryFee = 900, Stack = 1000, Target = 3000, Rounds = 8, Prize = 12000, XpReward = 800,
                    Trophy = ItemCatalog.TrophyObservatory,
                },
            },
            new FloorDef
            {
                Id = FloorId.PostTown,
                Game = GameId.Sugoroku,
                UnlockedBy = "stella",
                People = new Dictionary<char, string> { { 'a', "tomo" }, { 'b', "hana" }, { 'c', "genji" }, { 'M', "yuki" } },
                Map = new TileMap(new[]
                {
                    "############",
                    "#E..P..P..C#",
                    "#..........#",
                    "#.TT..a.TT.#",
                    "#.TT....TT.#",
                    "#..........#",
                    "#==========#",
                    "#....b.....#",
                    "#.~~~..,,,.#",
                    "#.~~~.,M,,.#",
                    "#.~~~..,,,.#",
                    "#..........#",
                    "#==========#",
                    "#.TT..c.TT.#",
                    "#.TT....TT.#",
                    "#P........P#",
                    "#....@.....#",
                    "############",
                }),
                Master = new ChallengeSpec
                {
                    Master = "yuki", Floor = FloorId.PostTown, Game = GameId.Sugoroku, RequiredLevel = 14,
                    EntryFee = 950, Stack = 1000, Target = 1800, Rounds = 10, Prize = 15000, XpReward = 900,
                    Trophy = ItemCatalog.TrophyPostTown,
                },
            },
            new FloorDef
            {
                Id = FloorId.Rooftop,
                Game = GameId.Hanabi,
                UnlockedBy = "yuki",
                People = new Dictionary<char, string> { { 'a', "taro" }, { 'b', "mio" }, { 'c', "goro" }, { 'M', "kaede" } },
                Map = new TileMap(new[]
                {
                    "############",
                    "#E..P..P..C#",
                    "#..........#",
                    "#.BBB..BBB.#",
                    "#..a.......#",
                    "#..........#",
                    "#.TT.,,.TT.#",
                    "#.TT.,,.TT.#",
                    "#....,,..b.#",
                    "#..,,,,,,..#",
                    "#..,,M,,,..#",
                    "#..,,,,,,..#",
                    "#.TT....TT.#",
                    "#.TT..c.TT.#",
                    "#..........#",
                    "#P..====..P#",
                    "#....@=....#",
                    "############",
                }),
                Master = new ChallengeSpec
                {
                    Master = "kaede", Floor = FloorId.Rooftop, Game = GameId.Hanabi, RequiredLevel = 15,
                    EntryFee = 1000, Stack = 1000, Target = 3000, Rounds = 10, Prize = 18000, XpReward = 1000,
                    Trophy = ItemCatalog.TrophyRooftop,
                },
            },
            new FloorDef
            {
                Id = FloorId.Penthouse,
                Game = GameId.Colosseum,
                UnlockedBy = "kaede",
                People = new Dictionary<char, string> { { 'a', "sebas" }, { 'M', "zero" } },
                Map = new TileMap(new[]
                {
                    "############",
                    "#E..P..P...#",
                    "#..........#",
                    "#.O......O.#",
                    "#...,,,,...#",
                    "#..,TTTT,..#",
                    "#..,TTTT,a.#",
                    "#...,M,,...#",
                    "#..........#",
                    "#.O......O.#",
                    "#..........#",
                    "#...====...#",
                    "#...====...#",
                    "#...====...#",
                    "#..........#",
                    "#P........P#",
                    "#....@.....#",
                    "############",
                }),
                Master = new ChallengeSpec
                {
                    Master = "zero", Floor = FloorId.Penthouse, Game = GameId.Colosseum, RequiredLevel = 16,
                    EntryFee = 1200, Stack = 1000, Target = 2000, Rounds = 1, Prize = 30000, XpReward = 1500,
                    Trophy = ItemCatalog.BlackCard,
                    Opponent = new BattleSetup(30, 44, 3),
                },
            },
        };

        /// <summary>物語の順（1F から、主に勝つたびに次のフロアが開く）。</summary>
        public static IReadOnlyList<FloorDef> All => floors;

        public static FloorDef Of(FloorId id)
        {
            foreach (var floor in floors)
            {
                if (floor.Id == id)
                    return floor;
            }
            throw new ArgumentOutOfRangeException(nameof(id), id, "定義されていないフロアです。");
        }

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
