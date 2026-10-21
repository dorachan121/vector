using System;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ホーム画面からの移動（テーブルへの着席・ストーリーモード・タブの切り替え）と、状態が変わったことの通知の窓口。
    /// タブはこれを通して操作し、互いや LobbyScene を直接は知らない。
    /// </summary>
    public sealed class HomeNavigator
    {
        readonly Action<HomeTabId> selectTab;
        readonly Action changed;

        public HomeNavigator(Action<HomeTabId> selectTab, Action changed)
        {
            this.selectTab = selectTab;
            this.changed = changed;
        }

        public void SelectTab(HomeTabId tab) => selectTab(tab);

        /// <summary>所持金・ポイント・進み具合が変わった（ヘッダーとバッジを更新する）。</summary>
        public void Changed() => changed();

        public void Play(GameId game, GameMode mode = GameMode.Player)
        {
            GameContext.SelectedMode = mode;
            GameContext.ReturnScene = SceneNames.Lobby;
            SceneFlow.Load(GameCatalog.Of(game).Scene);
        }

        public void EnterStory() => SceneFlow.Load(SceneNames.World);
    }
}
