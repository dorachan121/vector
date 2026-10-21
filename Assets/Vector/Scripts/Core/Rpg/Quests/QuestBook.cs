using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>全クエストの定義。</summary>
    public static class QuestBook
    {
        static readonly QuestDef[] quests =
        {
            new QuestDef { Id = "first_win", Giver = "lena", Kind = QuestKind.WinRounds, Target = 1, Xp = 50, Chips = 500 },
            new QuestDef { Id = "mabel_rounds", Giver = "mabel", Kind = QuestKind.PlayRounds, Game = GameId.Vector, Target = 5, Xp = 60, Comps = 20 },
            new QuestDef { Id = "gen_slash", Giver = "gen", Kind = QuestKind.Event, Tag = RoundTags.VectorSlashWin, Xp = 80, Comps = 20 },
            new QuestDef { Id = "jin_level", Giver = "jin", Kind = QuestKind.ReachLevel, Target = 5, Xp = 0, Chips = 1_000 },
            new QuestDef { Id = "gen_bonus", Giver = "gen", Kind = QuestKind.Event, Tag = RoundTags.VectorBonus, Xp = 150, Comps = 50, RequiresFlag = "quest.gen_slash" },
            new QuestDef { Id = "pip_yacht", Giver = "pip", Kind = QuestKind.Event, Tag = RoundTags.CrownYacht, Xp = 120, Comps = 30 },
            new QuestDef { Id = "sora_wins", Giver = "sora", Kind = QuestKind.WinRounds, Game = GameId.Crown, Target = 3, Xp = 80, Chips = 800 },
            new QuestDef { Id = "ryo_wins", Giver = "ryo", Kind = QuestKind.WinRounds, Game = GameId.Colosseum, Target = 3, Xp = 120, Comps = 30 },
            new QuestDef { Id = "fen_critical", Giver = "fen", Kind = QuestKind.Event, Tag = RoundTags.ColosseumCriticalFinish, Xp = 100, Item = ItemCatalog.LuckyCoin },
            new QuestDef { Id = "dash_exacta", Giver = "dash", Kind = QuestKind.Event, Tag = RoundTags.DerbyExacta, Xp = 120, Chips = 1_000 },
            new QuestDef { Id = "hiro_races", Giver = "hiro", Kind = QuestKind.PlayRounds, Game = GameId.Derby, Target = 5, Xp = 60, Comps = 30 },
            new QuestDef { Id = "nico_relic", Giver = "nico", Kind = QuestKind.Event, Tag = RoundTags.AlchemiaRelic, Xp = 100, Comps = 40 },
            new QuestDef { Id = "vivi_stone", Giver = "vivi", Kind = QuestKind.Event, Tag = RoundTags.AlchemiaStone, Xp = 300, Comps = 100 },
            new QuestDef { Id = "kit_deep", Giver = "kit", Kind = QuestKind.Event, Tag = RoundTags.DelveDeepReturn, Xp = 200, Chips = 2_000 },
            new QuestDef { Id = "rook_dives", Giver = "rook", Kind = QuestKind.PlayRounds, Game = GameId.Delve, Target = 5, Xp = 60, Comps = 30 },
            new QuestDef { Id = "coral_shots", Giver = "coral", Kind = QuestKind.PlayRounds, Game = GameId.Harpoon, Target = 10, Xp = 60, Comps = 30 },
            new QuestDef { Id = "finn_big_catch", Giver = "finn", Kind = QuestKind.Event, Tag = RoundTags.HarpoonBigCatch, Xp = 200, Chips = 1_500 },
            new QuestDef { Id = "pearl_jelly", Giver = "pearl", Kind = QuestKind.Event, Tag = RoundTags.HarpoonJelly, Xp = 120, Comps = 60 },
            new QuestDef { Id = "luna_spins", Giver = "luna", Kind = QuestKind.PlayRounds, Game = GameId.Orbit, Target = 5, Xp = 60, Comps = 30 },
            new QuestDef { Id = "orion_conjunction", Giver = "orion", Kind = QuestKind.Event, Tag = RoundTags.OrbitConjunction, Xp = 120, Chips = 1_000 },
            new QuestDef { Id = "tess_comet", Giver = "tess", Kind = QuestKind.Event, Tag = RoundTags.OrbitComet, Xp = 200, Comps = 80 },
            new QuestDef { Id = "tomo_journeys", Giver = "tomo", Kind = QuestKind.PlayRounds, Game = GameId.Sugoroku, Target = 3, Xp = 60, Comps = 30 },
            new QuestDef { Id = "hana_treasure", Giver = "hana", Kind = QuestKind.Event, Tag = RoundTags.SugorokuTreasure, Xp = 100, Comps = 50 },
            new QuestDef { Id = "genji_high", Giver = "genji", Kind = QuestKind.Event, Tag = RoundTags.SugorokuHighTier, Xp = 200, Chips = 2_000 },
            new QuestDef { Id = "mio_bloom", Giver = "mio", Kind = QuestKind.Event, Tag = RoundTags.HanabiBloom, Xp = 80, Comps = 40 },
            new QuestDef { Id = "goro_grand", Giver = "goro", Kind = QuestKind.Event, Tag = RoundTags.HanabiGrand, Xp = 200, Chips = 2_000 },
            new QuestDef { Id = "taro_close", Giver = "taro", Kind = QuestKind.Event, Tag = RoundTags.HanabiClose, Xp = 150, Item = ItemCatalog.LuckyCoin },
        };

        public static IReadOnlyList<QuestDef> All => quests;

        public static QuestDef Find(string id)
        {
            foreach (var q in quests)
            {
                if (q.Id == id)
                    return q;
            }
            return null;
        }

        /// <summary>その人が依頼するクエスト（受けられる順）。</summary>
        public static IEnumerable<QuestDef> From(string giver)
        {
            foreach (var q in quests)
            {
                if (q.Giver == giver)
                    yield return q;
            }
        }

        /// <summary>報酬を受け取ったときに立てる印（続きのクエストの条件に使う）。</summary>
        public static string ClaimedFlag(string id) => "quest." + id;
    }
}
