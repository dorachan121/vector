using UnityEditor;

namespace Vector.EditorTools
{
    /// <summary>エディタの起動時（スクリプトの再読み込み後）に、日本語フォントが画面の文言を網羅しているか確かめる。</summary>
    [InitializeOnLoad]
    static class JapaneseFontAutoRebuild
    {
        // アセットの読み込みが終わってから確かめる
        static JapaneseFontAutoRebuild() => EditorApplication.delayCall += JapaneseFontBuilder.EnsureUpToDate;
    }
}
