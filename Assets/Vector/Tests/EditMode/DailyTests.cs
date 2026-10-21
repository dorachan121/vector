using System.Collections.Generic;
using NUnit.Framework;
using Vector.Core;
using Vector.Core.Rpg;

namespace Vector.Tests
{
    public class DailyTests
    {
        int day = 100;

        RpgSession NewSession(out Bankroll chips, RpgState state = null)
        {
            chips = new Bankroll(1_000);
            return new RpgSession(state ?? new RpgState(), chips, () => day);
        }

        static RoundEvent Round(GameId game, long stake, long ret) => new RoundEvent(game, stake, ret, null);

        [Test]
        public void LoginBonus_OncePerDayAndAdvancesWithoutResetting()
        {
            var session = NewSession(out var chips);
            var daily = session.Daily;
            Assert.IsTrue(daily.LoginAvailable);
            var first = daily.ClaimLogin();
            Assert.AreEqual(LoginBonus.RewardFor(0).Chips, first.Chips);
            Assert.AreEqual(1_000 + first.Chips, chips.Balance);
            Assert.IsFalse(daily.LoginAvailable);
            Assert.IsNull(daily.ClaimLogin(), "同じ日は1回だけ");

            day += 5;
            Assert.AreEqual(1, daily.NextLoginDay, "休んでも次の日のボーナスから続く");
            Assert.IsNotNull(daily.ClaimLogin());
            day -= 3;
            Assert.IsFalse(daily.LoginAvailable, "日付を戻しても受け取れない");
        }

        [Test]
        public void LoginBonus_CyclesEverySevenDays()
        {
            var session = NewSession(out _);
            for (int i = 0; i < LoginBonus.CycleLength; i++)
            {
                Assert.AreEqual(i, session.Daily.NextLoginDay);
                Assert.AreEqual(i, session.Daily.ClaimedThisCycle);
                Assert.IsNotNull(session.Daily.ClaimLogin());
                Assert.AreEqual(i + 1, session.Daily.ClaimedThisCycle, "受け取った日はその日まで数える");
                day++;
            }
            Assert.AreEqual(0, session.Daily.NextLoginDay);
            Assert.AreEqual(0, session.Daily.ClaimedThisCycle, "次の巡は0日から");
            Assert.AreEqual(1, session.State.Count(ItemCatalog.LuckyCoin), "7日目は記念のコイン付き");
        }

        [Test]
        public void Missions_AreStablePerDayAndVaryOverDays()
        {
            var a = DailyMissions.For(100);
            var b = DailyMissions.For(100);
            Assert.AreEqual(DailyMissions.Count, a.Length);
            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i].Kind, b[i].Kind);
                Assert.AreEqual(a[i].Target, b[i].Target);
            }
            var kinds = new HashSet<(DailyMissionKind, long)>();
            for (int d = 0; d < 60; d++)
            {
                foreach (var m in DailyMissions.For(d))
                    kinds.Add((m.Kind, m.Target));
            }
            Assert.Greater(kinds.Count, 6, "日によっていろいろなミッションが出る");
        }

        [Test]
        public void Missions_ProgressByRoundsAndPayOnceEach()
        {
            var session = NewSession(out var chips);
            var daily = session.Daily;
            var missions = daily.Missions;
            for (int r = 0; r < 30; r++)
                session.OnRound(Round((GameId)(r % 6), 500, r % 2 == 0 ? 1_000 : 0));
            for (int i = 0; i < missions.Count; i++)
                Assert.IsTrue(daily.IsDone(i), $"{missions[i].Kind} {missions[i].Target}");
            Assert.AreEqual(3, daily.Claimable);
            Assert.IsNull(daily.ClaimBonus(), "達成ボーナスは3つ受け取ってから");
            var total = new RewardGrant();
            long comps = session.State.comps;
            for (int i = 0; i < missions.Count; i++)
            {
                var grant = daily.Claim(i);
                Assert.IsNotNull(grant);
                total = total.Plus(grant);
                Assert.IsNull(daily.Claim(i), "二重に受け取れない");
            }
            Assert.AreEqual(comps + total.Comps, session.State.comps, "まとめた報酬は受け取った合計と一致する");
            Assert.AreEqual(1, daily.Claimable);
            long before = chips.Balance;
            Assert.IsNotNull(daily.ClaimBonus());
            Assert.AreEqual(before + DailyMissions.AllClearBonus.Chips, chips.Balance);
            Assert.AreEqual(0, daily.Claimable);
        }

        [Test]
        public void Missions_ResetWhenTheDayChanges()
        {
            var session = NewSession(out _);
            session.OnRound(Round(GameId.Vector, 100, 0));
            Assert.AreEqual(1, session.Daily.ProgressOf(0));
            day++;
            Assert.AreEqual(0, session.Daily.ProgressOf(0));
            Assert.IsFalse(session.Daily.IsClaimed(0));
        }

        [Test]
        public void Report_ListsMissionsCompletedThisRound()
        {
            var session = NewSession(out _);
            long target = session.Daily.Missions[0].Target;
            for (int r = 0; r < target - 1; r++)
                Assert.IsFalse(session.OnRound(Round(GameId.Crown, 10, 0)).CompletedDailies.Exists(m => m.Kind == DailyMissionKind.PlayRounds));
            var report = session.OnRound(Round(GameId.Crown, 10, 0));
            Assert.IsTrue(report.CompletedDailies.Exists(m => m.Kind == DailyMissionKind.PlayRounds));
        }

        [Test]
        public void Sanitize_RepairsDailyState()
        {
            var state = new RpgState { loginCount = -4, lastLoginDay = -9, dailyDay = 7, dailyProgress = new List<long> { 5 }, dailyGames = new List<int> { 1, 1, 99 } };
            state.Sanitize();
            Assert.AreEqual(0, state.loginCount);
            Assert.AreEqual(-1, state.lastLoginDay);
            Assert.AreEqual(-1, state.dailyDay, "数の合わない進み具合は作り直す");
            CollectionAssert.AreEqual(new[] { 1 }, state.dailyGames);
        }
    }
}
