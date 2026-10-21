using UnityEngine;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>ゲームのタブ。6つのゲームのタイルを2列に並べ、押すとそのテーブルに着く。VECTOR は自分で打つ席と観戦して賭ける席を選べる。</summary>
    public sealed class GamesTab : IHomeTab
    {
        const float Gap = 26;

        GamesTab(Transform parent)
        {
            Root = Ui.Stretch(parent, "GamesTab");
            Body = HomeLayout.Top(Root);
        }

        public RectTransform Root { get; }

        /// <summary>中身の土台（上端に寄せる）。</summary>
        RectTransform Body { get; }

        public static GamesTab Create(Transform parent, HomeNavigator nav)
        {
            var tab = new GamesTab(parent);
            Ui.Ribbon(tab.Body, "Title", "CASINO FLOOR", new Vector2(0, 706), 560, Palette.Ruby, 36);
            var games = GameCatalog.All;
            for (int i = 0; i < games.Count; i++)
            {
                var entry = games[i];
                var position = new Vector2((i % 2 - 0.5f) * (GameTile.Size.x + Gap), 395 - i / 2 * (GameTile.Size.y + Gap));
                if (entry.Id == GameId.Vector)
                    GameTile.Create(tab.Body, position, entry, ("PLAYER", () => nav.Play(entry.Id)), ("AUDIENCE", () => nav.Play(entry.Id, GameMode.Audience)));
                else
                    GameTile.Create(tab.Body, position, entry, ("PLAY", () => nav.Play(entry.Id)));
            }
            return tab;
        }

        public void Refresh()
        {
        }
    }
}
