using System;
using TMPro;
using UnityEngine;
using Vector.Core;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Table
{
    /// <summary>画面上部のバー（ロビーへ戻る・テーブル情報・所持金・ヘルプ・設定）。</summary>
    public sealed class TableHud : MonoBehaviour
    {
        const float Height = 84f;
        static readonly Color ButtonColor = new Color(1, 1, 1, 0.08f);

        TextMeshProUGUI balance;
        TextMeshProUGUI round;
        CasinoButton lobby;
        long shownBalance;

        public event Action LobbyClicked;
        public event Action HelpClicked;
        public event Action SettingsClicked;

        public static TableHud Create(Transform parent, GameMode mode, TableRules rules)
        {
            var rt = Ui.Rect(parent, "Hud", new Vector2(0, Ui.ReferenceResolution.y / 2 - Height / 2), new Vector2(Ui.ReferenceResolution.x, Height));
            var hud = rt.gameObject.AddComponent<TableHud>();
            Ui.Image(rt, "Fade", Art.VerticalFade, new Color(0, 0, 0, 0.85f), Vector2.zero, new Vector2(Ui.ReferenceResolution.x, Height));

            float left = -Ui.ReferenceResolution.x / 2;
            hud.lobby = Ui.Button(rt, "Lobby", "< LOBBY", new Vector2(left + 100, 0), new Vector2(150, 50), ButtonColor, Theme.Ivory, () => hud.LobbyClicked?.Invoke(), 24);
            Ui.Text(rt, "Logo", "VECTOR", Fonts.Display, 44, Theme.Gold, new Vector2(left + 300, 2), new Vector2(200, 60), TextAlignmentOptions.Left);
            Ui.Text(rt, "Mode", Labels.Mode(mode), Fonts.Label, 20, Theme.Muted, new Vector2(left + 445, -2), new Vector2(200, 30), TextAlignmentOptions.Left);

            Ui.Text(rt, "Limits", $"TABLE LIMITS  {Labels.Money(rules.MinBet)} – {Labels.Money(rules.MaxBet)}", Fonts.Label, 22, Theme.Ivory.WithAlpha(0.8f), new Vector2(-40, 10), new Vector2(520, 30));
            hud.round = Ui.Text(rt, "Round", "", Fonts.Label, 18, Theme.Muted, new Vector2(-40, -16), new Vector2(520, 24));

            float right = Ui.ReferenceResolution.x / 2;
            Ui.Text(rt, "BalanceLabel", "BALANCE", Fonts.Label, 18, Theme.Muted, new Vector2(right - 430, 14), new Vector2(240, 24), TextAlignmentOptions.Right);
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

        public void SetRoundInfo(int shoeNumber, int roundNumber) => round.text = $"SHOE #{shoeNumber}  ·  ROUND {roundNumber}";

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
