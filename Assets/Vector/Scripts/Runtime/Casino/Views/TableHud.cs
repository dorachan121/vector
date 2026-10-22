using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Casino
{
    /// <summary>
    /// 画面上部のバー（戻る・ゲーム名・テーブル情報・所持金・ヘルプ・設定）。画面の上端に付き、横に長い端末でも左右のボタンは画面の端に寄る。
    /// </summary>
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
            // 画面の横幅いっぱいに広げる（左右の組は、それぞれ画面の左端・右端からの位置で置く）
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(0, Height);
            var hud = rt.gameObject.AddComponent<TableHud>();
            Ui.Image(rt, "Fade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0.95f), Palette.NightDeep.WithAlpha(0f)), Color.white, new Vector2(0, -10), new Vector2(w * 2, Height + 20));
            // 横幅のわりに縦が長い端末（タブレットなど）で画面の枠の上に余った所も、同じ暗さで続ける
            float fadeTop = -10 + (Height + 20) / 2;
            Ui.Image(rt, "FadeAbove", null, Palette.NightDeep.WithAlpha(0.95f), new Vector2(0, fadeTop + ScreenFrame.Bleed / 2), new Vector2(w * 2, ScreenFrame.Bleed));

            var left = Side(rt, "Left", 0);
            hud.lobby = Ui.IconButton(left, "Lobby", IconKind.Back, new Vector2(58, 2), 68, new Color(1, 1, 1, 0.1f), () => hud.LobbyClicked?.Invoke());
            var logo = Ui.Headline(left, "Logo", title, Fonts.Display, 46, new Vector2(300, 4), new Vector2(200, 60), TextAlignmentOptions.Left);
            // ゲーム名の長さに合わせて、右隣の小さな表示を詰めて置く
            float logoWidth = Mathf.Max(60, logo.GetPreferredValues(title).x);
            logo.rectTransform.sizeDelta = new Vector2(logoWidth, 60);
            logo.rectTransform.anchoredPosition = new Vector2(112 + logoWidth / 2, 4);
            Ui.Text(left, "Mode", subtitle, Fonts.Label, 20, Palette.Muted, new Vector2(127 + logoWidth + 100, 0), new Vector2(200, 30), TextAlignmentOptions.Left);

            hud.limits = Ui.Text(rt, "Limits", limits, Fonts.Label, 22, Palette.Ivory.WithAlpha(0.85f), new Vector2(-40, 12), new Vector2(620, 30));
            hud.round = Ui.Text(rt, "Round", "", Fonts.Label, 18, Palette.Muted, new Vector2(-40, -16), new Vector2(520, 24));

            var right = Side(rt, "Right", 1);
            hud.balance = CurrencyPill.Create(right, new Vector2(-380, 8), 320, Chrome.Coin, Color.white, Labels.Money);
            hud.balanceLabel = Ui.Text(right, "BalanceLabel", "PLAY MONEY", Fonts.Label, 15, Palette.Muted, new Vector2(-380, -32), new Vector2(300, 20));
            Ui.IconButton(right, "Help", IconKind.Help, new Vector2(-152, 2), 68, new Color(1, 1, 1, 0.1f), () => hud.HelpClicked?.Invoke());
            Ui.IconButton(right, "Settings", IconKind.Settings, new Vector2(-62, 2), 68, new Color(1, 1, 1, 0.1f), () => hud.SettingsClicked?.Invoke());
            return hud;
        }

        /// <summary>バーの左端（x = 0）か右端（x = 1）の高さの中ほどを原点にした、部品の置き場所。</summary>
        static RectTransform Side(RectTransform hud, string name, float x)
        {
            var rt = Ui.Rect(hud, name, Vector2.zero, Vector2.zero);
            rt.anchorMin = rt.anchorMax = new Vector2(x, 0.5f);
            return rt;
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
