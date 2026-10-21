using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Vector.EditorTools
{
    /// <summary>ビルドの前に日本語フォントを最新にする（文字の抜けたフォントで出荷しないように）。</summary>
    sealed class JapaneseFontBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) => JapaneseFontBuilder.EnsureUpToDate();
    }
}
