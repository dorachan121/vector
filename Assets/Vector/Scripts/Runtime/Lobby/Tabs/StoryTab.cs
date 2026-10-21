using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;
using Vector.Game.World;

namespace Vector.Game.Lobby
{
    /// <summary>ストーリーのタブ。最上階で待つオーナー、建物の各フロアの進み具合（上の階ほど上）、カジノへ入るボタン。</summary>
    public sealed class StoryTab : IHomeTab
    {
        static readonly Vector2 RowSize = new Vector2(1000, 102);
        const float RowSpacing = 114;
        const float TopRow = 140;

        sealed class Row
        {
            public FloorId Floor;
            public Image Body;
            public Image Tag;
            public TextMeshProUGUI Status;
            public Image StatusIcon;
        }

        readonly RpgSession session;
        readonly List<Row> rows = new List<Row>();
        Meter progress;
        TextMeshProUGUI progressText;

        StoryTab(Transform parent, RpgSession session)
        {
            this.session = session;
            Root = Ui.Stretch(parent, "StoryTab");
            Body = HomeLayout.Top(Root);
        }

        public RectTransform Root { get; }

        /// <summary>中身の土台（上端に寄せる）。</summary>
        RectTransform Body { get; }

        public static StoryTab Create(Transform parent, RpgSession session, HomeNavigator nav)
        {
            var tab = new StoryTab(parent, session);
            tab.BuildIntro();
            for (int i = 0; i < FloorOrder.TopDown.Length; i++)
                tab.rows.Add(tab.BuildRow(FloorOrder.TopDown[i], TopRow - i * RowSpacing));
            var enter = Ui.Button(tab.Body, "Enter", "ENTER THE CASINO", new Vector2(0, -690), new Vector2(680, 112), Palette.Gold, Palette.Ink, nav.EnterStory, 42, 30);
            Pulse.Add(enter, 0.025f, 2.2f);
            return tab;
        }

        void BuildIntro()
        {
            Ui.Ribbon(Body, "Title", "STORY MODE", new Vector2(0, 706), 560, Palette.Ruby, 36);
            var rays = Ui.Image(Body, "Rays", Chrome.Rays(18), Palette.Gold.WithAlpha(0.16f), new Vector2(300, 430), Vector2.one * 640);
            Spin.Add(rays, 5f);
            PortraitView.Create(Body, "Owner", Cast.Of("zero").Look, new Vector2(300, 420), 470);
            Ui.Ribbon(Body, "OwnerName", "OWNER  ZERO", new Vector2(300, 240), 380, Palette.Violet, 26);
            float left = -470;
            var head = Ui.Headline(Body, "Lead", "THE OWNER AWAITS", Fonts.Display, 50, new Vector2(left + 260, 560), new Vector2(520, 60), TextAlignmentOptions.Left);
            head.characterSpacing = 2;
            Ui.Paragraph(Body, "Pitch", "最上階のオーナー、ゼロに挑むには、\n各フロアの主に勝って上を目指すこと。\nカジノの中を歩き、人と話し、腕を磨こう。", 24, Palette.Ivory, new Vector2(left + 260, 430), new Vector2(520, 150));
            Ui.Text(Body, "ProgressLabel", "FLOOR MASTERS", Fonts.Label, 22, Palette.Muted, new Vector2(left + 260, 318), new Vector2(520, 28), TextAlignmentOptions.Left);
            progress = Meter.Create(Body, "Progress", new Vector2(left + 230, 284), new Vector2(460, 28), Palette.Gold);
            progressText = Ui.Text(Body, "ProgressText", "", Fonts.Display, 30, Palette.GoldLight, new Vector2(left + 500, 284), new Vector2(80, 36));
        }

        Row BuildRow(FloorId floor, float y)
        {
            var row = new Row { Floor = floor };
            row.Body = Ui.Plate(Body, "Floor" + floor, new Vector2(0, y), RowSize, Palette.PanelTop, 22, false);
            var rt = (RectTransform)row.Body.transform.parent;
            float left = -RowSize.x / 2;
            row.Tag = Ui.Sliced(rt, "Tag", Chrome.Gloss, Palette.Violet, new Vector2(left + 66, 0), new Vector2(96, 76), 20);
            Ui.Text(rt, "Level", StoryNames.FloorLevel(floor), Fonts.Display, 36, Palette.Ivory, new Vector2(left + 66, 1), new Vector2(96, 44));
            Ui.Text(rt, "Name", StoryNames.FloorTitle(floor), Fonts.Label, 30, Palette.Ivory, new Vector2(left + 340, 18), new Vector2(420, 36), TextAlignmentOptions.Left);
            var master = Floors.Of(floor).Master;
            Ui.Text(rt, "Master", $"{StoryNames.FloorJapanese(floor)}  ·  主 {StoryNames.Person(master.Master)}  ·  {GameCatalog.Of(master.Game).Title}",
                Fonts.Body, 19, Palette.Muted, new Vector2(left + 340, -20), new Vector2(420, 26), TextAlignmentOptions.Left);
            row.StatusIcon = Ui.Icon(rt, "StatusIcon", IconKind.Lock, Palette.Muted, new Vector2(RowSize.x / 2 - 220, 0), 40);
            row.Status = Ui.Text(rt, "Status", "", Fonts.Label, 26, Palette.Muted, new Vector2(RowSize.x / 2 - 110, 0), new Vector2(180, 34), TextAlignmentOptions.Left);
            return row;
        }

        public void Refresh()
        {
            int cleared = 0;
            foreach (var row in rows)
            {
                var status = session.StatusOf(row.Floor);
                if (status == FloorStatus.Cleared)
                    cleared++;
                Paint(row, status, Floors.Of(row.Floor).Master.RequiredLevel);
            }
            progress.SetFraction(cleared / (float)rows.Count, false);
            progressText.text = $"{cleared}/{rows.Count}";
        }

        static void Paint(Row row, FloorStatus status, int requiredLevel)
        {
            switch (status)
            {
                case FloorStatus.Cleared:
                    Set(row, Palette.PanelTop, Palette.Emerald, IconKind.Check, Palette.Emerald, "CLEARED");
                    break;
                case FloorStatus.Ready:
                    Set(row, Palette.VioletBright, Palette.Gold, IconKind.Crown, Palette.GoldLight, "CHALLENGE");
                    break;
                case FloorStatus.Open:
                    Set(row, Palette.Violet, Palette.VioletBright, IconKind.Star, Palette.Ivory, $"Lv {requiredLevel}");
                    break;
                default:
                    Set(row, Palette.NightDeep, Palette.Slate, IconKind.Lock, Palette.Muted, "LOCKED");
                    break;
            }
        }

        static void Set(Row row, Color body, Color tag, IconKind icon, Color text, string status)
        {
            row.Body.color = body;
            row.Tag.color = tag;
            row.StatusIcon.sprite = Icons.Of(icon);
            row.StatusIcon.color = text;
            row.Status.color = text;
            row.Status.text = status;
        }
    }
}
