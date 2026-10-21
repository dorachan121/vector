using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Casino
{
    /// <summary>画面上部のバー（戻る・ゲーム名・テーブル情報・所持金・ヘルプ・設定）。画面の上端に付く。</summary>
    public sealed class TableHud : MonoBehaviour
    {
        const float Height = 96f;

        CurrencyPill balance;
        TextMeshProUGUI round;
        TextMeshProUGUI limits;
        TextMeshProUGUI balanceLabel;
        CasinoButton lobby;
        Image progress;

        public event Action LobbyClicked;
        public event Action HelpClicked;
        public event Action SettingsClicked;

        /// <param name="title">左上のゲーム名。</param>
        /// <param name="subtitle">ゲーム名の右の小さな表示（席など）。</param>
        /// <param name="limits">中央のテーブル情報（リミットなど）。</param>
        public static TableHud Create(Transform parent, string title, string subtitle, string limits)
        {
            float w = Ui.Landscape.x;
            var rt = Ui.Edge(parent, "Hud", true, w, Height);
            var hud = rt.gameObject.AddComponent<TableHud>();
            Ui.Image(rt, "Fade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0.95f), Palette.NightDeep.WithAlpha(0f)), Color.white, new Vector2(0, -10), new Vector2(w * 2, Height + 20));

            float left = -w / 2;
            hud.lobby = Ui.IconButton(rt, "Lobby", IconKind.Back, new Vector2(left + 58, 2), 68, new Color(1, 1, 1, 0.1f), () => hud.LobbyClicked?.Invoke());
            var logo = Ui.Headline(rt, "Logo", title, Fonts.Display, 46, new Vector2(left + 300, 4), new Vector2(200, 60), TextAlignmentOptions.Left);
            // ゲーム名の長さに合わせて、右隣の小さな表示を詰めて置く
            float logoWidth = Mathf.Max(60, logo.GetPreferredValues(title).x);
            logo.rectTransform.sizeDelta = new Vector2(logoWidth, 60);
            logo.rectTransform.anchoredPosition = new Vector2(left + 112 + logoWidth / 2, 4);
            Ui.Text(rt, "Mode", subtitle, Fonts.Label, 20, Palette.Muted, new Vector2(left + 127 + logoWidth + 100, 0), new Vector2(200, 30), TextAlignmentOptions.Left);

            hud.limits = Ui.Text(rt, "Limits", limits, Fonts.Label, 22, Palette.Ivory.WithAlpha(0.85f), new Vector2(-40, 12), new Vector2(620, 30));
            hud.round = Ui.Text(rt, "Round", "", Fonts.Label, 18, Palette.Muted, new Vector2(-40, -16), new Vector2(520, 24));

            float right = w / 2;
            hud.balance = CurrencyPill.Create(rt, new Vector2(right - 380, 8), 320, Chrome.Coin, Color.white, Labels.Money);
            hud.balanceLabel = Ui.Text(rt, "BalanceLabel", "PLAY MONEY", Fonts.Label, 15, Palette.Muted, new Vector2(right - 380, -32), new Vector2(300, 20));
            Ui.IconButton(rt, "Help", IconKind.Help, new Vector2(right - 152, 2), 68, new Color(1, 1, 1, 0.1f), () => hud.HelpClicked?.Invoke());
            Ui.IconButton(rt, "Settings", IconKind.Settings, new Vector2(right - 62, 2), 68, new Color(1, 1, 1, 0.1f), () => hud.SettingsClicked?.Invoke());
            return hud;
        }

        /// <summary>所持金を表示する。animate なら数字を数え上げる。</summary>
        public void SetBalance(long value, bool animate) => balance.SetValue(value, animate);

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
                progress = Ui.Panel(rt, "Challenge", new Vector2(-40, -36), new Vector2(520, 8), Palette.GoldLight, 4);
                progress.type = Image.Type.Filled;
                progress.fillMethod = Image.FillMethod.Horizontal;
                balanceLabel.text = "CHALLENGE CHIPS";
                limits.color = Palette.GoldLight;
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
        public Vector3 BalanceWorldPosition => balance.Icon.position;
    }
}
