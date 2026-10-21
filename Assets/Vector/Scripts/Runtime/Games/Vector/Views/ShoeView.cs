using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>カードシューの置物。残り枚数と、カットカードまでの減り具合を示す。カードを配る起点。</summary>
    public sealed class ShoeView : MonoBehaviour
    {
        static readonly Vector2 BoxSize = new Vector2(150, 96);

        TextMeshProUGUI count;
        Image gauge;

        public RectTransform DealPoint { get; private set; }

        public static ShoeView Create(Transform parent, Vector2 position)
        {
            var rt = Ui.Rect(parent, "Shoe", position, BoxSize);
            var view = rt.gameObject.AddComponent<ShoeView>();
            Ui.Panel(rt, "Body", Vector2.zero, BoxSize, new Color(0.05f, 0.04f, 0.03f, 0.95f), 14);
            Ui.Outline(rt, "Trim", Vector2.zero, BoxSize, Theme.Gold.WithAlpha(0.8f), 14);
            Ui.Image(rt, "TopCard", Art.CardBack, Color.white, new Vector2(-34, 6), new Vector2(46, 64)).rectTransform.localEulerAngles = new Vector3(0, 0, 90);
            Ui.Text(rt, "Label", "SHOE", Fonts.Label, 18, Theme.Gold, new Vector2(30, 24), new Vector2(80, 22));
            view.count = Ui.Text(rt, "Count", "", Fonts.Label, 26, Theme.Ivory, new Vector2(30, -2), new Vector2(80, 30));
            Ui.Panel(rt, "GaugeBack", new Vector2(0, -34), new Vector2(BoxSize.x - 24, 8), new Color(1, 1, 1, 0.12f), 4);
            view.gauge = Ui.Panel(rt, "Gauge", new Vector2(0, -34), new Vector2(BoxSize.x - 24, 8), Theme.Gold, 4);
            view.gauge.type = Image.Type.Filled;
            view.gauge.fillMethod = Image.FillMethod.Horizontal;
            view.DealPoint = Ui.Rect(rt, "DealPoint", new Vector2(-34, 6), CardView.Size);
            return view;
        }

        public void SetRemaining(int remaining, int total)
        {
            count.text = remaining.ToString();
            gauge.fillAmount = total > 0 ? remaining / (float)total : 0;
        }
    }
}
