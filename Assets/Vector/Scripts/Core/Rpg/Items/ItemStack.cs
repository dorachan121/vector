using System;

namespace Vector.Core.Rpg
{
    /// <summary>持ち物1種類と個数（保存する）。</summary>
    [Serializable]
    public sealed class ItemStack
    {
        public const int MaxCount = 999;

        public string id;
        public int count;
    }
}
