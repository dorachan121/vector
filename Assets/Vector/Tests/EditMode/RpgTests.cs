using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Rpg;

namespace Vector.Tests
{
    public class RpgTests
    {
        static RpgSession NewSession(long balance, out Bankroll chips, RpgState state = null)
        {
            chips = new Bankroll(balance);
            return new RpgSession(state ?? new RpgState(), chips);
        }

        static RoundEvent Round(GameId game, long stake, long ret, params string[] tags) => new RoundEvent(game, stake, ret, tags);

        [Test]
        public void Progression_LevelsUpAndRanksFollowLevel()
        {
            var state = new RpgState();
            Assert.AreEqual(90, Progression.XpToNext(1));
            Assert.AreEqual(1, Progression.AddXp(state, 100));
            Assert.AreEqual(2, state.level);
            Assert.AreEqual(10, state.xp);
            Assert.AreEqual(3, Progression.AddXp(state, Progression.TotalXpFor(5) - Progression.TotalXpFor(2)));
            Assert.AreEqual(5, state.level);
            Assert.AreEqual(VipRank.Bronze, Progression.RankFor(3));
            Assert.AreEqual(VipRank.Silver, Progression.RankFor(4));
            Assert.AreEqual(VipRank.Black, Progression.RankFor(Progression.MaxLevel));
            state.level = Progression.MaxLevel;
            Progression.AddXp(state, 1_000_000);
            Assert.AreEqual(Progression.MaxLevel, state.level);
            Assert.AreEqual(0, state.xp);
        }

        [Test]
        public void OnRound_GrantsXpAndCompsByStakeOnly()
        {
            var session = NewSession(1_000, out _);
            var win = session.OnRound(Round(GameId.Crown, 500, 1_000));
            var loss = session.OnRound(Round(GameId.Crown, 500, 0));
            Assert.AreEqual(50, win.Xp);
            Assert.AreEqual(win.Xp, loss.Xp, "勝ち負けで経験値は変わらない");
            Assert.AreEqual(5, win.Comps);
            Assert.AreEqual(1, session.OnRound(Round(GameId.Crown, 5, 0)).Xp, "少額でも経験値 1");
            Assert.AreEqual(1_005, session.State.wagered);
        }

        [Test]
        public void Quests_ProgressOnlyAfterAcceptingAndPayOnClaim()
        {
            var session = NewSession(0, out var chips);
            session.OnRound(Round(GameId.Vector, 100, 200));
            Assert.AreEqual("first_win", session.OfferFrom("lena").Id);
            Assert.IsTrue(session.Accept("first_win"));
            Assert.IsNull(session.OfferFrom("lena"));
            Assert.IsFalse(session.State.Quest("first_win").done, "受ける前の勝ちは数えない");
            var report = session.OnRound(Round(GameId.Delve, 100, 300));
            Assert.AreEqual("first_win", report.CompletedQuests.Single().Id);
            Assert.IsTrue(session.Claim("first_win", out _));
            Assert.AreEqual(500, chips.Balance);
            Assert.IsFalse(session.Claim("first_win", out _), "二重に受け取れない");
            Assert.IsTrue(session.State.Has(QuestBook.ClaimedFlag("first_win")));
        }

        [Test]
        public void Quests_EventAndChainedOffers()
        {
            var session = NewSession(0, out _);
            Assert.AreEqual("gen_slash", session.OfferFrom("gen").Id);
            session.Accept("gen_slash");
            Assert.IsNull(session.OfferFrom("gen"), "続きは前のクエストの報酬を受け取ってから");
            session.OnRound(Round(GameId.Vector, 100, 200));
            Assert.IsFalse(session.State.Quest("gen_slash").done);
            session.OnRound(Round(GameId.Vector, 100, 200, RoundTags.VectorSlashWin));
            Assert.IsTrue(session.State.Quest("gen_slash").done);
            session.Claim("gen_slash", out _);
            Assert.AreEqual("gen_bonus", session.OfferFrom("gen").Id);
        }

        [Test]
        public void Quests_LevelQuestCompletesWhenLevelReached()
        {
            var state = new RpgState { level = 5 };
            var session = NewSession(0, out var chips, state);
            session.Accept("jin_level");
            Assert.IsTrue(state.Quest("jin_level").done, "すでに到達していれば受けた時点で達成");
            session.Claim("jin_level", out _);
            Assert.AreEqual(1_000, chips.Balance);
        }

        [Test]
        public void Shop_BuysWithCompsAndRedeemsVouchers()
        {
            var state = new RpgState { comps = 60 };
            var session = NewSession(0, out var chips, state);
            Assert.IsFalse(session.Buy(ItemCatalog.VoucherLarge));
            Assert.IsTrue(session.Buy(ItemCatalog.VoucherSmall));
            Assert.AreEqual(10, state.comps);
            Assert.IsFalse(session.Buy(ItemCatalog.TrophyLobby), "記念品は交換できない");
            Assert.AreEqual(500, session.Redeem(ItemCatalog.VoucherSmall));
            Assert.AreEqual(500, chips.Balance);
            Assert.AreEqual(0, session.Redeem(ItemCatalog.VoucherSmall));
        }

        [Test]
        public void Challenge_RequiresLevelAndFeeAndRewardsFirstVictoryOnce()
        {
            var spec = Floors.Of(FloorId.Lobby).Master;
            var state = new RpgState();
            var session = NewSession(10_000, out var chips, state);
            Assert.AreEqual(ChallengeBlock.Level, session.CheckChallenge(spec));
            Assert.AreEqual(FloorStatus.Open, session.StatusOf(FloorId.Lobby));
            state.level = spec.RequiredLevel;
            Assert.AreEqual(ChallengeBlock.None, session.CheckChallenge(spec));
            Assert.AreEqual(FloorStatus.Ready, session.StatusOf(FloorId.Lobby));
            Assert.IsFalse(session.IsFloorOpen(FloorId.DiceParlor));
            Assert.AreEqual(FloorStatus.Locked, session.StatusOf(FloorId.DiceParlor));

            var run = session.StartChallenge(spec);
            Assert.AreEqual(10_000 - spec.EntryFee, chips.Balance);
            Assert.AreEqual(spec.Stack, run.Bankroll.Balance);
            run.Bankroll.Credit(spec.Target - spec.Stack);
            run.RecordRound(10);
            Assert.AreEqual(ChallengeState.Won, run.State);
            var reward = session.FinishChallenge(run);
            Assert.IsTrue(reward.FirstVictory);
            Assert.AreEqual(10_000 - spec.EntryFee + spec.Prize, chips.Balance, "挑戦用のチップは持ち帰らず、賞金だけが入る");
            Assert.AreEqual(1, state.Count(spec.Trophy));
            Assert.IsTrue(session.IsFloorOpen(FloorId.DiceParlor));
            Assert.AreEqual(FloorStatus.Cleared, session.StatusOf(FloorId.Lobby));

            var again = session.StartChallenge(spec);
            again.Bankroll.Credit(spec.Target);
            again.RecordRound(10);
            var second = session.FinishChallenge(again);
            Assert.IsFalse(second.FirstVictory);
            Assert.AreEqual(0, second.Prize);
            Assert.AreEqual(1, state.Count(spec.Trophy));
        }

        [Test]
        public void Challenge_InterruptedBeforeTheResultRefundsTheEntryFeeOnce()
        {
            var spec = Floors.Of(FloorId.Lobby).Master;
            var state = new RpgState { level = spec.RequiredLevel };
            var session = NewSession(10_000, out var chips, state);

            session.StartChallenge(spec);
            Assert.AreEqual(spec.Master, state.challenge, "決着まで、参加費を払った挑戦を覚えておく");
            Assert.AreEqual(spec.EntryFee, session.RefundInterruptedChallenge());
            Assert.AreEqual(10_000, chips.Balance);
            Assert.IsNull(state.challenge);
            Assert.AreEqual(0, session.RefundInterruptedChallenge(), "二重に返さない");

            var run = session.StartChallenge(spec);
            run.Abandon();
            session.FinishChallenge(run);
            Assert.IsNull(state.challenge);
            Assert.AreEqual(0, session.RefundInterruptedChallenge(), "決着した挑戦の参加費は返さない");
            Assert.AreEqual(10_000 - spec.EntryFee, chips.Balance);
        }

        [Test]
        public void Sanitize_DropsDuplicatesAndUnknownEntries()
        {
            var state = new RpgState
            {
                flags = new List<string> { "a", "a", "", null, "b" },
                quests = new List<QuestProgress>
                {
                    new QuestProgress { id = "first_win", count = 5 },
                    new QuestProgress { id = "first_win", count = 1 },
                    new QuestProgress { id = "no_such_quest" },
                },
                items = new List<ItemStack>
                {
                    new ItemStack { id = ItemCatalog.LuckyCoin, count = 2 },
                    new ItemStack { id = ItemCatalog.LuckyCoin, count = 3 },
                    new ItemStack { id = "no_such_item", count = 1 },
                },
                challenge = "nobody",
            };
            state.Sanitize();
            CollectionAssert.AreEqual(new[] { "a", "b" }, state.flags);
            Assert.AreEqual(1, state.quests.Count);
            Assert.AreEqual(1, state.quests[0].count, "回数は目標で頭打ち");
            Assert.AreEqual(2, state.Count(ItemCatalog.LuckyCoin));
            Assert.AreEqual(1, state.items.Count);
            Assert.IsNull(state.challenge);
        }

        [Test]
        public void ChallengeRun_LosesWhenRoundsRunOutOrBroke()
        {
            var spec = new ChallengeSpec { Master = "test", Stack = 1_000, Target = 2_000, Rounds = 2 };
            var run = new ChallengeRun(spec);
            run.RecordRound(10);
            Assert.AreEqual(ChallengeState.InProgress, run.State);
            Assert.AreEqual(1, run.RoundsLeft);
            run.RecordRound(10);
            Assert.AreEqual(ChallengeState.Lost, run.State);
            Assert.Throws<InvalidOperationException>(() => run.RecordRound(10));

            var broke = new ChallengeRun(spec);
            broke.Bankroll.TryDebit(995);
            broke.RecordRound(10);
            Assert.AreEqual(ChallengeState.Lost, broke.State);

            var left = new ChallengeRun(spec);
            left.Abandon();
            Assert.AreEqual(ChallengeState.Abandoned, left.State);
            Assert.IsFalse(NewSession(0, out _).FinishChallenge(left).Won);
        }

        [Test]
        public void Floors_AreWellFormedAndReachable()
        {
            var floors = Floors.All;
            CollectionAssert.AreEquivalent(Enum.GetValues(typeof(FloorId)), floors.Select(f => f.Id), "どのフロアも1つずつ定義する");
            for (int i = 0; i < floors.Count; i++)
            {
                var floor = floors[i];
                var map = floor.Map;
                Assert.AreSame(floor, Floors.Of(floor.Id));
                Assert.AreEqual(12, map.Width, "縦画面の横幅に収まる");
                Assert.AreEqual(18, map.Height);
                CollectionAssert.AreEquivalent(map.People.Keys, floor.People.Keys, $"{floor.Id}: 見取り図の人と人の一覧が一致する");
                Assert.IsTrue(map.People.ContainsKey('M'), $"{floor.Id}: 主がいる");
                Assert.AreEqual(floor.Master.Master, floor.People['M']);
                Assert.AreEqual(floor.Id, floor.Master.Floor);

                var reachable = Reachable(map);
                bool hasElevator = false;
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1)
                            Assert.AreEqual(Tile.Wall, map[x, y], $"{floor.Id}: 外周は壁 ({x},{y})");
                        var tile = map[x, y];
                        hasElevator |= tile == Tile.Elevator;
                        if (TileMap.IsInteractive(tile))
                        {
                            bool touches = map.GroupAt(x, y).Any(p => Neighbors(p).Any(reachable.Contains));
                            Assert.IsTrue(touches, $"{floor.Id}: {tile} ({x},{y}) に近づける");
                        }
                    }
                }
                Assert.IsTrue(hasElevator, $"{floor.Id}: エレベーターがある");
                foreach (var person in map.People)
                    Assert.IsTrue(Neighbors(person.Value).Any(reachable.Contains), $"{floor.Id}: {floor.People[person.Key]} に話しかけられる");
            }
        }

        [Test]
        public void Floors_UnlockInStoryOrder()
        {
            var floors = Floors.All;
            Assert.IsNull(floors[0].UnlockedBy);
            for (int i = 1; i < floors.Count; i++)
                Assert.AreEqual(floors[i - 1].Master.Master, floors[i].UnlockedBy);
            for (int i = 1; i < floors.Count; i++)
                Assert.That(floors[i].Master.RequiredLevel, Is.GreaterThan(floors[i - 1].Master.RequiredLevel));
        }

        [Test]
        public void Quests_GiversLiveOnFloors()
        {
            var people = new HashSet<string>(Floors.All.SelectMany(f => f.People.Values));
            foreach (var quest in QuestBook.All)
            {
                Assert.IsTrue(people.Contains(quest.Giver), $"{quest.Id} の依頼主 {quest.Giver}");
                if (quest.Item != null)
                    Assert.IsNotNull(ItemCatalog.Find(quest.Item));
                if (quest.Kind == QuestKind.Event)
                    Assert.IsFalse(string.IsNullOrEmpty(quest.Tag));
            }
            Assert.AreEqual(QuestBook.All.Count, QuestBook.All.Select(q => q.Id).Distinct().Count());
        }

        [Test]
        public void Pathfinder_WalksAroundObstaclesAndStopsNextToPeople()
        {
            var map = Floors.Of(FloorId.Lobby).Map;
            var kai = map.People['M'];
            var path = Pathfinder.Find(map, map.Spawn, kai);
            Assert.IsNotNull(path);
            Assert.IsTrue(Pathfinder.IsAdjacent(path[path.Count - 1], kai));
            var previous = map.Spawn;
            foreach (var step in path)
            {
                Assert.IsTrue(map.IsFree(step.x, step.y));
                Assert.IsTrue(Pathfinder.IsAdjacent(previous, step));
                previous = step;
            }
            Assert.IsNull(Pathfinder.Find(map, map.Spawn, (0, 0)), "壁の外へは行けない");
            Assert.AreEqual(0, Pathfinder.Find(map, map.Spawn, map.Spawn).Count);
        }

        [Test]
        public void State_SanitizeRepairsBadData()
        {
            var state = new RpgState
            {
                level = 99, xp = -5, comps = -1, floor = 77, facing = 9, flags = null,
                quests = new List<QuestProgress> { new QuestProgress { id = "nope" }, new QuestProgress { id = "first_win", count = 50 } },
                items = new List<ItemStack> { new ItemStack { id = "bogus", count = 1 }, new ItemStack { id = ItemCatalog.LuckyCoin, count = 5000 } },
            };
            state.Sanitize();
            Assert.AreEqual(Progression.MaxLevel, state.level);
            Assert.AreEqual(0, state.xp);
            Assert.AreEqual(0, state.comps);
            Assert.AreEqual((int)FloorId.Lobby, state.floor);
            Assert.AreEqual(3, state.facing);
            Assert.IsNotNull(state.flags);
            Assert.AreEqual(1, state.quests.Count);
            Assert.AreEqual(1, state.quests[0].count);
            Assert.AreEqual(1, state.items.Count);
            Assert.AreEqual(ItemStack.MaxCount, state.items[0].count);

            var observatory = new RpgState { floor = (int)FloorId.Observatory };
            observatory.Sanitize();
            Assert.AreEqual((int)FloorId.Observatory, observatory.floor, "末尾に足したフロアにいる保存データも、そのまま読める");
        }

        static HashSet<(int x, int y)> Reachable(TileMap map)
        {
            var seen = new HashSet<(int, int)> { map.Spawn };
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue(map.Spawn);
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                foreach (var n in Neighbors(p))
                {
                    if (map.IsFree(n.x, n.y) && seen.Add(n))
                        queue.Enqueue(n);
                }
            }
            return seen;
        }

        static IEnumerable<(int x, int y)> Neighbors((int x, int y) p)
        {
            foreach (var (dx, dy) in Directions.Steps)
                yield return (p.x + dx, p.y + dy);
        }
    }
}
