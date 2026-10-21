using UnityEngine;
using Vector.Core.Harpoon;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// 獲物ごとの見た目の大きさと泳ぎ方・現れやすさ。画面の演出だけに使い、捕まえる確率には関係しない。
    /// </summary>
    public readonly struct SwimSpec
    {
        public readonly Vector2 Size;
        public readonly float MinSpeed;
        public readonly float MaxSpeed;

        /// <summary>現れやすさの重み（リヴァイアサンは別の間隔で現れる）。</summary>
        public readonly float Weight;

        /// <summary>群れの大きさの上限（1 なら単独）。</summary>
        public readonly int School;

        SwimSpec(float width, float height, float minSpeed, float maxSpeed, float weight, int school = 1)
        {
            Size = new Vector2(width, height);
            MinSpeed = minSpeed;
            MaxSpeed = maxSpeed;
            Weight = weight;
            School = school;
        }

        public static SwimSpec Of(int quarry)
        {
            switch (quarry)
            {
                case Quarry.Sardine: return new SwimSpec(100, 40, 170, 230, 24, 5);
                case Quarry.Clownfish: return new SwimSpec(104, 68, 110, 150, 18, 2);
                case Quarry.Puffer: return new SwimSpec(100, 90, 60, 90, 13);
                case Quarry.Turtle: return new SwimSpec(156, 128, 70, 95, 11);
                case Quarry.Swordfish: return new SwimSpec(270, 90, 230, 290, 9);
                case Quarry.Shark: return new SwimSpec(300, 134, 130, 170, 7);
                case Quarry.Whale: return new SwimSpec(430, 215, 55, 70, 3.5f);
                case Quarry.Leviathan: return new SwimSpec(560, 233, 80, 100, 0);
                default: return new SwimSpec(96, 132, 35, 50, 6);
            }
        }
    }
}
