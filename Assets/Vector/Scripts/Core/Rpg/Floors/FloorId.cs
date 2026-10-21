namespace Vector.Core.Rpg
{
    /// <summary>
    /// カジノリゾートのフロア。値は保存データ（いまいるフロア）に残るので、新しいフロアは末尾に足す。
    /// 物語の順（主に勝つたびに開く順）は <see cref="Floors.All"/> の並び。
    /// </summary>
    public enum FloorId : byte
    {
        /// <summary>1F グランドロビー（VECTOR）。</summary>
        Lobby,

        /// <summary>2F ダイスパーラー（CROWN DICE）。</summary>
        DiceParlor,

        /// <summary>3F アリーナ（COLOSSEUM）。</summary>
        Arena,

        /// <summary>4F ビーストトラック（DERBY）。</summary>
        BeastTrack,

        /// <summary>5F 錬金の間（ALCHEMIA）。</summary>
        AlchemyHall,

        /// <summary>B1 アンダーヴォールト（DELVE）。</summary>
        Undervault,

        /// <summary>最上階ペントハウス（オーナーとの最後の勝負）。</summary>
        Penthouse,

        /// <summary>6F 天文台（ORBIT）。</summary>
        Observatory,

        /// <summary>B2 深海水族館（HARPOON）。</summary>
        Aquarium,

        /// <summary>7F 宿場町（SUGOROKU）。</summary>
        PostTown,

        /// <summary>8F 屋上（HANABI）。</summary>
        Rooftop,
    }
}
