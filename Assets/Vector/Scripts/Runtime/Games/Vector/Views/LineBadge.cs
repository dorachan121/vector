using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>1本のラインの合計を示す小さな札。どちらの陣営のラインかで色が変わる。</summary>
    public sealed class LineBadge : MonoBehaviour
    {
        static readonly Vector2 BadgeSize = new Vector2(64, 34);

        Image plate;
        TextMeshProUGUI text;
        string prefix;

        public static LineBadge Create(Transform parent, string name, Vector2 position, string prefix)
        {
            var rt = Ui.Rect(parent, name, position, BadgeSize);
            var badge = rt.gameObject.AddComponent<LineBadge>();
            badge.plate = Ui.Panel(rt, "Plate", Vector2.zero, BadgeSize, Theme.Panel, 17);
            badge.text = Ui.Text(rt, "Sum", "", Fonts.Label, 22, Theme.Ivory, Vector2.zero, BadgeSize);
            badge.prefix = prefix;
            return badge;
        }

        public void Show(int sum, Color tint, bool emphasized)
        {
            text.text = prefix + sum;
            plate.color = emphasized ? tint : Color.Lerp(Theme.Panel, tint, 0.35f);
            text.color = emphasized ? Theme.Ink : Theme.Ivory;
            transform.localScale = Vector3.one * (emphasized ? 1.12f : 1f);
        }
    }
}
