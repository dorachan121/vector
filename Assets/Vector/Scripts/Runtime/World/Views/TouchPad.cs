using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.UI;

namespace Vector.Game.World
{
    /// <summary>
    /// 画面の下のタッチパッド（左に十字の矢印、右に話す・調べるボタン）。画面の下端に付く。
    /// キーボードと同じく、矢印は押している間その向きへ歩き、ボタンは向いている先に話しかける。
    /// </summary>
    public sealed class TouchPad : MonoBehaviour
    {
        public const float Height = 400f;

        readonly HoldButton[] arrows = new HoldButton[4];
        bool pressed;

        public static TouchPad Create(Transform parent)
        {
            float w = Ui.Portrait.x;
            var rt = Ui.Edge(parent, "TouchPad", false, w, Height);
            var pad = rt.gameObject.AddComponent<TouchPad>();
            Ui.Image(rt, "Fade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0f), Palette.NightDeep.WithAlpha(0.95f)), Color.white, new Vector2(0, 0), new Vector2(w * 3, Height));

            var cross = new Vector2(-w / 2 + 230, -10);
            Ui.Image(rt, "CrossBack", Art.Circle, Palette.Night.WithAlpha(0.8f), cross, Vector2.one * 330);
            Ui.Image(rt, "CrossRing", Art.Ring(0.04f), Palette.GoldMid.WithAlpha(0.6f), cross, Vector2.one * 330);
            // Directions の順（下・左・右・上）に、矢印の位置と、左向きのアイコンを回す角度
            var spots = new (int facing, Vector2 at, float angle)[]
            {
                (Directions.Down, new Vector2(0, -104), 90), (Directions.Left, new Vector2(-104, 0), 0),
                (Directions.Right, new Vector2(104, 0), 180), (Directions.Up, new Vector2(0, 104), -90),
            };
            foreach (var (facing, at, angle) in spots)
            {
                var face = Ui.Sliced(rt, "Arrow" + facing, Chrome.Gloss, Palette.VioletBright, cross + at, Vector2.one * 112, 56, true);
                Ui.Sliced(face.rectTransform, "Rim", Chrome.Rim, Palette.GoldLight.WithAlpha(0.7f), Vector2.zero, Vector2.one * 112, 56);
                var icon = Ui.Icon(face.rectTransform, "Icon", IconKind.Back, Palette.Ivory, Vector2.zero, 52);
                icon.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
                pad.arrows[facing] = face.gameObject.AddComponent<HoldButton>();
            }

            var action = Ui.Button(rt, "Talk", "TALK", new Vector2(w / 2 - 210, -10), new Vector2(230, 230), Palette.Gold, Palette.Ink, () =>
            {
                Sfx.Play(SfxKind.Button);
                pad.pressed = true;
            }, 44, 115);
            Ui.Text(action.transform, "Hint", "話す・調べる", Fonts.Body, 20, Palette.Ink.WithAlpha(0.75f), new Vector2(0, -50), new Vector2(200, 26));
            return pad;
        }

        /// <summary>押している矢印の向き（押していなければ -1）。</summary>
        public int HeldDirection
        {
            get
            {
                for (int i = 0; i < arrows.Length; i++)
                {
                    if (arrows[i].Held)
                        return i;
                }
                return -1;
            }
        }

        /// <summary>話すボタンが押されたか（読むと元に戻る）。</summary>
        public bool TakePress()
        {
            bool was = pressed;
            pressed = false;
            return was;
        }
    }
}
