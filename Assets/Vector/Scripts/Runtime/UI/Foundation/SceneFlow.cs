using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vector.Game.UI
{
    /// <summary>
    /// シーンの切り替え。暗転してから読み込み、新しいシーンで明けていく。暗転中は画面の操作を受け付けない。
    /// 暗転用の画面はシーンをまたいで使い回す。
    /// </summary>
    public sealed class SceneFlow : MonoBehaviour
    {
        const float OutSeconds = 0.22f;
        const float InSeconds = 0.35f;

        static SceneFlow instance;
        CanvasGroup group;
        bool busy;

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

        static SceneFlow Instance()
        {
            if (instance != null)
                return instance;
            var canvas = new GameObject("[SceneFlow]", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvas);
            var c = canvas.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 1000;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Ui.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            instance = canvas.AddComponent<SceneFlow>();
            var cover = Ui.Image(canvas.transform, "Cover", Chrome.Gradient(Palette.Violet, Palette.NightDeep), Color.white, Vector2.zero, Vector2.zero, true);
            cover.rectTransform.anchorMin = Vector2.zero;
            cover.rectTransform.anchorMax = Vector2.one;
            var mark = Ui.Image(cover.transform, "Mark", Chrome.Sparkle, Palette.Gold, Vector2.zero, new Vector2(120, 120));
            Spin.Add(mark, 160f);
            instance.group = Ui.Group(canvas);
            instance.group.alpha = 0;
            instance.group.blocksRaycasts = false;
            return instance;
        }

        IEnumerator Transition(string scene)
        {
            busy = true;
            group.blocksRaycasts = true;
            for (float t = 0; t < OutSeconds; t += Time.unscaledDeltaTime)
            {
                group.alpha = t / OutSeconds;
                yield return null;
            }
            group.alpha = 1;
            SceneManager.LoadScene(scene);
            yield return null;
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
