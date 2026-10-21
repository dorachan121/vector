using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Alchemia;
using Vector.Core.Colosseum;
using Vector.Core.Crown;
using Vector.Core.Delve;
using Vector.Core.Derby;
using Vector.Game.UI;

namespace Vector.Game
{
    /// <summary>カジノにあるゲームの一覧（ロビーと RPG のフロアで、名前・説明・テーブルのシーンを共有する）。</summary>
    public static class GameCatalog
    {
        static List<GameEntry> entries;

        public static IReadOnlyList<GameEntry> All => entries ??= Build();

        public static GameEntry Of(GameId id)
        {
            foreach (var e in All)
            {
                if (e.Id == id)
                    return e;
            }
            return All[0];
        }

        static List<GameEntry> Build()
        {
            var vector = GameContext.Rules;
            var crown = CrownRules.Standard;
            var battle = BattleRules.Standard;
            var derby = DerbyRules.Standard;
            var alchemia = AlchemiaRules.Standard;
            var delve = DelveRules.Standard;
            return new List<GameEntry>
            {
                new GameEntry
                {
                    Id = GameId.Vector, Title = "VECTOR", Genre = "CARD LINES", Scene = SceneNames.Table, Accent = Theme.Gold,
                    Pitch = "3×3 にカードを置き、選んだラインの合計でディーラーと勝負",
                    Terms = $"{Labels.Money(vector.MinBet)}–{Labels.Money(vector.MaxBet)}  ·  ALL LINES {vector.MainPays}",
                },
                new GameEntry
                {
                    Id = GameId.Crown, Title = "CROWN DICE", Genre = "DICE DUEL", Scene = SceneNames.Crown, Accent = Theme.Hex("7FB2FF"),
                    Pitch = "5個のダイスを振り直して、後から振るディーラーに勝つ。王冠の目は 6 より強い",
                    Terms = $"{Labels.Money(crown.MinBet)}–{Labels.Money(crown.MaxBet)}  ·  ANTE {crown.WinPays}  ·  CROWN YACHT +{crown.CrownYachtBonus}",
                },
                new GameEntry
                {
                    Id = GameId.Colosseum, Title = "COLOSSEUM", Genre = "CARD BATTLE", Scene = SceneNames.Colosseum, Accent = Theme.DealerRed,
                    Pitch = "獣の次の一撃を見て、斬るか受けるか。体力を削り合う1対1の決闘",
                    Terms = $"{Labels.Money(battle.MinBet)}–{Labels.Money(battle.MaxBet)}  ·  WIN {battle.WinPays}  ·  HP {battle.Setup.PlayerHp} vs {battle.Setup.BeastHp}",
                },
                new GameEntry
                {
                    Id = GameId.Derby, Title = "DERBY", Genre = "CARD RACING", Scene = SceneNames.Derby, Accent = Theme.TieGreen,
                    Pitch = "トランプで走る四神のレース。厳密な確率から出した固定オッズで単勝・馬単",
                    Terms = $"WIN {Labels.Money(derby.WinMinBet)}–{Labels.Money(derby.WinMaxBet)}  ·  EXACTA {Labels.Money(derby.ExactaMinBet)}–{Labels.Money(derby.ExactaMaxBet)}",
                },
                new GameEntry
                {
                    Id = GameId.Alchemia, Title = "ALCHEMIA", Genre = "MERGE SLOT", Scene = SceneNames.Alchemia, Accent = Theme.Hex("C79BFF"),
                    Pitch = "同じ元素が3つで融合して位が上がる。連鎖の果てに賢者の石を",
                    Terms = $"BET {Labels.Money(alchemia.MinFor(AlchemiaRules.Spin))}–{Labels.Money(alchemia.MaxFor(AlchemiaRules.Spin))}  ·  MAX WIN {alchemia.MaxWinMultiple:N0}x",
                },
                new GameEntry
                {
                    Id = GameId.Delve, Title = "DELVE", Genre = "DUNGEON DIVE", Scene = SceneNames.Delve, Accent = Theme.Hex("FF9A3C"),
                    Pitch = "扉を選んで迷宮を潜る。どこで引き返しても RTP は同じ、違うのは宝の大きさ",
                    Terms = $"{Labels.Money(delve.MinBet)}–{Labels.Money(delve.MaxBet)}  ·  B{delve.Depths} x{delve.MultiplierHundredths(delve.Depths) / 100.0:N2}",
                },
            };
        }
    }
}
