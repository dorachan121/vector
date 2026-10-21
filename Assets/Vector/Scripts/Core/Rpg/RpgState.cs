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
            if (floor < 0 || floor > (int)FloorId.Penthouse)
                floor = (int)FloorId.Lobby;
            facing = Math.Min(Math.Max(0, facing), 3);
            flags ??= new List<string>();
            flags.RemoveAll(f => string.IsNullOrEmpty(f));
            quests ??= new List<QuestProgress>();
            quests.RemoveAll(q => q == null || QuestBook.Find(q.id) == null);
            foreach (var q in quests)
                q.count = Math.Min(Math.Max(0, q.count), QuestBook.Find(q.id).Target);
            items ??= new List<ItemStack>();
            items.RemoveAll(i => i == null || ItemCatalog.Find(i.id) == null || i.count <= 0);
            foreach (var i in items)
                i.count = Math.Min(i.count, ItemStack.MaxCount);
        }
    }

    [Serializable]
    public sealed class QuestProgress
    {
        public string id;
        public int count;

        /// <summary>条件を満たした。</summary>
        public bool done;

        /// <summary>報酬を受け取った。</summary>
        public bool claimed;
    }

    [Serializable]
    public sealed class ItemStack
    {
        public const int MaxCount = 999;

        public string id;
        public int count;
    }
}
