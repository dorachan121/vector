using System.Collections.Generic;

namespace Vector.Core.Delve
{
    /// <summary>扉を開けた結果。開けた後は、その深さのすべての扉の中身を見せる。</summary>
    public readonly struct DoorReveal
    {
        public readonly int Depth;
        public readonly int Chosen;

        /// <summary>扉ごとに魔物がいたか。</summary>
        public readonly IReadOnlyList<bool> Monsters;

        public DoorReveal(int depth, int chosen, IReadOnlyList<bool> monsters)
        {
            Depth = depth;
            Chosen = chosen;
            Monsters = monsters;
        }

        public bool Survived => !Monsters[Chosen];
    }
}
