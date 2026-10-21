using System.Collections.Generic;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Persistence;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ホーム画面（縦向き）の起点。上部のヘッダー・下のタブ・5つのタブの画面を組み立て、起動して最初の1回だけタイトルを出す。
    /// タイトルが明けたら、読み込み時のお知らせと今日のログインボーナスを出す。
    /// </summary>
    public sealed class LobbyScene : MonoBehaviour
    {
        readonly Dictionary<HomeTabId, IHomeTab> tabs = new Dictionary<HomeTabId, IHomeTab>();
        ProfileService profiles;
        StoryProgress story;
        RectTransform root;
        HomeHeader header;
        BottomNav nav;
        Toast toast;
        HomeTabId current;

        void Start()
        {
            profiles = GameContext.Profiles;
            story = GameContext.Story;
            Build();
            SelectTab(HomeTabId.Home);
            Refresh(false);
            if (GameContext.TitleShown)
                Welcome();
            else
            {
                GameContext.TitleShown = true;
                TitleSplash.Show(root, Welcome);
            }
        }

        void Build()
        {
            root = Ui.CreateScreen("Lobby", ScreenShape.Portrait);
            Ui.Image(root, "Glow", Art.Glow, Palette.Haze.WithAlpha(0.35f), new Vector2(0, 200), new Vector2(1400, 1600));
            var navigator = new HomeNavigator(SelectTab, () => Refresh(true));
            var area = Ui.Between(root, "Tabs", HomeHeader.Height, BottomNav.Height, Ui.Portrait.x);
            tabs[HomeTabId.Home] = HomeTab.Create(area, story.Session, navigator);
            tabs[HomeTabId.Games] = GamesTab.Create(area, navigator);
            tabs[HomeTabId.Story] = StoryTab.Create(area, story.Session, navigator);
            tabs[HomeTabId.Missions] = MissionsTab.Create(area, story, navigator, root);
            tabs[HomeTabId.Shop] = ShopTab.Create(area, story, profiles, GameContext.Rules.MinBet, navigator);
            header = HomeHeader.Create(root, () => SelectTab(HomeTabId.Shop), () => HelpDialog.Open(root, LobbyHelp.Pages()),
                () => SettingsDialog.Open(root, profiles, GameContext.ApplySettings));
            nav = BottomNav.Create(root);
            nav.Selected += SelectTab;
            toast = Toast.Create(root, new Vector2(0, 560));
        }

        void SelectTab(HomeTabId id)
        {
            current = id;
            foreach (var pair in tabs)
                pair.Value.Root.gameObject.SetActive(pair.Key == id);
            nav.SetActive(id);
            var tab = tabs[id].Root;
            tabs[id].Refresh();
            // 切り替えたタブは少し下から浮かび上がる
            tab.anchoredPosition = new Vector2(0, -30);
            Tween.Move(tab, Vector2.zero, 0.25f, Ease.OutCubic);
        }

        /// <summary>所持金・ポイント・進み具合が変わったとき（ヘッダー・バッジ・表示中のタブを最新にする）。</summary>
        void Refresh(bool animate)
        {
            header.Refresh(story.State, profiles.Profile.balance, animate);
            var session = story.Session;
            nav.SetBadge(HomeTabId.Missions, session.Daily.Claimable);
            bool ready = false;
            foreach (var floor in FloorOrder.BottomUp)
                ready |= session.StatusOf(floor) == FloorStatus.Ready;
            nav.SetAlert(HomeTabId.Story, ready);
            if (animate)
                tabs[current].Refresh();
        }

        void Welcome()
        {
            ShowLoadNotice();
            var grant = story.Session.Daily.ClaimLogin();
            if (grant == null)
                return;
            story.Save();
            Refresh(true);
            int day = story.Session.Daily.ClaimedThisCycle;
            RewardPopup.Show(root, "LOGIN BONUS", RewardDisplay.Message($"{day}日目のログインボーナスです。今日もごゆっくりどうぞ。", grant, story.State.level), RewardDisplay.From(grant));
        }

        void ShowLoadNotice()
        {
            if (profiles.RefundedStake > 0)
                toast.Show($"前回中断したラウンドの賭け金 {Labels.Money(profiles.RefundedStake)} を返金しました", Palette.GoldLight);
            else if (profiles.LoadResult == ProfileLoadResult.RestoredFromBackup)
                toast.Show("セーブデータをバックアップから復旧しました", Palette.GoldLight);
            else if (profiles.LoadResult == ProfileLoadResult.ResetAfterCorruption)
                toast.Show("セーブデータが読めなかったため、初期状態から始めます", Theme.Negative);
            profiles.ClearLoadNotice();
        }

        /// <summary>Esc（Android の戻るボタン）で、ホーム以外のタブならホームへ、ホームなら終了の確認を出す。</summary>
        void Update()
        {
            if (Dialog.IsAnyOpen || !Input.GetKeyDown(KeyCode.Escape))
                return;
            if (current != HomeTabId.Home)
                SelectTab(HomeTabId.Home);
            else
                ConfirmQuit();
        }

        void ConfirmQuit()
        {
            Dialog.Open(root, "QUIT GAME", "ゲームを終了しますか？", new Vector2(640, 300))
                .AddButton("CANCEL", null)
                .AddButton("QUIT", Application.Quit, true);
        }
    }
}
