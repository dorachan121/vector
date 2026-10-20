using System;
using System.IO;
using System.Text;

namespace Vector.Game.Persistence
{
    /// <summary>
    /// ファイルへの保存。一時ファイルに書いてから置き換えるので、書き込み中に落ちても既存のデータは壊れない。
    /// </summary>
    public sealed class FileProfileStorage : IProfileStorage
    {
        /// <summary>これを超えるファイルは不正とみなして読まない。</summary>
        const long MaxFileBytes = 64 * 1024;

        readonly string path;

        public FileProfileStorage(string directory, string fileName = "profile.sav")
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("保存先のフォルダを指定してください。", nameof(directory));
            path = Path.Combine(directory, fileName);
        }

        string BackupPath => path + ".bak";
        string TempPath => path + ".tmp";

        public string ReadPrimary() => Read(path);

        public string ReadBackup() => Read(BackupPath);

        public void Write(string contents)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(TempPath, contents, Encoding.UTF8);
            if (File.Exists(path))
                File.Replace(TempPath, path, BackupPath);
            else
                File.Move(TempPath, path);
        }

        static string Read(string file)
        {
            var info = new FileInfo(file);
            if (!info.Exists || info.Length > MaxFileBytes)
                return null;
            return File.ReadAllText(file, Encoding.UTF8);
        }
    }
}
