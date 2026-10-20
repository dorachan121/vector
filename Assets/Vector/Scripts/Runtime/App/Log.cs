using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Vector.Game
{
    /// <summary>
    /// ログ出力。情報ログはエディタとデバッグ版でのみ出力し、リリースビルドではコンパイル時に除去する。
    /// </summary>
    public static class Log
    {
        const string Prefix = "[Vector] ";

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
        public static void Info(string message) => Debug.Log(Prefix + message);

        public static void Warn(string message) => Debug.LogWarning(Prefix + message);

        public static void Error(string message) => Debug.LogError(Prefix + message);

        public static void Exception(System.Exception exception) => Debug.LogException(exception);
    }
}
