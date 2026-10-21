using TMPro;
using UnityEngine;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>勝敗の大きな表示（勝者・得点・収支・ボーナス役）。</summary>
    public sealed class ResultBanner : MonoBehaviour
    {
        static readonly Vector2 BannerSize = new Vector2(620, 172);

        CanvasGroup group;
        TextMeshProUGUI headline;
        TextMeshProUGUI scores;
        TextMeshProUGUI net;
        TextMeshProUGUI bonus;

        public static ResultBanner Create(Transform parent, Vector2 position)
        {
            var rt = Ui.Rect(parent, "Result", position, BannerSize);
            var view = rt.gameObject.AddComponent<ResultBanner>();
            view.group = Ui.Group(rt.gameObject);
            view.group.blocksRaycasts = false;
            Ui.Panel(rt, "Plate", Vector2.zero, BannerSize, new Color(0.02f, 0.03f, 0.03f, 0.9f), 26);
            Ui.Outline(rt, "Outline", Vector2.zero, BannerSize, Theme.Gold.WithAlpha(0.8f), 26);
            view.headline = Ui.Text(rt, "Headline", "", Fonts.Display, 54, Theme.Ivory, new Vector2(0, 46), new Vector2(BannerSize.x, 64));
            view.scores = Ui.Text(rt, "Scores", "", Fonts.Label, 24, Theme.Ivory, new Vector2(0, 4), new Vector2(BannerSize.x, 32));
            view.net = Ui.Text(rt, "Net", "", Fonts.Display, 34, Theme.Positive, new Vector2(0, -36), new Vector2(BannerSize.x, 44));
            view.bonus = Ui.Text(rt, "Bonus", "", Fonts.Label, 17, Theme.GoldBright, new Vector2(0, -68), new Vector2(BannerSize.x, 24));
            view.group.alpha = 0;
            rt.gameObject.SetActive(false);
            return view;
        }

        public void Show(RoundResult result, bool hasBonusBet)
        {
            gameObject.SetActive(true);
            headline.text = Labels.Winner(result.Winner);
            headline.color = Theme.ColorFor(result.Winner);
            scores.text = $"PLAYER {Labels.Family(result.PlayerFamily)} {result.PlayerScore}   —   DEALER {Labels.Family(result.DealerFamily)} {result.DealerScore}";
            net.text = result.Net == 0 ? "PUSH  $0" : Labels.SignedMoney(result.Net);
            net.color = result.Net > 0 ? Theme.Positive : result.Net < 0 ? Theme.Negative : Theme.Ivory;
            bonus.text = hasBonusBet ? "BONUS: " + Labels.HandName(result.BonusHand) : "";
            transform.localScale = Vector3.one * 0.85f;
            Tween.Fade(group, 1, 0.25f);
            Tween.Scale(transform, Vector3.one, 0.35f, Ease.OutBack);
        }

        public void Hide()
        {
            Tween.Fade(group, 0, 0.2f, Ease.OutQuad, () => gameObject.SetActive(false));
        }
    }
}
