using UnityEngine;
using static Vector.Game.UI.Sdf;
using static Vector.Game.UI.SpriteFactory;

namespace Vector.Game.UI
{
    /// <summary>立ち絵（バストアップ）の部品の形（白。色は Image.color で付ける）。</summary>
    public static class PortraitArt
    {
        /// <summary>前髪。上は丸い頭頂、下は毛先が尖った房。</summary>
        public static Sprite Bangs => Cached("portrait.bangs", () => Paint(256, 200, (x, y) =>
        {
            // 頭頂の丸み。下の縁は横ほど下がる曲線で切り、横髪とつながるようにする
            float u = (x - 128) / 128f;
            float dome = Mathf.Max(SdEllipse(x, y, 128, 70, 126, 128), -(y - (96 - 64 * u * u)));
            float teeth = float.MaxValue;
            float[] tips = { 18, 52, 92, 128, 164, 204, 238 };
            float[] lengths = { 40, 78, 66, 84, 64, 76, 42 };
            for (int i = 0; i < tips.Length; i++)
            {
                float tx = tips[i];
                float left = tx - 30, right = tx + 30;
                teeth = Mathf.Min(teeth, SdTriangle(x, y, left, 104, right, 104, tx + (tx < 128 ? -6 : 6), 104 - lengths[i]));
            }
            // 房は頭の丸みの内側だけ（端の房の上辺が外にはみ出さないように）
            teeth = Mathf.Max(teeth, SdEllipse(x, y, 128, 110, 127, 170));
            return new Color(1, 1, 1, Coverage(Mathf.Min(dome, teeth)));
        }));

        /// <summary>上まぶたとまつげ（太い弧と、目じりの跳ね）。</summary>
        public static Sprite Lash => Cached("portrait.lash", () => Paint(96, 44, (x, y) =>
        {
            float arc = Mathf.Max(Mathf.Abs(SdEllipse(x, y, 48, 4, 42, 34)) - 4.5f, -(y - 12));
            float flick = SdSegment(x, y, 84, 20, 94, 30, 3f);
            return new Color(1, 1, 1, Coverage(Mathf.Min(arc, flick)));
        }));

        /// <summary>にっこりした口元（下向きの弧）。</summary>
        public static Sprite Smile => Cached("portrait.smile", () => Paint(64, 32, (x, y) =>
        {
            float arc = Mathf.Max(Mathf.Abs(SdCircle(x, y, 32, 34, 24)) - 3f, y - 22);
            return new Color(1, 1, 1, Coverage(arc));
        }));

        /// <summary>上向きの二等辺三角形（襟やリボン）。</summary>
        public static Sprite Triangle => Cached("portrait.triangle", () => Paint(64, 64, (x, y) =>
            new Color(1, 1, 1, Coverage(SdTriangle(x, y, 2, 2, 62, 2, 32, 62)))));
    }
}
