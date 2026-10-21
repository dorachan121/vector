using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>テーブル中央の進行表示（「PLACE YOUR BETS」など）と、残り時間のリング。</summary>
    public sealed class StatusBanner : MonoBehaviour
    {
        static readonly Vector2 BannerSize = new Vector2(620, 54);

        TextMeshProUGUI title;
        TextMeshProUGUI detail;
        Image ring;
        TextMeshProUGUI seconds;
        int lastWholeSecond = -1;

        public static StatusBanner Create(Transform parent, Vector2 position)
        {
            var rt = Ui.Rect(parent, "Status", position, BannerSize);
            var view = rt.gameObject.AddComponent<StatusBanner>();
            Ui.Panel(rt, "Plate", Vector2.zero, BannerSize, new Color(0, 0, 0, 0.55f), 27);
            view.title = Ui.Text(rt, "Title", "", Fonts.Label, 28, Theme.GoldBright, new Vector2(-30, 7), new Vector2(520, 30));
            view.detail = Ui.Text(rt, "Detail", "", Fonts.Body, 17, Theme.Ivory.WithAlpha(0.85f), new Vector2(-30, -15), new Vector2(520, 22));
            var timer = Ui.Rect(rt, "Timer", new Vector2(BannerSize.x / 2 - 30, 0), new Vector2(44, 44));
            Ui.Image(timer, "Track", Art.Ring(0.14f), new Color(1, 1, 1, 0.15f), Vector2.zero, new Vector2(44, 44));
            view.ring = Ui.Image(timer, "Ring", Art.Ring(0.14f), Theme.Gold, Vector2.zero, new Vector2(44, 44));
            view.ring.type = Image.Type.Filled;
            view.ring.fillMethod = Image.FillMethod.Radial360;
            view.ring.fillOrigin = (int)Image.Origin360.Top;
            view.ring.fillClockwise = false;
            view.seconds = Ui.Text(timer, "Seconds", "", Fonts.Label, 20, Theme.Ivory, Vector2.zero, new Vector2(44, 44));
            view.HideTimer();
            return view;
        }

        public void Show(string titleText, string detailText = "")
        {
            title.text = titleText;
            detail.text = detailText;
            title.rectTransform.anchoredPosition = new Vector2(-30, string.IsNullOrEmpty(detailText) ? 0 : 7);
        }

        /// <summary>残り時間を表示する。秒が切り替わったら true（カウントダウン音用）。</summary>
        public bool SetTimer(float remaining, float total)
        {
            ring.transform.parent.gameObject.SetActive(true);
            ring.fillAmount = total > 0 ? Mathf.Clamp01(remaining / total) : 0;
            ring.color = remaining <= 5 ? Theme.DealerRed : Theme.Gold;
            int whole = Mathf.CeilToInt(remaining);
            bool changed = whole != lastWholeSecond;
            // 文字列の生成は秒が変わったときだけ（毎フレームのガベージを出さない）
            if (changed)
                seconds.text = whole.ToString();
            lastWholeSecond = whole;
            return changed;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void HideTimer()
        {
            ring.transform.parent.gameObject.SetActive(false);
            lastWholeSecond = -1;
        }
    }
}
