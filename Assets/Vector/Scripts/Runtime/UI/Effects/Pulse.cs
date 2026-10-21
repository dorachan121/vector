using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>ゆっくり拡縮を繰り返して目を引く（押してほしいボタン・通知のバッジ）。</summary>
    public sealed class Pulse : MonoBehaviour
    {
        float amount;
        float speed;
        float phase;

        public static Pulse Add(Component target, float amount = 0.06f, float speed = 3.2f)
        {
            var pulse = target.gameObject.AddComponent<Pulse>();
            pulse.amount = amount;
            pulse.speed = speed;
            pulse.phase = Random.value * 6f;
            return pulse;
        }

        void Update()
        {
            float s = 1 + amount * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * speed + phase));
            transform.localScale = new Vector3(s, s, 1);
        }
    }
}
