using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// 端末の画面の向きを、画面の形（縦・横）に合わせて固定する。自動回転は、その形の向きだけを許す。
    /// PC では向きの指定は効かず、ウィンドウの縦横比のまま表示する。
    /// </summary>
    public static class OrientationLock
    {
        /// <summary>いま固定している形。</summary>
        public static ScreenShape Current { get; private set; } = ScreenShape.Portrait;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Current = ScreenShape.Portrait;

        public static void Apply(ScreenShape shape)
        {
            Current = shape;
            bool portrait = shape == ScreenShape.Portrait;
            Screen.autorotateToPortrait = portrait;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = !portrait;
            Screen.autorotateToLandscapeRight = !portrait;
            // いまの向きが許されていなければ、端末を持っている向きに近いほうへ回してから自動回転に戻す
            Screen.orientation = portrait ? ScreenOrientation.Portrait
                : Input.deviceOrientation == DeviceOrientation.LandscapeRight ? ScreenOrientation.LandscapeRight : ScreenOrientation.LandscapeLeft;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        /// <summary>
        /// 端末をその形の向きに持っているか。手に持つ端末でなければ、また向きが分からない（平置きなど）ときは持っているとみなす。
        /// </summary>
        public static bool IsHeld(ScreenShape shape)
        {
            if (SystemInfo.deviceType != DeviceType.Handheld)
                return true;
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Portrait:
                case DeviceOrientation.PortraitUpsideDown:
                    return shape == ScreenShape.Portrait;
                case DeviceOrientation.LandscapeLeft:
                case DeviceOrientation.LandscapeRight:
                    return shape == ScreenShape.Landscape;
                default:
                    return true;
            }
        }
    }
}
