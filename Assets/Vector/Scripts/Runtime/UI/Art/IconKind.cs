namespace Vector.Game.UI
{
    /// <summary>画面で使うアイコン（<see cref="Icons"/> が白で描き、色は Image.color で付ける）。</summary>
    public enum IconKind : byte
    {
        Home,
        Games,
        Story,
        Missions,
        Shop,
        Profile,
        Settings,
        Help,
        Back,
        Plus,
        Close,
        Lock,
        Check,
        Star,
        Crown,
        Trophy,
        Gift,
        Calendar,
        Play,

        /// <summary>回転の矢印（反時計回り。左右反転すると時計回り）。</summary>
        Rotate,
        Coin,
    }
}
