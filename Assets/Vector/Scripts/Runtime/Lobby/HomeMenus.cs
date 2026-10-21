using System;

namespace Vector.Game.Lobby
{
    /// <summary>ホームの横のメニューから開く画面（お知らせ・ログインボーナスの暦・メニュー）。開き方は LobbyScene が決める。</summary>
    public sealed class HomeMenus
    {
        public HomeMenus(Action notices, Action loginCalendar, Action menu)
        {
            Notices = notices;
            LoginCalendar = loginCalendar;
            Menu = menu;
        }

        public Action Notices { get; }
        public Action LoginCalendar { get; }
        public Action Menu { get; }
    }
}
