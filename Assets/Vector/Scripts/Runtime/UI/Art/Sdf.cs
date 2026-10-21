using UnityEngine;

namespace Vector.Game.UI
{
    /// <summary>
    /// 2次元の符号付き距離関数（内側が負）。座標系はどれも呼び出し側の正規化座標で、Art / Chrome / Icons が図形の描画に使う。
    /// </summary>
    internal static class Sdf
    {
        internal static float SdCircle(float x, float y, float cx, float cy, float r)
        {
            float dx = x - cx, dy = y - cy;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        internal static float SdRoundBox(float x, float y, float hw, float hh, float r)
        {
            float qx = Mathf.Abs(x) - hw + r;
            float qy = Mathf.Abs(y) - hh + r;
            float ox = Mathf.Max(qx, 0), oy = Mathf.Max(qy, 0);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
        }

        internal static float SdRhombus(float x, float y, float hw, float hh)
        {
            float ax = Mathf.Abs(x), ay = Mathf.Abs(y);
            float k = hw * hh / Mathf.Sqrt(hw * hw + hh * hh);
            return (ax / hw + ay / hh - 1f) * k;
        }

        /// <summary>正 n 角形（apothem は中心から辺までの距離）。</summary>
        internal static float SdPolygon(float x, float y, int n, float apothem)
        {
            float sector = Mathf.PI * 2 / n;
            float a = Mathf.Repeat(Mathf.Atan2(y, x) + Mathf.PI / 2, sector) - sector / 2;
            return Mathf.Sqrt(x * x + y * y) * Mathf.Cos(a) - apothem;
        }

        /// <summary>n 芒星（inner / outer は谷と先端の半径）。距離は近似だが、縁のアンチエイリアスには十分。</summary>
        internal static float SdStar(float x, float y, int n, float inner, float outer)
        {
            float sector = Mathf.PI * 2 / n;
            float a = Mathf.Abs(Mathf.Repeat(Mathf.Atan2(y, x) + Mathf.PI / 2, sector) - sector / 2) / (sector / 2);
            float radius = Mathf.Lerp(outer, inner, a);
            return (Mathf.Sqrt(x * x + y * y) - radius) * 0.7f;
        }

        /// <summary>三角形（Inigo Quilez の距離関数）。</summary>
        internal static float SdTriangle(float px, float py, float ax, float ay, float bx, float by, float cx, float cy)
        {
            float e0x = bx - ax, e0y = by - ay;
            float e1x = cx - bx, e1y = cy - by;
            float e2x = ax - cx, e2y = ay - cy;
            float v0x = px - ax, v0y = py - ay;
            float v1x = px - bx, v1y = py - by;
            float v2x = px - cx, v2y = py - cy;
            float t0 = Mathf.Clamp01((v0x * e0x + v0y * e0y) / (e0x * e0x + e0y * e0y));
            float t1 = Mathf.Clamp01((v1x * e1x + v1y * e1y) / (e1x * e1x + e1y * e1y));
            float t2 = Mathf.Clamp01((v2x * e2x + v2y * e2y) / (e2x * e2x + e2y * e2y));
            float pq0x = v0x - e0x * t0, pq0y = v0y - e0y * t0;
            float pq1x = v1x - e1x * t1, pq1y = v1y - e1y * t1;
            float pq2x = v2x - e2x * t2, pq2y = v2y - e2y * t2;
            float s = Mathf.Sign(e0x * e2y - e0y * e2x);
            float d0 = pq0x * pq0x + pq0y * pq0y, s0 = s * (v0x * e0y - v0y * e0x);
            float d1 = pq1x * pq1x + pq1y * pq1y, s1 = s * (v1x * e1y - v1y * e1x);
            float d2 = pq2x * pq2x + pq2y * pq2y, s2 = s * (v2x * e2y - v2y * e2x);
            float d = Mathf.Min(d0, Mathf.Min(d1, d2));
            float sg = Mathf.Min(s0, Mathf.Min(s1, s2));
            return -Mathf.Sqrt(d) * Mathf.Sign(sg);
        }

        /// <summary>a から b への線分を半径 r で太らせた形（カプセル）。</summary>
        internal static float SdSegment(float px, float py, float ax, float ay, float bx, float by, float r)
        {
            float pax = px - ax, pay = py - ay, bax = bx - ax, bay = by - ay;
            float h = Mathf.Clamp01((pax * bax + pay * bay) / (bax * bax + bay * bay));
            float dx = pax - bax * h, dy = pay - bay * h;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        /// <summary>楕円（近似。縁のアンチエイリアスには十分）。</summary>
        internal static float SdEllipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            float u = (x - cx) / rx, v = (y - cy) / ry;
            return (Mathf.Sqrt(u * u + v * v) - 1f) * Mathf.Min(rx, ry);
        }

        /// <summary>点を原点まわりに degrees だけ回す（図形を逆に回したいときは負の角度）。</summary>
        internal static void Rotate(ref float x, ref float y, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
            float nx = c * x - s * y;
            y = s * x + c * y;
            x = nx;
        }
    }
}
