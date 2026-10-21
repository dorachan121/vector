namespace Vector.Core.Harpoon
{
    /// <summary>
    /// 獲物の種類。番号はベットエリアを兼ねる（1発の賭けは、狙った獲物の番号に置く）。
    /// 大きな獲物ほど配当が高く、そのぶん逃げやすい。
    /// </summary>
    public static class Quarry
    {
        public const int Sardine = 0;
        public const int Clownfish = 1;
        public const int Puffer = 2;
        public const int Turtle = 3;
        public const int Swordfish = 4;
        public const int Shark = 5;
        public const int Whale = 6;
        public const int Leviathan = 7;

        /// <summary>捕まえてから倍率が決まる、金のクラゲ。</summary>
        public const int Jellyfish = 8;

        public const int Count = 9;

        public static bool IsValid(int quarry) => quarry >= 0 && quarry < Count;
    }
}
