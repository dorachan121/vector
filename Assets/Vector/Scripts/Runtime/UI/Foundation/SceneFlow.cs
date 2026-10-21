using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector.Game.UI.Widgets;

namespace Vector.Game.UI
{
    /// <summary>
    /// シーンの切り替え。暗転してから読み込み、新しいシーンで明けていく。暗転中は画面の操作を受け付けない。
    /// 新しいシーンの画面の形（縦・横）が変わるときは、暗転したまま端末の回転を促してから向きを変える。
    /// 暗転用の画面はシーンをまたいで使い回す。
    /// </summary>
    public sealed class SceneFlow : MonoBehaviour
    {
        const float OutSeconds = 0.22f;
        const float InSeconds = 0.35f;

        /// <summary>暗転用の画面の基準（縦でも横でも収まる正方形）。</summary>
        static readonly Vector2 Reference = new Vector2(1080f, 1080f);

        static SceneFlow instance;
        CanvasGroup group;
        RectTransform cover;
        GameObject mark;
        bool busy;
        ScreenShape? requested;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => instance = null;

        /// <summary>暗転してシーンを読み込む。切り替えの途中に呼ばれたら無視する。</summary>
        public static void Load(string scene)
        {
            var flow = Instance();
            if (flow.busy)
                return;
            flow.StartCoroutine(flow.Transition(scene));
        }

        /// <summary>
        /// 画面の形を使い始める（<see cref="Ui.CreateScreen"/> から）。切り替えの途中なら明ける前に回転を促してから、そうでなければすぐに向きを変える。
        /// </summary>
        internal static void UseShape(ScreenShape shape)
        {
            if (instance != null && instance.busy)
                instance.requested = shape;
            else
                OrientationLock.Apply(shape);
        }

        static SceneFlow Instance()
        {
            if (instance != null)
                return instance;
            var canvas = Ui.CreateCanvas("[SceneFlow]", 1000, Reference);
            DontDestroyOnLoad(canvas.gameObject);
            instance = canvas.gameObject.AddComponent<SceneFlow>();
            var image = Ui.Image(canvas.transform, "Cover", Chrome.Gradient(Palette.Violet, Palette.NightDeep), Color.white, Vector2.zero, Vector2.zero, true);
            instance.cover = image.rectTransform;
            instance.cover.anchorMin = Vector2.zero;
            instance.cover.anchorMax = Vector2.one;
            var mark = Ui.Image(instance.cover, "Mark", Chrome.Sparkle, Palette.Gold, Vector2.zero, new Vector2(120, 120));
            Spin.Add(mark, 160f);
            instance.mark = mark.gameObject;
            instance.group = Ui.Group(canvas.gameObject);
            instance.group.alpha = 0;
            instance.group.blocksRaycasts = false;
            return instance;
        }

        IEnumerator Transition(string scene)
        {
            busy = true;
            requested = null;
            group.blocksRaycasts = true;
            for (float t = 0; t < OutSeconds; t += Time.unscaledDeltaTime)
            {
                group.alpha = t / OutSeconds;
                yield return null;
            }
            group.alpha = 1;
            SceneManager.LoadScene(scene);
            // 次のフレームで新しいシーンの Start（画面の組み立て）が済む
            yield return null;
            if (requested is ScreenShape shape)
            {
                if (shape != OrientationLock.Current)
                {
                    mark.SetActive(false);
                    yield return RotatePrompt.Play(cover, shape);
                    mark.SetActive(true);
                }
                OrientationLock.Apply(shape);
                requested = null;
            }
            for (float t = 0; t < InSeconds; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1 - t / InSeconds;
                yield return null;
            }
            group.alpha = 0;
            group.blocksRaycasts = false;
            busy = false;
        }
    }
}
