using System;
using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// ストーリーモードの進み具合（保存する）。セーブデータの直列化（Unity の JsonUtility）に合わせて public フィールドで持つ。
    /// 読み込んだ値は必ず <see cref="Sanitize"/> で範囲内に収める。
    /// </summary>
    [Serializable]
    public sealed class RpgState
    {
        /// <summary>物語の進行の印の数の上限（壊れたデータで膨らまないように）。</summary>
        const int MaxFlags = 512;

        public int level = 1;

        /// <summary>今のレベルで貯めた経験値。</summary>
        public long xp;

        /// <summary>コンプポイント（賭けた額に応じて貯まり、景品と交換できる）。</summary>
        public long comps;

        /// <summary>今いるフロア（<see cref="FloorId"/> の値）と位置・向き。</summary>
        public int floor;
        public int x = -1;
        public int y = -1;
        public int facing;

        /// <summary>物語の進行の印（会った・倒した・聞いた など）。</summary>
        public List<string> flags = new List<string>();

        public List<QuestProgress> quests = new List<QuestProgress>();
        public List<ItemStack> items = new List<ItemStack>();

        /// <summary>ストーリーモードで賭けた総額（成績の表示用）。</summary>
        public long wagered;

        /// <summary>
        /// 参加費を払って決着していない挑戦の主の id（無ければ null）。決着の前にアプリが終わったときは、
        /// 次の起動時に参加費を返す（テーブルの賭け金と同じく、中断した勝負は無効にする）。
        /// </summary>
        public string challenge;

        /// <summary>ログインボーナスを受け取った日数（累計）と、最後に受け取った日（日付の通し番号。まだなら -1）。</summary>
        public int loginCount;
        public int lastLoginDay = -1;

        /// <summary>デイリーミッションの日（日付の通し番号）と、3つの進み具合・受け取り済みか・達成ボーナス・今日遊んだゲーム。</summary>
        public int dailyDay = -1;
        public List<long> dailyProgress = new List<long>();
        public List<bool> dailyClaimed = new List<bool>();
        public bool dailyBonusClaimed;
        public List<int> dailyGames = new List<int>();

        public bool Has(string flag) => flags.Contains(flag);

        public void Set(string flag)
        {
            if (!flags.Contains(flag))
                flags.Add(flag);
        }

        public QuestProgress Quest(string id)
        {
            foreach (var q in quests)
            {
                if (q.id == id)
                    return q;
            }
            return null;
        }

        public int Count(string item)
        {
            foreach (var stack in items)
            {
                if (stack.id == item)
                    return stack.count;
            }
            return 0;
        }

        public void AddItem(string item, int count = 1)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            foreach (var stack in items)
            {
                if (stack.id == item)
                {
                    stack.count = Math.Min(stack.count + count, ItemStack.MaxCount);
                    return;
                }
            }
            items.Add(new ItemStack { id = item, count = Math.Min(count, ItemStack.MaxCount) });
        }

        public bool TryRemoveItem(string item, int count = 1)
        {
            foreach (var stack in items)
            {
                if (stack.id != item || stack.count < count)
                    continue;
                stack.count -= count;
                if (stack.count == 0)
                    items.Remove(stack);
                return true;
            }
            return false;
        }

        public void Sanitize()
        {
            level = Math.Min(Math.Max(1, level), Progression.MaxLevel);
            xp = Math.Min(Math.Max(0, xp), Progression.XpToNext(level));
            comps = Math.Min(Math.Max(0, comps), 1_000_000_000L);
            wagered = Math.Max(0, wagered);
            if (floor < 0 || floor >= Floors.All.Count)
                floor = (int)FloorId.Lobby;
            facing = Math.Min(Math.Max(0, facing), 3);
            flags ??= new List<string>();
            var seenFlags = new HashSet<string>();
            flags.RemoveAll(f => string.IsNullOrEmpty(f) || !seenFlags.Add(f));
            if (flags.Count > MaxFlags)
                flags.RemoveRange(MaxFlags, flags.Count - MaxFlags);
            // 同じクエスト・同じ持ち物が2つあると数え方が狂うので、最初の1つだけ残す
            quests ??= new List<QuestProgress>();
            var seenQuests = new HashSet<string>();
            quests.RemoveAll(q => q == null || QuestBook.Find(q.id) == null || !seenQuests.Add(q.id));
            foreach (var q in quests)
                q.count = Math.Min(Math.Max(0, q.count), QuestBook.Find(q.id).Target);
            items ??= new List<ItemStack>();
            var seenItems = new HashSet<string>();
            items.RemoveAll(i => i == null || ItemCatalog.Find(i.id) == null || i.count <= 0 || !seenItems.Add(i.id));
            foreach (var i in items)
                i.count = Math.Min(i.count, ItemStack.MaxCount);
            if (challenge != null && Floors.OfMaster(challenge) == null)
                challenge = null;
            loginCount = Math.Max(0, loginCount);
            lastLoginDay = Math.Max(-1, lastLoginDay);
            dailyDay = Math.Max(-1, dailyDay);
            dailyProgress ??= new List<long>();
            dailyClaimed ??= new List<bool>();
            dailyGames ??= new List<int>();
            for (int i = 0; i < dailyProgress.Count; i++)
                dailyProgress[i] = Math.Max(0, dailyProgress[i]);
            var seenGames = new HashSet<int>();
            int gameCount = Enum.GetValues(typeof(GameId)).Length;
            dailyGames.RemoveAll(g => g < 0 || g >= gameCount || !seenGames.Add(g));
            // 数が合わなければ今日の進み具合を作り直す（DailyProgress が次に読むとき）
            if (dailyProgress.Count != DailyMissions.Count || dailyClaimed.Count != DailyMissions.Count)
                dailyDay = -1;
        }
    }
}
