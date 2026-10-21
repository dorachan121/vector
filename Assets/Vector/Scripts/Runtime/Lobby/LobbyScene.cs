using TMPro;
using UnityEngine;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.Persistence;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ロビー（タイトル）シーンの起点。ストーリーモード（RPG）への入口とゲームの選択、所持金と成績の表示、
    /// 遊び方・設定・キャッシャーを扱う。
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
            root = Ui.CreateScreen("Lobby");
            Ui.Image(root, "Felt", Art.Table, new Color(1, 1, 1, 0.35f), new Vector2(0, -40), new Vector2(1870, 870));

            BuildTitle();
            BuildStoryBanner();
            BuildGames();
            BuildFooter();
            toast = Toast.Create(root, new Vector2(0, 470));
        }

        void BuildTitle()
        {
            Ui.Image(root, "LogoGlow", Art.Glow, Theme.Gold.WithAlpha(0.22f), new Vector2(0, 366), new Vector2(900, 260));
            var logo = Ui.Text(root, "Logo", "VECTOR", Fonts.Display, 140, Theme.Gold, new Vector2(0, 372), new Vector2(1000, 150));
            logo.characterSpacing = 18;
            logo.enableVertexGradient = true;
            logo.colorGradient = new VertexGradient(Theme.GoldBright, Theme.GoldBright, Theme.GoldDeep, Theme.GoldDeep);
            Ui.Text(root, "Tagline", "CASINO RESORT  ·  SIX ORIGINAL GAMES", Fonts.Label, 26, Theme.Ivory, new Vector2(0, 282), new Vector2(1200, 36)).characterSpacing = 14;
            var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            for (int i = 0; i < suits.Length; i++)
                Ui.Image(root, "Suit" + i, Art.Suit(suits[i]), Theme.Gold.WithAlpha(0.8f), new Vector2((i < 2 ? -1 : 1) * (430 + (i % 2) * 50), 372), new Vector2(30, 30));
        }

        /// <summary>ストーリーモード（カジノの中を歩く RPG）への入口。</summary>
        void BuildStoryBanner()
        {
            var size = new Vector2(1640, 104);
            var banner = Ui.Rect(root, "Story", new Vector2(0, 188), size);
            Ui.Image(banner, "Glow", Art.Glow, Theme.GoldBright.WithAlpha(0.12f), Vector2.zero, size * 1.2f);
            Ui.Panel(banner, "Plate", Vector2.zero, size, new Color(0.09f, 0.06f, 0.02f, 0.94f), 24);
            Ui.Outline(banner, "Outline", Vector2.zero, size, Theme.GoldBright.WithAlpha(0.9f), 24);
            Ui.Text(banner, "Title", "STORY MODE", Fonts.Display, 46, Theme.GoldBright, new Vector2(-640, 0), new Vector2(320, 60), TextAlignmentOptions.Left);
            Ui.Paragraph(banner, "Pitch", "新しく開業したカジノリゾートで、フロアの主たちに挑み、最上階のオーナーを目指す。\nカジノの中を歩き、人と話し、テーブルで腕を磨くカジノ RPG。",
                20, Theme.Ivory, new Vector2(40, 0), new Vector2(820, 70), TextAlignmentOptions.Left);
            Ui.Button(banner, "Play", "PLAY STORY", new Vector2(size.x / 2 - 150, 0), new Vector2(240, 64), Theme.GoldBright, Theme.Ink, OpenStory, 28, 24);
        }

        void BuildGames()
        {
            var games = GameCatalog.All;
            for (int i = 0; i < games.Count; i++)
            {
                var entry = games[i];
                var position = new Vector2((i % 3 - 1) * (GameCard.Size.x + 30), 16 - i / 3 * (GameCard.Size.y + 26));
                if (entry.Id == GameId.Vector)
                    GameCard.Create(root, position, entry, ("PLAYER", () => SitVector(GameMode.Player)), ("AUDIENCE", () => SitVector(GameMode.Audience)));
                else
                    GameCard.Create(root, position, entry, ("TAKE A SEAT", () => Sit(entry.Scene)));
            }
        }

        void BuildFooter()
        {
            const float y = -400;
            Ui.Panel(root, "Bar", new Vector2(0, y), new Vector2(1500, 120), new Color(0, 0, 0, 0.55f), 24);
            Ui.Text(root, "BalanceLabel", "BALANCE", Fonts.Label, 20, Theme.Muted, new Vector2(-560, y + 26), new Vector2(300, 26), TextAlignmentOptions.Left);
            balance = Ui.Text(root, "Balance", "", Fonts.Display, 50, Theme.GoldBright, new Vector2(-560, y - 10), new Vector2(300, 60), TextAlignmentOptions.Left);
            stats = Ui.Text(root, "Stats", "", Fonts.Label, 20, Theme.Ivory.WithAlpha(0.8f), new Vector2(-150, y), new Vector2(460, 80), TextAlignmentOptions.Left);
            stats.textWrappingMode = TextWrappingModes.Normal;
            Ui.Button(root, "Help", "HOW TO PLAY", new Vector2(270, y), new Vector2(190, 64), ButtonColor, Theme.Ivory, () => HelpDialog.Open(root, LobbyHelp.Pages()), 24);
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

        void SitVector(GameMode mode)
        {
            GameContext.SelectedMode = mode;
            Sit(SceneNames.Table);
        }

        void Sit(string scene)
        {
            GameContext.ReturnScene = SceneNames.Lobby;
            SceneFlow.Load(scene);
        }

        void OpenStory() => SceneFlow.Load(SceneNames.World);

        /// <summary>Esc（Android の戻るボタン）で終了の確認を出す。</summary>
        void Update()
        {
            if (!Dialog.IsAnyOpen && Input.GetKeyDown(KeyCode.Escape))
                ConfirmQuit();
        }

        void ConfirmQuit()
        {
            Dialog.Open(root, "QUIT GAME", "ゲームを終了しますか？", new Vector2(600, 260))
                .AddButton("CANCEL", null)
                .AddButton("QUIT", Application.Quit, true);
        }
    }
}
