using System;

namespace Vector.Core.Delve
{
    /// <summary>迷宮の1つの深さ（扉の数と、そのうち魔物がいる扉の数）。</summary>
    public readonly struct DelveFloor
    {
        public readonly int Doors;
        public readonly int Monsters;

        public DelveFloor(int doors, int monsters)
        {
            if (doors < 2)
                throw new ArgumentOutOfRangeException(nameof(doors));
            if (monsters < 1 || monsters >= doors)
                throw new ArgumentOutOfRangeException(nameof(monsters));
            Doors = doors;
            Monsters = monsters;
        }

        public int SafeDoors => Doors - Monsters;

        /// <summary>この深さを無事に抜ける確率。</summary>
        public double Survival => SafeDoors / (double)Doors;
    }
}
