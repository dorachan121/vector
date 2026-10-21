using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.World;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ストーリーの塔の1フロア（主の顔の札と、横の名札）。進み具合（制覇・挑戦できる・レベル不足・未開放）で色と印が変わる。押すと詳しい案内を開く。
    /// </summary>
    public sealed class FloorNode
    {
        public const float FaceSize = 148f;

        Image frame;
        Image lockShade;
        Image lockIcon;
        Image glow;
        RectTransform stamp;
        TextMeshProUGUI master;
        TextMeshProUGUI status;
        Image statusChip;
        string masterName;

        public FloorId Floor { get; private set; }

        /// <summary>顔の札の位置（親の座標）。</summary>
        public Vector2 Position { get; private set; }

        /// <param name="parent">置く親。</param>
        /// <param name="floor">フロア。</param>
        /// <param name="position">顔の札の中心。</param>
        /// <param name="labelSide">名札を出す側（-1 で左、1 で右）。</param>
        /// <param name="onClick">押したとき。</param>
        public static FloorNode Create(Transform parent, FloorId floor, Vector2 position, int labelSide, Action onClick)
        {
            var node = new FloorNode { Floor = floor, Position = position };
            var spec = Floors.Of(floor).Master;
            node.masterName = StoryNames.Person(spec.Master);
            node.glow = Ui.Image(parent, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.6f), position, Vector2.one * 300);
            Pulse.Add(node.glow, 0.08f, 2.2f);

            var face = Ui.Rect(parent, "Floor" + floor, position, Vector2.one * FaceSize);
            Ui.Shadow(face, "Shadow", new Vector2(0, -6), Vector2.one * FaceSize, 36, 0.6f);
            node.frame = Ui.Sliced(face, "Frame", Chrome.Gloss, Palette.Gold, Vector2.zero, Vector2.one * FaceSize, 36, true);
            node.frame.gameObject.AddComponent<ClickTarget>().Clicked += onClick;
            Ui.Sliced(face, "Inner", Chrome.Shade, Palette.Violet, Vector2.zero, Vector2.one * (FaceSize - 16), 30);
            var window = Ui.Clip(face, "Window", Vector2.zero, Vector2.one * (FaceSize - 22));
            PortraitView.Create(window, "Master", Cast.Of(spec.Master).Look, new Vector2(0, -56), 250);
            node.lockShade = Ui.Sliced(face, "LockShade", Chrome.Shade, Palette.NightDeep.WithAlpha(0.78f), Vector2.zero, Vector2.one * (FaceSize - 16), 30);
            node.lockIcon = Ui.Icon(face, "Lock", IconKind.Lock, Palette.Muted, Vector2.zero, 64);
            Ui.Sliced(face, "Shine", Chrome.Shine, Color.white.WithAlpha(0.6f), Vector2.zero, Vector2.one * FaceSize, 36);
            node.stamp = Ui.Rect(face, "Clear", new Vector2(FaceSize / 2 - 20, -FaceSize / 2 + 20), Vector2.one * 70);
            Ui.Image(node.stamp, "Disc", Art.Circle, Palette.Gold, Vector2.zero, Vector2.one * 70);
            Ui.Icon(node.stamp, "Crown", IconKind.Crown, Palette.Ink, new Vector2(0, 2), 44);

            // 名札（階・フロアの名前・主・進み具合）
            var label = Ui.Rect(parent, "Label" + floor, position + new Vector2(labelSide * (FaceSize / 2 + 24 + 150), 0), new Vector2(300, 150));
            var align = labelSide < 0 ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
            Ui.Headline(label, "Level", StoryNames.FloorLevel(floor), Fonts.Display, 46, new Vector2(0, 46), new Vector2(300, 54), align);
            Ui.Outline(Ui.Text(label, "Title", StoryNames.FloorTitle(floor), Fonts.Label, 26, Palette.Ivory, new Vector2(0, 8), new Vector2(300, 32), align), Palette.Ink, 0.24f);
            node.master = Ui.Text(label, "Master", "", Fonts.Body, 21, Palette.Muted, new Vector2(0, -22), new Vector2(300, 28), align);
            Ui.Outline(node.master, Palette.Ink, 0.28f);
            float chipX = labelSide < 0 ? 300 / 2 - 88 : -300 / 2 + 88;
            node.statusChip = Ui.Sliced(label, "Chip", Chrome.Gloss, Palette.Emerald, new Vector2(chipX, -58), new Vector2(176, 38), 19);
            node.status = Ui.Text(label, "Status", "", Fonts.Body, 21, Palette.Ivory, new Vector2(chipX, -57), new Vector2(170, 30));
            Ui.Outline(node.status, Palette.Ink, 0.24f);
            return node;
        }

        public void Paint(FloorStatus floorStatus, int requiredLevel)
        {
            bool locked = floorStatus == FloorStatus.Locked;
            lockShade.enabled = locked;
            lockIcon.enabled = locked;
            glow.enabled = floorStatus == FloorStatus.Ready;
            stamp.gameObject.SetActive(floorStatus == FloorStatus.Cleared);
            master.text = locked ? "主  ？？？" : "主  " + masterName;
            switch (floorStatus)
            {
                case FloorStatus.Cleared:
                    Set(Palette.Gold, Palette.Emerald, "制覇");
                    break;
                case FloorStatus.Ready:
                    Set(Palette.GoldLight, Palette.Ruby, "挑戦できる！");
                    break;
                case FloorStatus.Open:
                    Set(Palette.VioletBright, Palette.Violet, $"Lv.{requiredLevel} で挑戦");
                    break;
                default:
                    Set(Palette.Slate, Palette.NightDeep, "未開放");
                    break;
            }
        }

        void Set(Color frameColor, Color chipColor, string text)
        {
            frame.color = frameColor;
            statusChip.color = chipColor;
            status.text = text;
        }
    }
}
