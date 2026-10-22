using UnityEngine;
using Vector.Core;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// プレイ。ゲームの大きなカードを横に送って選び、大きな PLAY で着席する。VECTOR は自分で打つ席と観戦して賭ける席を選べる。
    /// </summary>
    public sealed class PlayTab : IHomeTab
    {
        readonly HomeNavigator nav;
        GameCarousel carousel;
        SegmentedTabs seats;
        RectTransform seatRoot;

        PlayTab(Transform parent, HomeNavigator nav)
        {
            this.nav = nav;
            Root = Ui.Stretch(parent, "PlayTab");
            Body = HomeLayout.Top(Root);
            Cards = HomeLayout.Middle(Root);
            Controls = HomeLayout.Bottom(Root);
        }

        public RectTransform Root { get; }

        /// <summary>ページの見出し（上端に寄せる）。</summary>
        RectTransform Body { get; }

        /// <summary>ゲームのカード（見出しと PLAY の間の真ん中に置く）。</summary>
        RectTransform Cards { get; }

        /// <summary>席の選択と PLAY（下端に寄せて、親指の届くところに置く）。</summary>
        RectTransform Controls { get; }

        public static PlayTab Create(Transform parent, HomeNavigator nav)
        {
            var tab = new PlayTab(parent, nav);
            PageTitle.Create(tab.Body, 728, "PLAY", "ゲームを選ぶ");
            tab.carousel = GameCarousel.Create(tab.Cards,new Vector2(0, 116), GameCatalog.All, -GameKeyCard.Size.y / 2 - 34);
            tab.carousel.Changed += _ => tab.Refresh();

            tab.seatRoot = Ui.Rect(tab.Controls, "Seats", new Vector2(0, -486), new Vector2(760, 90));
            tab.seats = SegmentedTabs.Create(tab.seatRoot, Vector2.zero, 760, "自分で打つ", "観戦して賭ける");
            tab.seats.Select(0);

            var start = Ui.Button(tab.Controls, "Start", "PLAY", new Vector2(0, -626), new Vector2(680, 140), Palette.Gold, Palette.Ink, tab.Sit, 70, 70);
            Ui.Icon(start.transform, "Icon", IconKind.Play, Palette.Ink, new Vector2(-170, 0), 64);
            Pulse.Add(start, 0.025f, 2f);
            return tab;
        }

        public void Refresh() => seatRoot.gameObject.SetActive(Current.Id == GameId.Vector);

        GameEntry Current => GameCatalog.All[carousel.Index];

        void Sit()
        {
            var mode = Current.Id == GameId.Vector && seats.Current == 1 ? GameMode.Audience : GameMode.Player;
            nav.Play(Current.Id, mode);
        }
    }
}
