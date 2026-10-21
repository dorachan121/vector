using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Casino
{
    /// <summary>画面上部のバー（ロビーへ戻る・テーブル情報・所持金・ヘルプ・設定）。</summary>
    public sealed class TableHud : MonoBehaviour
    {
        const float Height = 84f;
        static readonly Color ButtonColor = new Color(1, 1, 1, 0.08f);

        TextMeshProUGUI balance;
        TextMeshProUGUI round;
        TextMeshProUGUI limits;
        TextMeshProUGUI balanceLabel;
        CasinoButton lobby;
        Image progress;
        long shownBalance;

        public event Action LobbyClicked;
        public event Action HelpClicked;
        public event Action SettingsClicked;

        /// <param name="title">左上のゲーム名。</param>
        /// <param name="subtitle">ゲーム名の右の小さな表示（席など）。</param>
        /// <param name="limits">中央のテーブル情報（リミットなど）。</param>
        public static TableHud Create(Transform parent, string title, string subtitle, string limits)
        {
            var rt = Ui.Rect(parent, "Hud", new Vector2(0, Ui.ReferenceResolution.y / 2 - Height / 2), new Vector2(Ui.ReferenceResolution.x, Height));
            var hud = rt.gameObject.AddComponent<TableHud>();
            Ui.Image(rt, "Fade", Art.VerticalFade, new Color(0, 0, 0, 0.85f), Vector2.zero, new Vector2(Ui.ReferenceResolution.x, Height));

            float left = -Ui.ReferenceResolution.x / 2;
            hud.lobby = Ui.Button(rt, "Lobby", "< LOBBY", new Vector2(left + 100, 0), new Vector2(150, 50), ButtonColor, Theme.Ivory, () => hud.LobbyClicked?.Invoke(), 24);
            var logo = Ui.Text(rt, "Logo", title, Fonts.Display, 44, Theme.Gold, new Vector2(left + 300, 2), new Vector2(200, 60), TextAlignmentOptions.Left);
            // ゲーム名の長さに合わせて、右隣の小さな表示を詰めて置く
            float logoWidth = Mathf.Max(60, logo.GetPreferredValues(title).x);
            logo.rectTransform.sizeDelta = new Vector2(logoWidth, 60);
            logo.rectTransform.anchoredPosition = new Vector2(left + 200 + logoWidth / 2, 2);
            Ui.Text(rt, "Mode", subtitle, Fonts.Label, 20, Theme.Muted, new Vector2(left + 215 + logoWidth + 100, -2), new Vector2(200, 30), TextAlignmentOptions.Left);

            hud.limits = Ui.Text(rt, "Limits", limits, Fonts.Label, 22, Theme.Ivory.WithAlpha(0.8f), new Vector2(-40, 10), new Vector2(620, 30));
            hud.round = Ui.Text(rt, "Round", "", Fonts.Label, 18, Theme.Muted, new Vector2(-40, -16), new Vector2(520, 24));

            float right = Ui.ReferenceResolution.x / 2;
            hud.balanceLabel = Ui.Text(rt, "BalanceLabel", "BALANCE", Fonts.Label, 18, Theme.Muted, new Vector2(right - 430, 14), new Vector2(240, 24), TextAlignmentOptions.Right);
            hud.balance = Ui.Text(rt, "Balance", "", Fonts.Display, 40, Theme.GoldBright, new Vector2(right - 430, -12), new Vector2(240, 48), TextAlignmentOptions.Right);
            Ui.Button(rt, "Help", "HOW TO PLAY", new Vector2(right - 225, 0), new Vector2(150, 50), ButtonColor, Theme.Ivory, () => hud.HelpClicked?.Invoke(), 20);
            Ui.Button(rt, "Settings", "SETTINGS", new Vector2(right - 80, 0), new Vector2(120, 50), ButtonColor, Theme.Ivory, () => hud.SettingsClicked?.Invoke(), 20);
            return hud;
        }

        /// <summary>所持金を表示する。animate なら数字を数え上げる。</summary>
        public void SetBalance(long value, bool animate)
        {
            long from = shownBalance;
            shownBalance = value;
            if (!animate || from == value)
            {
                balance.text = Labels.Money(value);
                return;
            }
            Tween.Run(balance, "count", 0.8f, Ease.OutCubic, t => balance.text = Labels.Money((long)Math.Round(from + (value - from) * (double)t)));
        }

        /// <summary>テーブル情報の2行目（ラウンド番号など）を自由な文言にする。</summary>
        public void SetInfo(string text)
        {
            if (progress == null)
                round.text = text;
        }

        /// <summary>
        /// フロアの主の挑戦中の表示（中央に相手と目標、その下に進み具合のゲージ）。所持金の欄は挑戦用のチップになる。
        /// </summary>
        public void SetChallenge(string headline, string detail, float fraction)
        {
            if (progress == null)
            {
                var rt = (RectTransform)transform;
                Ui.Panel(rt, "ChallengeBack", new Vector2(-40, -36), new Vector2(520, 8), new Color(1, 1, 1, 0.15f), 4);
                progress = Ui.Panel(rt, "Challenge", new Vector2(-40, -36), new Vector2(520, 8), Theme.GoldBright, 4);
                progress.type = Image.Type.Filled;
                progress.fillMethod = Image.FillMethod.Horizontal;
                balanceLabel.text = "CHALLENGE CHIPS";
                limits.color = Theme.GoldBright;
            }
            limits.text = headline;
            round.text = detail;
            progress.fillAmount = Mathf.Clamp01(fraction);
        }

        public void SetLobbyEnabled(bool enabled) => lobby.Interactable = enabled;

        /// <summary>Esc（Android の戻るボタン）は LOBBY ボタンと同じ。</summary>
        void Update()
        {
            if (!Dialog.IsAnyOpen && lobby.Interactable && Input.GetKeyDown(KeyCode.Escape))
                LobbyClicked?.Invoke();
        }

        /// <summary>払い戻しのチップが飛んでいく先。</summary>
        public Vector3 BalanceWorldPosition => balance.transform.position;
    }
}
