using System;
using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// ログインボーナスとデイリーミッションの進み具合（状態は RpgState に保存する）。今日の日付は外から渡す（テストでは固定する）。
    /// 日付が変わると、ミッションは新しい3つに入れ替わる。
    /// </summary>
    public sealed class DailyProgress
    {
        readonly RpgState state;
        readonly IChipAccount chips;
        readonly Func<int> today;

        public DailyProgress(RpgState state, IChipAccount chips, Func<int> today)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.chips = chips ?? throw new ArgumentNullException(nameof(chips));
            this.today = today ?? throw new ArgumentNullException(nameof(today));
        }

        // ------------------------------------------------------------------
        // ログインボーナス
        // ------------------------------------------------------------------

        /// <summary>今日のログインボーナスをまだ受け取っていない（端末の日付を戻しても、前に受け取った日より後でなければ受け取れない）。</summary>
        public bool LoginAvailable => today() > state.lastLoginDay;

        /// <summary>次に受け取るのは巡の何日目か（0〜6）。</summary>
        public int NextLoginDay => LoginBonus.Day(state.loginCount);

        /// <summary>いまの巡で受け取った日数（0〜7）。今日の分を受け取ったあとは、今日まで数える。</summary>
        public int ClaimedThisCycle
        {
            get
            {
                if (state.loginCount == 0)
                    return 0;
                return LoginAvailable ? LoginBonus.Day(state.loginCount) : LoginBonus.Day(state.loginCount - 1) + 1;
            }
        }

        /// <summary>今日のログインボーナスを受け取る。受け取れなければ null。</summary>
        public RewardGrant ClaimLogin()
        {
            if (!LoginAvailable)
                return null;
            var reward = LoginBonus.RewardFor(state.loginCount);
            state.loginCount++;
            state.lastLoginDay = today();
            return reward.ApplyTo(state, chips);
        }

        // ------------------------------------------------------------------
        // デイリーミッション
        // ------------------------------------------------------------------

        public IReadOnlyList<DailyMissionDef> Missions
        {
            get
            {
                Roll();
                return DailyMissions.For(state.dailyDay);
            }
        }

        public long ProgressOf(int index)
        {
            Roll();
            return Math.Min(state.dailyProgress[index], Missions[index].Target);
        }

        public bool IsDone(int index) => ProgressOf(index) >= Missions[index].Target;

        public bool IsClaimed(int index)
        {
            Roll();
            return state.dailyClaimed[index];
        }

        public bool BonusClaimed
        {
            get
            {
                Roll();
                return state.dailyBonusClaimed;
            }
        }

        /// <summary>3つすべての報酬を受け取った。</summary>
        public bool AllClaimed
        {
            get
            {
                for (int i = 0; i < DailyMissions.Count; i++)
                {
                    if (!IsClaimed(i))
                        return false;
                }
                return true;
            }
        }

        /// <summary>受け取れる報酬の数（バッジの表示用）。達成ボーナスも1つと数える。</summary>
        public int Claimable
        {
            get
            {
                int n = 0;
                for (int i = 0; i < DailyMissions.Count; i++)
                {
                    if (IsDone(i) && !IsClaimed(i))
                        n++;
                }
                if (AllClaimed && !BonusClaimed)
                    n++;
                return n;
            }
        }

        /// <summary>達成したミッションの報酬を受け取る。受け取れなければ null。</summary>
        public RewardGrant Claim(int index)
        {
            if (index < 0 || index >= DailyMissions.Count || !IsDone(index) || IsClaimed(index))
                return null;
            state.dailyClaimed[index] = true;
            var mission = Missions[index];
            return new RewardGrant { Xp = mission.Xp, Comps = mission.Comps, Chips = mission.Chips }.ApplyTo(state, chips);
        }

        /// <summary>3つすべて受け取ったあとの達成ボーナスを受け取る。受け取れなければ null。</summary>
        public RewardGrant ClaimBonus()
        {
            if (!AllClaimed || BonusClaimed)
                return null;
            state.dailyBonusClaimed = true;
            return DailyMissions.AllClearBonus.ApplyTo(state, chips);
        }

        /// <summary>精算したラウンドでミッションを進める。このラウンドで新しく達成したミッションを返す。</summary>
        internal List<DailyMissionDef> OnRound(RoundEvent round)
        {
            Roll();
            if (!state.dailyGames.Contains((int)round.Game))
                state.dailyGames.Add((int)round.Game);
            var completed = new List<DailyMissionDef>();
            var missions = Missions;
            for (int i = 0; i < missions.Count; i++)
            {
                var m = missions[i];
                bool wasDone = state.dailyProgress[i] >= m.Target;
                state.dailyProgress[i] = Math.Min(m.Target, Advance(m, state.dailyProgress[i], round));
                if (!wasDone && state.dailyProgress[i] >= m.Target)
                    completed.Add(m);
            }
            return completed;
        }

        long Advance(DailyMissionDef m, long current, RoundEvent round)
        {
            switch (m.Kind)
            {
                case DailyMissionKind.PlayRounds: return current + 1;
                case DailyMissionKind.WinRounds: return round.Won ? current + 1 : current;
                case DailyMissionKind.Wager: return current + round.Stake;
                case DailyMissionKind.PlayGame: return round.Game == m.Game ? current + 1 : current;
                default: return state.dailyGames.Count;
            }
        }

        /// <summary>日付が変わっていたら、今日のミッションに入れ替える。</summary>
        void Roll()
        {
            int day = today();
            if (state.dailyDay == day && state.dailyProgress.Count == DailyMissions.Count && state.dailyClaimed.Count == DailyMissions.Count)
                return;
            state.dailyDay = day;
            state.dailyProgress = new List<long>(new long[DailyMissions.Count]);
            state.dailyClaimed = new List<bool>(new bool[DailyMissions.Count]);
            state.dailyBonusClaimed = false;
            state.dailyGames = new List<int>();
        }
    }
}
