namespace Vector.Game.Persistence
{
    public enum ProfileLoadResult
    {
        /// <summary>セーブデータが無く、新規に作った。</summary>
        New,

        /// <summary>最新のセーブデータを読み込んだ。</summary>
        Loaded,

        /// <summary>最新が壊れていたため、1つ前のバックアップから復旧した。</summary>
        RestoredFromBackup,

        /// <summary>最新もバックアップも読めなかったため、初期状態に戻した。</summary>
        ResetAfterCorruption,
    }
}
