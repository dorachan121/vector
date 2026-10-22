using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Vector.EditorTools
{
    /// <summary>
    /// Google Play に出す Android App Bundle（.aab）を、アップロード鍵で署名して Builds/Android に作る。
    /// メニュー「Vector/Build Android App Bundle」か、バッチモードの -executeMethod Vector.EditorTools.AndroidRelease.Build。
    /// <para>鍵とパスワードはリポジトリに置かず、環境変数で渡す（<see cref="KeystoreVariable"/> ほか）。
    /// 環境変数が無ければ、プレイヤー設定の Publishing Settings でこのセッションに入れた鍵を使う。
    /// Play に上げるたびに Version Code（Bundle Version Code）を1つ上げること。</para>
    /// </summary>
    public static class AndroidRelease
    {
        public const string KeystoreVariable = "VECTOR_KEYSTORE";
        public const string KeystorePassVariable = "VECTOR_KEYSTORE_PASS";
        public const string AliasVariable = "VECTOR_KEY_ALIAS";
        /// <summary>省略したときはキーストアのパスワードと同じ。</summary>
        public const string AliasPassVariable = "VECTOR_KEY_PASS";

        const string OutputFolder = "Builds/Android";

        [MenuItem("Vector/Build Android App Bundle")]
        public static void Build()
        {
            var signing = Signing.Capture();
            try
            {
                if (!ApplySigning(out string problem))
                {
                    Finish(false, problem);
                    return;
                }
                Directory.CreateDirectory(OutputFolder);
                EditorUserBuildSettings.buildAppBundle = true;
                string path = $"{OutputFolder}/VECTOR-{PlayerSettings.bundleVersion}-{PlayerSettings.Android.bundleVersionCode}.aab";
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    locationPathName = path,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None,
                });
                bool ok = report.summary.result == BuildResult.Succeeded;
                Finish(ok, ok ? $"{path} を作りました（{report.summary.totalSize / (1024f * 1024f):F1} MB）。" : $"ビルドに失敗しました（{report.summary.totalErrors} 件のエラー）。");
            }
            finally
            {
                // 端末ごとの鍵の置き場所を ProjectSettings に残さない
                signing.Restore();
            }
        }

        /// <summary>環境変数の鍵を使うように設定する。署名の鍵が無ければ false（デバッグ鍵の .aab は Play に上げられない）。</summary>
        static bool ApplySigning(out string problem)
        {
            problem = null;
            string keystore = Environment.GetEnvironmentVariable(KeystoreVariable);
            if (!string.IsNullOrEmpty(keystore))
            {
                string pass = Environment.GetEnvironmentVariable(KeystorePassVariable);
                string alias = Environment.GetEnvironmentVariable(AliasVariable);
                if (!File.Exists(keystore) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(alias))
                {
                    problem = $"{KeystoreVariable} の鍵が見つからないか、{KeystorePassVariable} / {AliasVariable} が空です。";
                    return false;
                }
                string aliasPass = Environment.GetEnvironmentVariable(AliasPassVariable);
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = keystore;
                PlayerSettings.Android.keystorePass = pass;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keyaliasPass = string.IsNullOrEmpty(aliasPass) ? pass : aliasPass;
                return true;
            }
            if (PlayerSettings.Android.useCustomKeystore && !string.IsNullOrEmpty(PlayerSettings.Android.keystorePass))
                return true;
            problem = $"署名の鍵がありません。環境変数 {KeystoreVariable}・{KeystorePassVariable}・{AliasVariable} を設定するか、" +
                      "Player Settings の Publishing Settings で鍵とパスワードを入れてください。";
            return false;
        }

        static void Finish(bool ok, string message)
        {
            if (ok)
                Debug.Log("[Vector] " + message);
            else
                Debug.LogError("[Vector] " + message);
            if (Application.isBatchMode)
                EditorApplication.Exit(ok ? 0 : 1);
        }

        /// <summary>ビルドの前の署名の設定（ビルドの後に元へ戻す）。</summary>
        readonly struct Signing
        {
            readonly bool custom;
            readonly string keystore;
            readonly string alias;

            Signing(bool custom, string keystore, string alias)
            {
                this.custom = custom;
                this.keystore = keystore;
                this.alias = alias;
            }

            public static Signing Capture() => new Signing(PlayerSettings.Android.useCustomKeystore, PlayerSettings.Android.keystoreName, PlayerSettings.Android.keyaliasName);

            public void Restore()
            {
                PlayerSettings.Android.useCustomKeystore = custom;
                PlayerSettings.Android.keystoreName = keystore;
                PlayerSettings.Android.keyaliasName = alias;
            }
        }
    }
}
