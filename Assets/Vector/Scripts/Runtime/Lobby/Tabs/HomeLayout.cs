using UnityEngine;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ホームのタブの中身の置き場所。中身はヘッダーと下のタブに挟まれた最小の高さ（基準の縦画面のとき）を基準に組み、
    /// 縦に長い端末では上下の端に寄せるか真ん中に置く（余った高さはその間に回る）。
    /// </summary>
    public static class HomeLayout
    {
        public static readonly float ContentHeight = Ui.Portrait.y - HomeHeader.Height - BottomNav.Height;

        /// <summary>上端に寄せる中身の土台（座標は基準の高さの中身の中心から）。</summary>
        public static RectTransform Top(Transform tab) => Ui.Edge(tab, "Top", true, Ui.Portrait.x, ContentHeight);

        /// <summary>真ん中に置く中身の土台（座標は基準の高さの中身の中心から）。上下に寄せた中身との間は均等に空く。</summary>
        public static RectTransform Middle(Transform tab) => Ui.Rect(tab, "Middle", Vector2.zero, new Vector2(Ui.Portrait.x, ContentHeight));

        /// <summary>下端に寄せる中身の土台（座標は基準の高さの中身の中心から）。</summary>
        public static RectTransform Bottom(Transform tab) => Ui.Edge(tab, "Bottom", false, Ui.Portrait.x, ContentHeight);
    }
}
