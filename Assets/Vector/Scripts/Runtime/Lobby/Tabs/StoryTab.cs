using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ストーリー。カジノの塔を下から登っていく地図（上の階ほど上）。各フロアの主の顔の札と進み具合、いま目指すフロアの印。
    /// 塔は上下にスクロールでき、開いたときは目指すフロアが真ん中に来る。フロアを押すと詳しい案内を出し、そこからフロアへ入れる。
    /// </summary>
    public sealed class StoryTab : IHomeTab
    {
        /// <summary>
        /// 塔を見せる窓の上端（ページの見出しの下）と下端（「カジノへ入る」ボタンの上）。基準の高さの中身の中心からの位置で、
        /// 縦に長い端末では上端は見出しに、下端はボタンに付いたまま窓が伸びる。
        /// </summary>
        const float ViewTop = 672;
        const float ViewBottom = -556;

        /// <summary>フロアの間隔と、左右に振る幅。</summary>
        const float Step = 170;
        const float Zig = 200;

        /// <summary>塔のいちばん上（王冠と最上階）・いちばん下の余白。</summary>
        const float TopPad = 210;
        const float BottomPad = 150;

        readonly StoryProgress story;
        readonly HomeNavigator nav;
        readonly Transform popups;
        readonly List<FloorNode> nodes = new List<FloorNode>();
        RectTransform viewport;
        RectTransform tower;
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
            var floors = FloorOrder.TopDown;
            float height = TopPad + (floors.Length - 1) * Step + BottomPad;
            tab.BuildTower(height);
            var points = new Vector2[floors.Length];
            for (int i = 0; i < floors.Length; i++)
                points[i] = new Vector2(i == 0 ? 0 : (i % 2 == 1 ? Zig : -Zig), height / 2 - TopPad - i * Step);
            tab.BuildPath(points);
            for (int i = 0; i < floors.Length; i++)
            {
                var floor = floors[i];
                // 札の説明は道の内側に出す。中央の最上階は、すぐ下のフロアと反対側に出して重ならないようにする
                float from = i == 0 && points.Length > 1 ? points[1].x : points[i].x;
                int side = from > 0 ? -1 : 1;
                tab.nodes.Add(FloorNode.Create(tab.tower, floor, points[i], side, () => tab.Open(floor)));
            }
            // 揺れるのは中身だけにして、置き場所（marker）は Refresh で動かせるようにする
            tab.marker = Ui.Rect(tab.tower, "Marker", Vector2.zero, new Vector2(90, 90));
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
            {
                marker.anchoredPosition = next.Position + new Vector2(0, FloorNode.FaceSize / 2 + 36);
                ScrollTo(next.Position.y);
            }
        }

        /// <summary>塔の y の位置が窓の真ん中に来るようにスクロールする（端では止める）。</summary>
        void ScrollTo(float y)
        {
            float height = tower.sizeDelta.y, view = viewport.rect.height;
            if (height <= view)
                return;
            float fromTop = height / 2 - y;
            tower.anchoredPosition = new Vector2(0, Mathf.Clamp(fromTop - view / 2, 0, height - view));
        }

        /// <summary>塔の絵（夜空の中のビル・窓の灯り・屋上の王冠）。ビルは上下にスクロールする窓の中に描く。</summary>
        void BuildTower(float height)
        {
            Ui.Image(Body, "Sky", Chrome.Gradient(Theme.Hex("2A1060"), Palette.NightDeep.WithAlpha(0f)), Color.white, new Vector2(0, 100), new Vector2(Ui.Portrait.x * 3, 1500));
            Ui.Image(Body, "Glow", Art.Glow, Palette.Magenta.WithAlpha(0.22f), new Vector2(0, -20), new Vector2(1000, 1500));

            float half = HomeLayout.ContentHeight / 2;
            viewport = Ui.Between(Root, "TowerView", half - ViewTop, half + ViewBottom, Ui.Portrait.x);
            viewport.gameObject.AddComponent<RectMask2D>();
            // 札のない所を押してもスクロールできるように、透明の受け皿を敷く
            Ui.Stretch(viewport, "DragArea").gameObject.AddComponent<Image>().color = Color.clear;
            tower = Ui.Rect(viewport, "Tower", Vector2.zero, new Vector2(Ui.Portrait.x, height));
            tower.anchorMin = tower.anchorMax = tower.pivot = new Vector2(0.5f, 1f);
            tower.anchoredPosition = Vector2.zero;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = tower;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.decelerationRate = 0.1f;
            scroll.scrollSensitivity = 40f;

            var building = new Vector2(620, height - 90);
            var center = new Vector2(0, -45);
            Ui.Panel(tower, "Building", center, building, Theme.Hex("1E0C44").WithAlpha(0.9f), 40);
            Ui.Outline(tower, "BuildingRim", center, building, Palette.GoldMid.WithAlpha(0.5f), 40);
            int rows = (int)((building.y - 60) / 86);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    bool lit = (row * 7 + col * 3) % 5 != 0;
                    // フロアの名前や札に重なるので、窓の灯りはごく淡くする
                    Ui.Panel(tower, "Window", center + new Vector2((col - 2.5f) * 80, building.y / 2 - 60 - row * 86), new Vector2(30, 40),
                        lit ? Palette.GoldLight.WithAlpha(0.07f) : Palette.Violet.WithAlpha(0.12f), 6);
                }
            }
            var crown = Ui.Icon(tower, "Crown", IconKind.Crown, Palette.Gold, new Vector2(0, height / 2 - 70), 120);
            Float.Add(crown.rectTransform, 6f, 1.2f);

            // 窓の上下の端をぼかして、続きがあること（スクロールできること）を見せる
            const float fade = 70;
            var shade = Palette.NightDeep.WithAlpha(0.75f);
            EdgeFade("FadeTop", true, Chrome.Gradient(shade, shade.WithAlpha(0f)), fade);
            EdgeFade("FadeBottom", false, Chrome.Gradient(shade.WithAlpha(0f), shade), fade);
        }

        /// <summary>窓の上端か下端に付くぼかし（下の札は押せるまま）。</summary>
        void EdgeFade(string name, bool top, Sprite gradient, float height)
        {
            var image = Ui.Edge(viewport, name, top, Ui.Portrait.x, height).gameObject.AddComponent<Image>();
            image.sprite = gradient;
            image.raycastTarget = false;
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
                    Ui.Image(tower, "Path", Art.Circle, Palette.GoldLight.WithAlpha(0.7f), Vector2.Lerp(from, to, d / (float)dots), Vector2.one * 14);
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
