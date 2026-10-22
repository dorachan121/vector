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
    /// ホーム画面（縦向き）の起点。カジノのロビーの背景・上部のヘッダー・下のタブ・5つのタブの画面を組み立て、
    /// 起動して最初の1回だけタイトルを出す。タイトルが明けたら、読み込み時のお知らせと今日のログインボーナスを出す。
    /// </summary>
    public sealed class LobbyScene : MonoBehaviour
    {
        readonly Dictionary<HomeTabId, IHomeTab> tabs = new Dictionary<HomeTabId, IHomeTab>();
        ProfileService profiles;
        StoryProgress story;
        RectTransform root;
        RectTransform backdrop;
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
            backdrop = HomeBackdrop.Create(root);
            var navigator = new HomeNavigator(SelectTab, () => Refresh(true));
            var menus = new HomeMenus(() => NoticePopup.Show(root), ShowLoginCalendar, ShowMenu);
            var area = Ui.Between(root, "Tabs", HomeHeader.Height, BottomNav.Height, Ui.Portrait.x);
            tabs[HomeTabId.Home] = HomeTab.Create(area, story.Session, navigator, menus);
            tabs[HomeTabId.Story] = StoryTab.Create(area, story, navigator, root);
            tabs[HomeTabId.Play] = PlayTab.Create(area, navigator);
            tabs[HomeTabId.Missions] = MissionsTab.Create(area, story, navigator, root);
            tabs[HomeTabId.Shop] = ShopTab.Create(area, story, profiles, GameContext.Rules.MinBet, navigator);
            header = HomeHeader.Create(root, () => SelectTab(HomeTabId.Shop));
            nav = BottomNav.Create(root);
            nav.Selected += SelectTab;
            toast = Toast.Create(root, 400);
        }

        void SelectTab(HomeTabId id)
        {
            current = id;
            // ロビーの背景はホームだけ。ほかのタブは暗い地に中身を見せる
            backdrop.gameObject.SetActive(id == HomeTabId.Home);
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
            LoginBonusPopup.Show(root, story.Session.Daily.ClaimedThisCycle, grant, story.State.level, null);
        }

        /// <summary>ホームの「ログボ」: 今の巡の暦を見せる（受け取りは来場時に済んでいる）。</summary>
        void ShowLoginCalendar()
        {
            var dialog = Dialog.Open(root, "LOGIN BONUS", null, new Vector2(960, 800));
            float top = dialog.Content.sizeDelta.y / 2;
            LoginCalendar.Create(dialog.Content, new Vector2(0, top - 290), story.Session.Daily.ClaimedThisCycle, false);
            Ui.Text(dialog.Content, "Note", "1日1回、ホームに来たときに受け取れます。休んでもリセットされません。", Fonts.Body, 22, Palette.Muted, new Vector2(0, top - 580), new Vector2(900, 30));
            dialog.AddButton("閉じる", null, true);
        }

        void ShowMenu() => MenuPopup.Show(root, () => HelpDialog.Open(root, LobbyHelp.Pages()),
            () => SettingsDialog.Open(root, profiles, GameContext.ApplySettings), () => TitleSplash.Show(root, null));

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
