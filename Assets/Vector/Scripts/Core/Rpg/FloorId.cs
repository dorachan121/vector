namespace Vector.Core.Rpg
{
    /// <summary>カジノリゾートのフロア（物語の順）。</summary>
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
    }
}
