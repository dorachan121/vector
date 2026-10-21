using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>一定の速さで回り続ける（光の放射・読み込み中の印）。</summary>
    public sealed class Spin : MonoBehaviour
    {
        float degreesPerSecond;

        public static Spin Add(Component target, float degreesPerSecond)
        {
            var spin = target.gameObject.AddComponent<Spin>();
            spin.degreesPerSecond = degreesPerSecond;
            return spin;
        }

        void Update()
        {
            var e = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(0, 0, e.z + degreesPerSecond * Time.unscaledDeltaTime);
        }
    }
}
