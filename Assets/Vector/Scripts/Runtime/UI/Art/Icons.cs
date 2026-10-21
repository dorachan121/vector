using System;
using UnityEngine;
using static Vector.Game.UI.Sdf;
using static Vector.Game.UI.SpriteFactory;

namespace Vector.Game.UI
{
    /// <summary>
    /// アイコンを距離関数で描く（白・128px・正規化座標 [-1, 1]）。どの大きさでも輪郭が崩れない。
    /// </summary>
    public static class Icons
    {
        public static Sprite Of(IconKind kind) => Cached("icon." + kind, () => Disc(128, Shape(kind)));

        static Func<float, float, float> Shape(IconKind kind)
        {
            switch (kind)
            {
                case IconKind.Home:
                    return (x, y) =>
                    {
                        float roof = SdTriangle(x, y, -0.92f, 0.08f, 0.92f, 0.08f, 0f, 0.86f);
                        float body = SdRoundBox(x, y + 0.36f, 0.62f, 0.48f, 0.08f);
                        float door = SdRoundBox(x, y + 0.56f, 0.17f, 0.3f, 0.06f);
                        return Mathf.Max(Mathf.Min(roof, body), -door);
                    };
                case IconKind.Games:
                    return (x, y) =>
                    {
                        float bx = x + 0.2f, by = y - 0.02f;
                        Rotate(ref bx, ref by, 14f);
                        float back = SdRoundBox(bx, by, 0.42f, 0.62f, 0.1f);
                        float fx = x - 0.24f, fy = y + 0.06f;
                        Rotate(ref fx, ref fy, -12f);
                        float front = SdRoundBox(fx, fy, 0.44f, 0.64f, 0.1f);
                        float diamond = SdRhombus(fx, fy, 0.2f, 0.3f);
                        return Mathf.Min(Mathf.Max(back, -(front - 0.08f)), Mathf.Max(front, -diamond));
                    };
                case IconKind.Story:
                    return (x, y) =>
                    {
                        float head = SdCircle(x, y, 0, 0.26f, 0.6f);
                        float tip = SdTriangle(x, y, -0.5f, -0.06f, 0.5f, -0.06f, 0f, -0.94f);
                        float hole = SdCircle(x, y, 0, 0.26f, 0.24f);
                        return Mathf.Max(Mathf.Min(head, tip), -hole);
                    };
                case IconKind.Missions:
                    return (x, y) =>
                    {
                        float board = SdRoundBox(x, y + 0.06f, 0.66f, 0.84f, 0.12f);
                        float clip = SdRoundBox(x, y - 0.82f, 0.3f, 0.14f, 0.06f);
                        float d = Mathf.Max(board, -SdRoundBox(x, y + 0.06f, 0.5f, 0.68f, 0.06f));
                        d = Mathf.Min(d, clip);
                        for (int i = 0; i < 3; i++)
                        {
                            float ly = 0.3f - i * 0.36f;
                            d = Mathf.Min(d, SdSegment(x, y, -0.06f, ly, 0.34f, ly, 0.07f));
                            d = Mathf.Min(d, Mathf.Min(SdSegment(x, y, -0.36f, ly, -0.27f, ly - 0.09f, 0.06f), SdSegment(x, y, -0.27f, ly - 0.09f, -0.13f, ly + 0.1f, 0.06f)));
                        }
                        return d;
                    };
                case IconKind.Shop:
                case IconKind.Gift:
                    return (x, y) =>
                    {
                        float box = SdRoundBox(x, y + 0.34f, 0.66f, 0.5f, 0.06f);
                        float lid = SdRoundBox(x, y - 0.24f, 0.78f, 0.16f, 0.05f);
                        float d = Mathf.Min(box, lid);
                        d = Mathf.Max(d, -SdRoundBox(x, y + 0.1f, 0.06f, 1f, 0f));
                        d = Mathf.Max(d, -SdRoundBox(x, y + 0.04f, 1f, 0.035f, 0f));
                        float bowL = Mathf.Abs(SdEllipse(x, y, -0.24f, 0.56f, 0.24f, 0.17f)) - 0.07f;
                        float bowR = Mathf.Abs(SdEllipse(x, y, 0.24f, 0.56f, 0.24f, 0.17f)) - 0.07f;
                        return Mathf.Min(d, Mathf.Min(bowL, bowR));
                    };
                case IconKind.Profile:
                    return (x, y) =>
                    {
                        float head = SdCircle(x, y, 0, 0.36f, 0.36f);
                        float body = Mathf.Max(SdEllipse(x, y, 0, -0.74f, 0.78f, 0.62f), -(y + 0.82f));
                        return Mathf.Min(head, body);
                    };
                case IconKind.Settings:
                    return (x, y) =>
                    {
                        float r = Mathf.Sqrt(x * x + y * y);
                        float a = Mathf.Atan2(y, x);
                        float teeth = Mathf.SmoothStep(-0.25f, 0.25f, Mathf.Cos(a * 8f)) * 0.17f;
                        float gear = r - (0.66f + teeth);
                        return Mathf.Max(gear, -SdCircle(x, y, 0, 0, 0.27f));
                    };
                case IconKind.Help:
                    return (x, y) =>
                    {
                        float arc = Mathf.Max(Mathf.Abs(SdCircle(x, y, 0, 0.3f, 0.36f)) - 0.12f, -(x + 0.2f) * 0.5f - (y - 0.1f));
                        float stem = SdSegment(x, y, 0.05f, 0.02f, 0f, -0.3f, 0.12f);
                        float dot = SdCircle(x, y, 0, -0.68f, 0.14f);
                        return Mathf.Min(Mathf.Min(arc, stem), dot);
                    };
                case IconKind.Back:
                    return (x, y) => Mathf.Min(SdSegment(x, y, 0.3f, 0.62f, -0.34f, 0f, 0.15f), SdSegment(x, y, -0.34f, 0f, 0.3f, -0.62f, 0.15f));
                case IconKind.Plus:
                    return (x, y) => Mathf.Min(SdSegment(x, y, -0.62f, 0, 0.62f, 0, 0.17f), SdSegment(x, y, 0, -0.62f, 0, 0.62f, 0.17f));
                case IconKind.Close:
                    return (x, y) => Mathf.Min(SdSegment(x, y, -0.52f, -0.52f, 0.52f, 0.52f, 0.15f), SdSegment(x, y, -0.52f, 0.52f, 0.52f, -0.52f, 0.15f));
                case IconKind.Lock:
                    return (x, y) =>
                    {
                        float body = SdRoundBox(x, y + 0.34f, 0.62f, 0.5f, 0.12f);
                        float shackle = Mathf.Max(Mathf.Abs(SdRoundBox(x, y - 0.32f, 0.36f, 0.46f, 0.36f)) - 0.1f, -(y - 0.1f));
                        float hole = SdCircle(x, y, 0, -0.32f, 0.11f);
                        return Mathf.Max(Mathf.Min(body, shackle), -hole);
                    };
                case IconKind.Check:
                    return (x, y) => Mathf.Min(SdSegment(x, y, -0.62f, 0.02f, -0.18f, -0.46f, 0.16f), SdSegment(x, y, -0.18f, -0.46f, 0.66f, 0.52f, 0.16f));
                case IconKind.Star:
                    return (x, y) => SdStar(x, y + 0.04f, 5, 0.42f, 0.98f);
                case IconKind.Crown:
                    return (x, y) =>
                    {
                        float band = SdRoundBox(x, y + 0.52f, 0.76f, 0.16f, 0.05f);
                        float body = SdTriangle(x, y, -0.82f, -0.38f, 0.82f, -0.38f, 0f, 0.18f);
                        body = Mathf.Min(body, SdTriangle(x, y, -0.82f, -0.38f, -0.3f, -0.38f, -0.78f, 0.52f));
                        body = Mathf.Min(body, SdTriangle(x, y, 0.82f, -0.38f, 0.3f, -0.38f, 0.78f, 0.52f));
                        body = Mathf.Min(body, SdTriangle(x, y, -0.36f, -0.38f, 0.36f, -0.38f, 0f, 0.62f));
                        float jewels = Mathf.Min(SdCircle(x, y, 0, 0.74f, 0.13f), Mathf.Min(SdCircle(x, y, -0.8f, 0.62f, 0.12f), SdCircle(x, y, 0.8f, 0.62f, 0.12f)));
                        return Mathf.Min(Mathf.Min(band, body), jewels);
                    };
                case IconKind.Trophy:
                    return (x, y) =>
                    {
                        float cup = Mathf.Max(SdEllipse(x, y, 0, 0.42f, 0.56f, 0.6f), y - 0.72f);
                        cup = Mathf.Min(cup, SdRoundBox(x, y - 0.6f, 0.56f, 0.12f, 0.04f));
                        float handles = Mathf.Min(Mathf.Abs(SdCircle(x, y, -0.6f, 0.42f, 0.24f)) - 0.07f, Mathf.Abs(SdCircle(x, y, 0.6f, 0.42f, 0.24f)) - 0.07f);
                        float stem = SdRoundBox(x, y + 0.36f, 0.1f, 0.24f, 0.02f);
                        float foot = SdRoundBox(x, y + 0.72f, 0.44f, 0.12f, 0.05f);
                        return Mathf.Min(Mathf.Min(cup, handles), Mathf.Min(stem, foot));
                    };
                case IconKind.Calendar:
                    return (x, y) =>
                    {
                        float body = SdRoundBox(x, y + 0.1f, 0.78f, 0.72f, 0.14f);
                        float inner = SdRoundBox(x, y + 0.24f, 0.6f, 0.44f, 0.06f);
                        float d = Mathf.Max(body, -inner);
                        d = Mathf.Min(d, SdRoundBox(x, y + 0.16f, 0.16f, 0.14f, 0.03f));
                        float rings = Mathf.Min(SdSegment(x, y, -0.4f, 0.66f, -0.4f, 0.92f, 0.08f), SdSegment(x, y, 0.4f, 0.66f, 0.4f, 0.92f, 0.08f));
                        return Mathf.Min(d, rings);
                    };
                case IconKind.Play:
                    return (x, y) => SdTriangle(x, y, -0.46f, 0.7f, -0.46f, -0.7f, 0.74f, 0f) - 0.06f;
                case IconKind.Rotate:
                    return (x, y) =>
                    {
                        // 右上（-10° から 60° まで）を空けた輪と、空けた所の下端で上を指す矢じり
                        float ring = Mathf.Abs(Mathf.Sqrt(x * x + y * y) - 0.58f) - 0.12f;
                        float gap = Mathf.Max(x * -0.174f + y * -0.985f, x * -0.866f + y * 0.5f);
                        float head = SdTriangle(x, y, 0.27f, -0.05f, 0.87f, -0.15f, 0.64f, 0.3f);
                        return Mathf.Min(Mathf.Max(ring, -gap), head);
                    };
                default:
                    return (x, y) =>
                    {
                        float coin = SdCircle(x, y, 0, 0, 0.9f);
                        float rim = Mathf.Abs(SdCircle(x, y, 0, 0, 0.68f)) - 0.05f;
                        float star = SdStar(x, y + 0.02f, 5, 0.22f, 0.46f);
                        return Mathf.Max(coin, -Mathf.Min(rim, star));
                    };
            }
        }
    }
}
