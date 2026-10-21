using System;

namespace Vector.Core.Rpg
{
    /// <summary>受けたクエストの進み具合（保存する）。</summary>
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
}
