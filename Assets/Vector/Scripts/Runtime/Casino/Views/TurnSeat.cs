using UnityEngine;
using Vector.Game.World;

namespace Vector.Game.Casino
{
    /// <summary>先攻・後攻の演出に出す1人（名前・色・顔）。顔は立ち絵か、獣などの絵。</summary>
    public readonly struct TurnSeat
    {
        public readonly string Name;
        public readonly Color Color;
        public readonly CharacterLook? Look;
        public readonly Sprite Picture;

        public TurnSeat(string name, Color color, CharacterLook look)
        {
            Name = name;
            Color = color;
            Look = look;
            Picture = null;
        }

        public TurnSeat(string name, Color color, Sprite picture)
        {
            Name = name;
            Color = color;
            Look = null;
            Picture = picture;
        }
    }
}
