using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.Persistence;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ロビー（タイトル）シーンの起点。席の選択、所持金と成績の表示、遊び方・設定・キャッシャーを扱う。
    /// </summary>
    public sealed class LobbyScene : MonoBehaviour
    {
        static readonly Color ButtonColor = new Color(1, 1, 1, 0.08f);

        ProfileService profiles;
        TableRules rules;
        RectTransform root;
        TextMeshProUGUI balance;
        TextMeshProUGUI stats;
        CasinoButton cashier;
        Toast toast;

        void Start()
        {
            Ui.EnsureEventSystem();
            profiles = GameContext.Profiles;
            rules = GameContext.Rules;
            Build();
            Refresh();
            ShowLoadNotice();
        }

        void Build()
        {
            var canvas = Ui.CreateCanvas("LobbyCanvas", 0);
            root = Ui.Stretch(canvas.transform, "Lobby");
            var backdrop = Ui.Image(root, "Backdrop", Art.Backdrop, Color.white, Vector2.zero, Vector2.zero);
            backdrop.rectTransform.anchorMin = Vector2.zero;
            backdrop.rectTransform.anchorMax = Vector2.one;
            Ui.Image(root, "Felt", Art.Table, new Color(1, 1, 1, 0.35f), new Vector2(0, -40), new Vector2(1870, 870));

            BuildTitle();
            SeatCard.Create(root, new Vector2(-340, -120), "PLAYER SEAT",
                "自分でカードを置き、ディーラーと勝負する",
                $"MIN {Labels.Money(rules.MinBet)}  ·  MAX {Labels.Money(rules.MaxBet)}  ·  ALL LINES {rules.MainPays}",
                "TAKE A SEAT", Theme.Gold, () => Sit(GameMode.Player));
            SeatCard.Create(root, new Vector2(340, -120), "AUDIENCE",
                "AIプレイヤーとディーラーの勝負に賭ける（バックベット）",
                $"PLAYER {rules.BackPlayerPays}  ·  DEALER {rules.BackDealerPays}  ·  TIE {rules.BackTiePays}",
                "WATCH & BET", Theme.PlayerBlue, () => Sit(GameMode.Audience));
            BuildFooter();
            toast = Toast.Create(root, new Vector2(0, 470));
        }

        void BuildTitle()
        {
            Ui.Image(root, "LogoGlow", Art.Glow, Theme.Gold.WithAlpha(0.22f), new Vector2(0, 300), new Vector2(1100, 360));
            var logo = Ui.Text(root, "Logo", "VECTOR", Fonts.Display, 210, Theme.Gold, new Vector2(0, 318), new Vector2(1200, 230));
            logo.characterSpacing = 18;
            logo.enableVertexGradient = true;
            logo.colorGradient = new VertexGradient(Theme.GoldBright, Theme.GoldBright, Theme.GoldDeep, Theme.GoldDeep);
            Ui.Text(root, "Tagline", "THE CASINO GAME OF LINES", Fonts.Label, 30, Theme.Ivory, new Vector2(0, 186), new Vector2(1000, 40)).characterSpacing = 22;
            Ui.Text(root, "Catch", "縦・横・斜め。勝ち筋は、ひとつじゃない。", Fonts.Body, 28, Theme.Muted, new Vector2(0, 136), new Vector2(1000, 40));
            var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            for (int i = 0; i < suits.Length; i++)
                Ui.Image(root, "Suit" + i, Art.Suit(suits[i]), Theme.Gold.WithAlpha(0.8f), new Vector2((i - 1.5f) * 54, 88), new Vector2(30, 30));
        }

        void BuildFooter()
        {
            const float y = -400;
            Ui.Panel(root, "Bar", new Vector2(0, y), new Vector2(1500, 120), new Color(0, 0, 0, 0.55f), 24);
            Ui.Text(root, "BalanceLabel", "BALANCE", Fonts.Label, 20, Theme.Muted, new Vector2(-560, y + 26), new Vector2(300, 26), TextAlignmentOptions.Left);
            balance = Ui.Text(root, "Balance", "", Fonts.Display, 50, Theme.GoldBright, new Vector2(-560, y - 10), new Vector2(300, 60), TextAlignmentOptions.Left);
            stats = Ui.Text(root, "Stats", "", Fonts.Label, 20, Theme.Ivory.WithAlpha(0.8f), new Vector2(-150, y), new Vector2(460, 80), TextAlignmentOptions.Left);
            stats.textWrappingMode = TextWrappingModes.Normal;
            Ui.Button(root, "Help", "HOW TO PLAY", new Vector2(270, y), new Vector2(190, 64), ButtonColor, Theme.Ivory, () => HelpDialog.Open(root, rules), 24);
            Ui.Button(root, "Settings", "SETTINGS", new Vector2(470, y), new Vector2(170, 64), ButtonColor, Theme.Ivory,
                () => SettingsDialog.Open(root, profiles, GameContext.ApplySettings), 24);
            cashier = Ui.Button(root, "Cashier", "CASHIER", new Vector2(655, y), new Vector2(160, 64), Theme.Gold, Theme.Ink, Reload, 24);
            Ui.Text(root, "Disclaimer", "このゲームのチップは遊戯用です。現金・景品とは交換できません。  PLAY MONEY ONLY — NO REAL-MONEY GAMBLING.",
                Fonts.Body, 17, Theme.Muted.WithAlpha(0.8f), new Vector2(0, -508), new Vector2(1600, 26));
        }

        void Refresh()
        {
            var profile = profiles.Profile;
            balance.text = Labels.Money(profile.balance);
            var s = profile.stats;
            stats.text = s.roundsPlayed == 0
                ? "ようこそ。遊戯用チップ " + Labels.Money(PlayerProfile.StartingBalance) + " でスタートします。"
                : $"ROUNDS {s.roundsPlayed}   WON {s.roundsWon}   LOST {s.roundsLost}   PUSH {s.roundsPushed}\n" +
                  $"NET {Labels.SignedMoney(s.Net)}   BEST WIN {Labels.Money(s.biggestWin)}";
            cashier.Interactable = profile.balance < rules.MinBet;
        }

        void ShowLoadNotice()
        {
            if (profiles.RefundedStake > 0)
                toast.Show($"前回中断したラウンドの賭け金 {Labels.Money(profiles.RefundedStake)} を返金しました", Theme.GoldBright);
            else if (profiles.LoadResult == ProfileLoadResult.RestoredFromBackup)
                toast.Show("セーブデータをバックアップから復旧しました", Theme.GoldBright);
            else if (profiles.LoadResult == ProfileLoadResult.ResetAfterCorruption)
                toast.Show("セーブデータが読めなかったため、初期状態から始めます", Theme.Negative);
            profiles.ClearLoadNotice();
        }

        void Reload()
        {
            long added = profiles.Reload(rules.MinBet);
            if (added <= 0)
                return;
            Sfx.Play(SfxKind.ChipsCollect);
            toast.Show($"遊戯用チップを {Labels.Money(added)} 補充しました", Theme.GoldBright);
            Refresh();
        }

        void Sit(GameMode mode)
        {
            GameContext.SelectedMode = mode;
            SceneManager.LoadScene(SceneNames.Table);
        }
    }
}
