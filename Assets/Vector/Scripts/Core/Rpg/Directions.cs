namespace Vector.Core.Rpg
{
    /// <summary>向き（0 下, 1 左, 2 右, 3 上）と1歩の移動量。</summary>
    public static class Directions
    {
        public const int Down = 0;
        public const int Left = 1;
        public const int Right = 2;
        public const int Up = 3;

        public static readonly (int dx, int dy)[] Steps = { (0, 1), (-1, 0), (1, 0), (0, -1) };

        public static (int dx, int dy) Step(int facing) => Steps[facing];

        /// <summary>移動量から向き。</summary>
        public static int Of(int dx, int dy) => dy > 0 ? Down : dy < 0 ? Up : dx < 0 ? Left : Right;
    }
}
