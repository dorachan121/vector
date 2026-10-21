using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Alchemia;
using Vector.Core.Colosseum;
using Vector.Core.Crown;
using Vector.Core.Delve;
using Vector.Core.Derby;
using Vector.Core.Hanabi;
using Vector.Core.Harpoon;
using Vector.Core.Orbit;
using Vector.Core.Sugoroku;
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
            var orbit = OrbitRules.Standard;
            var harpoon = HarpoonRules.Standard;
            var sugoroku = SugorokuRules.Standard;
            var hanabi = HanabiRules.Standard;
            return new List<GameEntry>
            {
                new GameEntry
                {
                    Id = GameId.Vector, Title = "VECTOR", Rtp = "96.8%", Genre = "CARD LINES", Scene = SceneNames.Table, Accent = Theme.Gold,
                    Pitch = "3×3 にカードを置き、選んだラインの合計でディーラーと勝負",
                    Features = new[] { "先攻・後攻", "戦略ヒント", "観戦ベット" },
                    Terms = $"{Labels.Money(vector.MinBet)}–{Labels.Money(vector.MaxBet)}  ·  ALL LINES {vector.MainPays}",
                },
                new GameEntry
                {
                    Id = GameId.Crown, Title = "CROWN DICE", Rtp = "96.85%", Genre = "DICE DUEL", Scene = SceneNames.Crown, Accent = Theme.Hex("7FB2FF"),
                    Pitch = "5個のダイスを振り直して、後から振るディーラーに勝つ。王冠の目は 6 より強い",
                    Features = new[] { "先攻・後攻", "最善手ヒント" },
                    Terms = $"{Labels.Money(crown.MinBet)}–{Labels.Money(crown.MaxBet)}  ·  ANTE {crown.WinPays}  ·  CROWN YACHT +{crown.CrownYachtBonus}",
                },
                new GameEntry
                {
                    Id = GameId.Colosseum, Title = "COLOSSEUM", Rtp = "96.71%", Genre = "CARD BATTLE", Scene = SceneNames.Colosseum, Accent = Theme.DealerRed,
                    Pitch = "獣の次の一撃を見て、斬るか受けるか。体力を削り合う1対1の決闘",
                    Features = new[] { "先攻・後攻", "最善手ヒント" },
                    Terms = $"{Labels.Money(battle.MinBet)}–{Labels.Money(battle.MaxBet)}  ·  WIN {battle.WinPays}  ·  HP {battle.Setup.PlayerHp} vs {battle.Setup.BeastHp}",
                },
                new GameEntry
                {
                    Id = GameId.Derby, Title = "DERBY", Rtp = "≤97%", Genre = "CARD RACING", Scene = SceneNames.Derby, Accent = Theme.TieGreen,
                    Pitch = "トランプで走る四神のレース。厳密な確率から出した固定オッズで単勝・馬単",
                    Features = new[] { "固定オッズ", "ライブ確率" },
                    Terms = $"WIN {Labels.Money(derby.WinMinBet)}–{Labels.Money(derby.WinMaxBet)}  ·  EXACTA {Labels.Money(derby.ExactaMinBet)}–{Labels.Money(derby.ExactaMaxBet)}",
                },
                new GameEntry
                {
                    Id = GameId.Alchemia, Title = "ALCHEMIA", Rtp = "96.9%", Genre = "MERGE SLOT", Scene = SceneNames.Alchemia, Accent = Theme.Hex("C79BFF"),
                    Pitch = "同じ元素が3つで融合して位が上がる。連鎖の果てに賢者の石を",
                    Features = new[] { "連鎖", $"最大 {alchemia.MaxWinMultiple:N0}倍" },
                    Terms = $"BET {Labels.Money(alchemia.MinFor(AlchemiaRules.Spin))}–{Labels.Money(alchemia.MaxFor(AlchemiaRules.Spin))}  ·  MAX WIN {alchemia.MaxWinMultiple:N0}x",
                },
                new GameEntry
                {
                    Id = GameId.Delve, Title = "DELVE", Rtp = "96.75%+", Genre = "DUNGEON DIVE", Scene = SceneNames.Delve, Accent = Theme.Hex("FF9A3C"),
                    Pitch = "扉を選んで迷宮を潜る。どこで引き返しても RTP は同じ、違うのは宝の大きさ",
                    Features = new[] { "引き際", "RTP 一定" },
                    Terms = $"{Labels.Money(delve.MinBet)}–{Labels.Money(delve.MaxBet)}  ·  B{delve.Depths} x{delve.MultiplierHundredths(delve.Depths) / 100.0:N2}",
                },
                new GameEntry
                {
                    Id = GameId.Orbit, Title = "ORBIT", Rtp = "97.3%", Genre = "ORRERY ROULETTE", Scene = SceneNames.Orbit, Accent = Palette.Cyan,
                    Pitch = "3つの惑星が止まる位置と並びに賭ける天球儀のルーレット。直列すれば 287 倍",
                    Features = new[] { "RTP 一定", "直列 287:1" },
                    Terms = $"{Labels.Money(orbit.MinBet)}–{Labels.Money(orbit.MaxFor(OrbitSpots.Half(0)))}  ·  ALIGNMENT {orbit.PaysFor(OrbitSpots.Alignment)}  ·  COMET 1 IN {OrbitRules.CometOneIn}",
                },
                new GameEntry
                {
                    Id = GameId.Harpoon, Title = "HARPOON", Rtp = $"{HarpoonRules.ReturnPercent}%", Genre = "FISH HUNT", Scene = SceneNames.Harpoon, Accent = Theme.Hex("34C6E0"),
                    Pitch = "泳ぐ獲物を選んでモリを撃つ。大物ほど逃げやすく、どれを狙っても RTP は同じ",
                    Features = new[] { "タップで狩る", $"最大 {harpoon.MaxMultiple(Quarry.Jellyfish)}倍" },
                    Terms = $"BET {Labels.Money(harpoon.BetLevels[0])}–{Labels.Money(harpoon.BetLevels[harpoon.BetLevels.Count - 1])} / 1発  ·  LEVIATHAN x{harpoon.MaxMultiple(Quarry.Leviathan)}",
                },
                new GameEntry
                {
                    Id = GameId.Sugoroku, Title = "SUGOROKU", Rtp = "96.49%", Genre = "BOARD JOURNEY", Scene = SceneNames.Sugoroku, Accent = Theme.Hex("E0503A"),
                    Pitch = "2つの目のどちらで進むかを選び、小判を拾いながら上りを目指す和の双六",
                    Features = new[] { "先を読む", "最善手ヒント" },
                    Terms = $"{Labels.Money(sugoroku.MinBet)}–{Labels.Money(sugoroku.MaxBet)}  ·  {sugoroku.Turns} ROLLS  ·  TOP x{sugoroku.PayFor(sugoroku.MaxTier) / 100}",
                },
                new GameEntry
                {
                    Id = GameId.Hanabi, Title = "HANABI", Rtp = $"{HanabiRules.ReturnPercent}%", Genre = "FIREWORK CRASH", Scene = SceneNames.Hanabi, Accent = Theme.Hex("FF6FAE"),
                    Pitch = "昇るほど倍率が上がる打ち上げ花火。弾ける前に「開く」か、もっと待つか",
                    Features = new[] { "引き際", $"最大 {hanabi.MaxHundredths / HanabiRules.One:N0}倍" },
                    Terms = $"{Labels.Money(hanabi.MinBet)}–{Labels.Money(hanabi.MaxBet)}  ·  RTP {HanabiRules.ReturnPercent}% AT ANY CASH OUT",
                },
            };
        }
    }
}
