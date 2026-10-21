using Vector.Core.Rpg;

namespace Vector.Game.Story
{
    /// <summary>フロアの並び（建物の上から・挑む順）。</summary>
    public static class FloorOrder
    {
        /// <summary>建物の上から（エレベーターやフロアの一覧の表示順）。</summary>
        public static readonly FloorId[] TopDown =
        {
            FloorId.Penthouse, FloorId.Rooftop, FloorId.PostTown, FloorId.Observatory, FloorId.AlchemyHall, FloorId.BeastTrack, FloorId.Arena, FloorId.DiceParlor, FloorId.Lobby, FloorId.Undervault, FloorId.Aquarium,
        };

        /// <summary>挑む順（1F から主に勝つたびに次へ）。</summary>
        public static readonly FloorId[] BottomUp = Unlocking();

        static FloorId[] Unlocking()
        {
            var order = new FloorId[Floors.All.Count];
            for (int i = 0; i < order.Length; i++)
                order[i] = Floors.All[i].Id;
            return order;
        }
    }
}
