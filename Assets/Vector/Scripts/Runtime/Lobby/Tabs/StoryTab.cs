using System.Collections.Generic;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ストーリー。カジノの塔を下から登っていく地図（上の階ほど上）。各フロアの主の顔の札と進み具合、いま目指すフロアの印。
    /// フロアを押すと詳しい案内を出し、そこからフロアへ入れる。
    /// </summary>
    public sealed class StoryTab : IHomeTab
    {
        const float TopY = 548;
        const float Step = 180;
        const float Zig = 200;

        readonly StoryProgress story;
        readonly HomeNavigator nav;
        readonly Transform popups;
        readonly List<FloorNode> nodes = new List<FloorNode>();
        RectTransform marker;

        StoryTab(Transform parent, StoryProgress story, HomeNavigator nav, Transform popups)
        {
            this.story = story;
            this.nav = nav;
            this.popups = popups;
            Root = Ui.Stretch(parent, "StoryTab");
            Body = HomeLayout.Top(Root);
            Controls = HomeLayout.Bottom(Root);
        }

        public RectTransform Root { get; }

        /// <summary>中身の土台（上端に寄せる）。</summary>
        RectTransform Body { get; }

        /// <summary>カジノへ入るボタン（下端に寄せる）。</summary>
        RectTransform Controls { get; }

        /// <param name="popups">フロアの案内を開く画面。</param>
        public static StoryTab Create(Transform parent, StoryProgress story, HomeNavigator nav, Transform popups)
        {
            var tab = new StoryTab(parent, story, nav, popups);
            tab.BuildTower();
            var floors = FloorOrder.TopDown;
            var points = new Vector2[floors.Length];
            for (int i = 0; i < floors.Length; i++)
                points[i] = new Vector2(i == 0 ? 0 : (i % 2 == 1 ? Zig : -Zig), TopY - i * Step);
            tab.BuildPath(points);
            for (int i = 0; i < floors.Length; i++)
            {
                var floor = floors[i];
                int side = points[i].x > 0 ? -1 : 1;
                tab.nodes.Add(FloorNode.Create(tab.Body, floor, points[i], side, () => tab.Open(floor)));
            }
            // 揺れるのは中身だけにして、置き場所（marker）は Refresh で動かせるようにする
            tab.marker = Ui.Rect(tab.Body, "Marker", Vector2.zero, new Vector2(90, 90));
            var pin = Ui.Rect(tab.marker, "Pin", Vector2.zero, new Vector2(90, 90));
            Ui.Icon(pin, "Pin", IconKind.Story, Palette.Ruby, Vector2.zero, 84);
            Ui.Icon(pin, "Dot", IconKind.Star, Palette.GoldLight, new Vector2(0, 10), 30);
            Float.Add(pin, 12f, 2.4f);
            PageTitle.Create(tab.Body, 728, "STORY", "最上階のオーナーへ挑め");
            var enter = Ui.Button(tab.Controls, "Enter", "カジノへ入る", new Vector2(0, -640), new Vector2(680, 124), Palette.Gold, Palette.Ink, nav.EnterStory, 44, 62);
            enter.Label.font = Fonts.Body;
            Pulse.Add(enter, 0.025f, 2f);
            return tab;
        }

        public void Refresh()
        {
            var session = story.Session;
            FloorNode next = null;
            foreach (var node in nodes)
            {
                var status = session.StatusOf(node.Floor);
                node.Paint(status, Floors.Of(node.Floor).Master.RequiredLevel);
            }
            foreach (var floor in FloorOrder.BottomUp)
            {
                if (session.StatusOf(floor) != FloorStatus.Cleared)
                {
                    next = nodes.Find(n => n.Floor == floor);
                    break;
                }
            }
            marker.gameObject.SetActive(next != null);
            if (next != null)
                marker.anchoredPosition = next.Position + new Vector2(0, FloorNode.FaceSize / 2 + 36);
        }

        /// <summary>塔の絵（夜空の中のビル・窓の灯り・屋上の王冠）。</summary>
        void BuildTower()
        {
            Ui.Image(Body, "Sky", Chrome.Gradient(Theme.Hex("2A1060"), Palette.NightDeep.WithAlpha(0f)), Color.white, new Vector2(0, 100), new Vector2(Ui.Portrait.x * 3, 1500));
            var tower = new Vector2(0, -20);
            Ui.Image(Body, "Glow", Art.Glow, Palette.Magenta.WithAlpha(0.22f), tower, new Vector2(1000, 1500));
            Ui.Panel(Body, "Tower", tower, new Vector2(620, 1260), Theme.Hex("1E0C44").WithAlpha(0.9f), 40);
            Ui.Outline(Body, "TowerRim", tower, new Vector2(620, 1260), Palette.GoldMid.WithAlpha(0.5f), 40);
            for (int row = 0; row < 14; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    bool lit = (row * 7 + col * 3) % 5 != 0;
                    Ui.Panel(Body, "Window", tower + new Vector2((col - 2.5f) * 80, 560 - row * 86), new Vector2(36, 46),
                        lit ? Palette.GoldLight.WithAlpha(0.18f) : Palette.Violet.WithAlpha(0.25f), 6);
                }
            }
            var crown = Ui.Icon(Body, "Crown", IconKind.Crown, Palette.Gold, new Vector2(0, TopY + 120), 120);
            Float.Add(crown.rectTransform, 6f, 1.2f);
        }

        /// <summary>フロアどうしをつなぐ点線の道。</summary>
        void BuildPath(Vector2[] points)
        {
            for (int i = 0; i + 1 < points.Length; i++)
            {
                var from = points[i];
                var to = points[i + 1];
                int dots = 7;
                for (int d = 1; d < dots; d++)
                    Ui.Image(Body, "Path", Art.Circle, Palette.GoldLight.WithAlpha(0.7f), Vector2.Lerp(from, to, d / (float)dots), Vector2.one * 14);
            }
        }

        void Open(FloorId floor)
        {
            var status = story.Session.StatusOf(floor);
            FloorDetailPopup.Show(popups, floor, status, () => Enter(floor));
        }

        /// <summary>そのフロアのエレベーターの前（出発地点）から始める。</summary>
        void Enter(FloorId floor)
        {
            var state = story.State;
            state.floor = (int)floor;
            state.x = state.y = -1;
            story.Save();
            nav.EnterStory();
        }
    }
}
