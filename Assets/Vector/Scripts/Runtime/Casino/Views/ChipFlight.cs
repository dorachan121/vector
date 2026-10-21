using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>チップの山を飛ばす演出（ベット・払い戻し・回収）。どのテーブルでも共通。</summary>
    public static class ChipFlight
    {
        const float FlightSeconds = 0.38f;
        const int MaxFlyingChips = 4;

        /// <summary>飛ばし始めてから最後のチップが着くまでの時間。</summary>
        public static float Seconds => FlightSeconds + MaxFlyingChips * 0.05f;

        /// <summary>額に応じて最大数枚のチップを、fx レイヤーの上で from から to へ飛ばして消す。</summary>
        public static void Fly(RectTransform fx, long amount, Vector3 fromWorld, Vector3 toWorld, float delay = 0f)
        {
            if (amount <= 0)
                return;
            var chips = ChipSet.Breakdown(amount, MaxFlyingChips);
            Vector2 from = fx.InverseTransformPoint(fromWorld), to = fx.InverseTransformPoint(toWorld);
            for (int i = 0; i < chips.Count; i++)
            {
                var chip = ChipView.Create(fx, chips[i], 56, from + new Vector2(0, i * 5));
                float d = delay + i * 0.05f;
                Tween.Move(chip.Rect, to + new Vector2(0, i * 5), FlightSeconds, Ease.InOutQuad, () => Object.Destroy(chip.gameObject), d);
            }
        }
    }
}
