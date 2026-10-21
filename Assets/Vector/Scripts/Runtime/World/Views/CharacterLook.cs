using UnityEngine;

namespace Vector.Game.World
{
    /// <summary>フロアを歩く人の見た目（<see cref="CharacterView"/> で描く）。</summary>
    public struct CharacterLook
    {
        public Color Skin;
        public Color Hair;
        public Color Outfit;
        public Color Accent;

        /// <summary>頭に王冠（女王やオーナー）。</summary>
        public bool Crown;

        /// <summary>フロアの主（足元に金の輪）。</summary>
        public bool Master;

        /// <summary>長い髪（立ち絵で肩まで垂らす）。</summary>
        public bool LongHair;

        /// <summary>瞳の色（未設定なら髪の色を暗くしたもの）。</summary>
        public Color Eyes;
    }
}
