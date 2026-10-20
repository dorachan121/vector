namespace Vector.Game.Persistence
{
    /// <summary>セーブデータの置き場所。テストではメモリ上の実装に差し替える。</summary>
    public interface IProfileStorage
    {
        /// <summary>最新のセーブデータ。無ければ null。</summary>
        string ReadPrimary();

        /// <summary>1つ前のセーブデータ（最新が壊れていたときの復旧用）。無ければ null。</summary>
        string ReadBackup();

        /// <summary>途中で中断されても壊れないように書き込み、直前の内容をバックアップとして残す。</summary>
        void Write(string contents);
    }
}
