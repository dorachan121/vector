using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.Audio;
using Vector.Game.UI;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// 海の底に据えたモリの発射台。狙った獲物へ砲身を向けてモリを撃ち、ロープで引き戻す。
    /// 座標は獲物と同じ海の座標で、モリは飛んでいる間も動く獲物を追いかける（結果はもう決まっていて、演出だけ）。
    /// </summary>
    public sealed class LauncherView
    {
        const float BarrelLength = 96f;
        const float FlightSeconds = 0.26f;
        const float ReelSeconds = 0.4f;

        RectTransform turret;
        RectTransform harpoon;
        Image rope;

        /// <summary>砲身の回転の中心。</summary>
        public Vector2 Pivot { get; private set; }

        /// <summary>モリの出口（砲身の先）。</summary>
        public Vector2 Muzzle => Pivot + Direction * BarrelLength;

        Vector2 Direction
        {
            get
            {
                float rad = -turret.localEulerAngles.z * Mathf.Deg2Rad;
                return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            }
        }

        public static LauncherView Create(RectTransform layer, Vector2 pivot)
        {
            var view = new LauncherView { Pivot = pivot };
            view.rope = Ui.Image(layer, "Rope", null, Theme.Hex("E8DCC0").WithAlpha(0f), pivot, new Vector2(3, 0));
            view.rope.rectTransform.pivot = new Vector2(0.5f, 0);

            view.turret = Ui.Rect(layer, "Turret", pivot, new Vector2(60, BarrelLength * 2));
            Ui.Panel(view.turret, "Barrel", new Vector2(0, BarrelLength / 2), new Vector2(40, BarrelLength + 10), Theme.Hex("5B6B7C"), 12);
            Ui.Panel(view.turret, "Band", new Vector2(0, BarrelLength - 6), new Vector2(46, 14), Palette.Gold, 6);
            Ui.Panel(view.turret, "Band", new Vector2(0, BarrelLength * 0.35f), new Vector2(44, 10), Palette.GoldDark, 5);

            view.harpoon = Ui.Image(layer, "Harpoon", MarineArt.Harpoon, Theme.Hex("E6ECF2"), view.Muzzle, new Vector2(120, 25)).rectTransform;
            view.harpoon.localEulerAngles = new Vector3(0, 0, 90);

            Ui.Image(layer, "DomeGlow", Art.Glow, Palette.Cyan.WithAlpha(0.25f), pivot, new Vector2(230, 170));
            Ui.Image(layer, "Dome", Art.Circle, Theme.Hex("26313D"), pivot, new Vector2(130, 130));
            Ui.Image(layer, "DomeRim", Art.Ring(0.08f), Palette.Gold, pivot, new Vector2(130, 130));
            Ui.Image(layer, "Lens", Art.Circle, Palette.Cyan.WithAlpha(0.8f), pivot, new Vector2(34, 34));
            return view;
        }

        /// <summary>獲物へモリを撃ち、届くまで待つ（届いたときの獲物の位置を返す）。</summary>
        public IEnumerator Fire(FishView target)
        {
            Aim(target.Position);
            Sfx.Play(SfxKind.Harpoon);
            var from = Muzzle;
            Tween.Run(harpoon, "flight", FlightSeconds, Ease.InCubic, t => Place(Vector2.Lerp(from, target.Position, t)));
            yield return Tween.Wait(FlightSeconds);
            Place(target.Position);
        }

        /// <summary>モリを引き戻す。捕まえた獲物があれば一緒に引き寄せ、手前で小さくする。</summary>
        public IEnumerator Reel(FishView caught)
        {
            var from = (Vector2)harpoon.anchoredPosition;
            var fishFrom = caught != null ? caught.Position : Vector2.zero;
            Tween.Run(harpoon, "reel", ReelSeconds, Ease.InOutQuad, t =>
            {
                var at = Vector2.Lerp(from, Muzzle, t);
                Place(at);
                if (caught != null && caught.Rect != null)
                {
                    caught.Rect.anchoredPosition = Vector2.Lerp(fishFrom, Muzzle, t);
                    float s = Mathf.Lerp(1f, 0.35f, t);
                    caught.Rect.localScale = new Vector3(Mathf.Sign(caught.Rect.localScale.x) * s, s, 1);
                }
            });
            yield return Tween.Wait(ReelSeconds);
            Stow();
        }

        void Aim(Vector2 target)
        {
            var d = target - Pivot;
            float angle = Mathf.Atan2(d.x, Mathf.Max(10f, d.y)) * Mathf.Rad2Deg;
            turret.localEulerAngles = new Vector3(0, 0, -angle);
        }

        /// <summary>モリの穂先を at に置き、砲身の先からロープを張る。</summary>
        void Place(Vector2 at)
        {
            var muzzle = Muzzle;
            var d = at - muzzle;
            var direction = d.sqrMagnitude > 1f ? d.normalized : Direction;
            harpoon.anchoredPosition = at - direction * 50f;
            harpoon.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            rope.rectTransform.anchoredPosition = muzzle;
            rope.rectTransform.sizeDelta = new Vector2(3, Mathf.Max(0, d.magnitude - 90f));
            rope.rectTransform.localEulerAngles = new Vector3(0, 0, -Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg);
            rope.color = rope.color.WithAlpha(0.85f);
        }

        /// <summary>モリを砲身に戻してロープを隠す。</summary>
        void Stow()
        {
            harpoon.anchoredPosition = Muzzle;
            harpoon.localEulerAngles = new Vector3(0, 0, 90 + turret.localEulerAngles.z);
            rope.color = rope.color.WithAlpha(0f);
        }
    }
}
