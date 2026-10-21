namespace Vector.Core.Rpg
{
    /// <summary>フロアの1マスの種類。</summary>
    public enum Tile : byte
    {
        Wall,
        Floor,

        /// <summary>模様の違う床（絨毯の柄など）。歩ける。</summary>
        Rug,

        /// <summary>赤い絨毯の通路。歩ける。</summary>
        Carpet,

        /// <summary>そのフロアのゲームのテーブル（隣のマスから話しかけると着席）。</summary>
        Table,

        /// <summary>エレベーター（フロアの移動）。</summary>
        Elevator,

        /// <summary>キャッシャー（コンプの交換とチップの補充）。</summary>
        Cashier,

        /// <summary>バーのカウンター。</summary>
        Bar,

        Plant,
        Pillar,

        /// <summary>噴水・水路。</summary>
        Water,
    }
}
