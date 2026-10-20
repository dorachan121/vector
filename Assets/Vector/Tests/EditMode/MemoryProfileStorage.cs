using Vector.Game.Persistence;

namespace Vector.Tests
{
    /// <summary>ファイルの代わりにメモリへ保存するテスト用ストレージ。</summary>
    sealed class MemoryProfileStorage : IProfileStorage
    {
        public string Primary;
        public string Backup;
        public int Writes;

        public string ReadPrimary() => Primary;

        public string ReadBackup() => Backup;

        public void Write(string contents)
        {
            Backup = Primary;
            Primary = contents;
            Writes++;
        }
    }
}
