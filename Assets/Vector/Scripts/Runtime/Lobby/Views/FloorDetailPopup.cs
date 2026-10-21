using System;
using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;
using Vector.Game.World;

namespace Vector.Game.Lobby
{
    /// <summary>ストーリーの塔でフロアを選んだときの案内（主の立ち絵・遊ぶゲーム・挑戦の条件・はじめて勝ったときの報酬・そのフロアへ行くボタン）。</summary>
    public static class FloorDetailPopup
    {
        public static void Show(Transform root, FloorId floor, FloorStatus status, Action enter)
        {
            var spec = Floors.Of(floor).Master;
            bool locked = status == FloorStatus.Locked;
            var dialog = Dialog.Open(root, StoryNames.Floor(floor), null, new Vector2(960, 1260));
            var content = dialog.Content;
            float top = content.sizeDelta.y / 2;

            var rays = Ui.Image(content, "Rays", Chrome.Rays(18), Palette.Gold.WithAlpha(0.22f), new Vector2(0, top - 230), Vector2.one * 640);
            Spin.Add(rays, 8f);
            var stage = Ui.Clip(content, "Stage", new Vector2(0, top - 230), new Vector2(880, 440));
            var portrait = PortraitView.Create(stage, "Master", Cast.Of(spec.Master).Look, new Vector2(0, -80), 600);
            if (locked)
                portrait.gameObject.AddComponent<CanvasGroup>().alpha = 0.25f;
            Ui.Image(content, "Fade", Chrome.Gradient(Palette.PanelTop.WithAlpha(0f), Palette.PanelTop), Color.white, new Vector2(0, top - 410), new Vector2(880, 90));
            string name = locked ? "？？？" : StoryNames.Person(spec.Master);
            Ui.Outline(Ui.Text(content, "Name", name, Fonts.Body, 44, Palette.GoldLight, new Vector2(0, top - 470), new Vector2(880, 56)), Palette.Ink, 0.24f);
            Ui.Text(content, "Role", locked ? "" : StoryNames.Title(spec.Master), Fonts.Body, 24, Palette.Muted, new Vector2(0, top - 520), new Vector2(880, 32));

            var game = GameCatalog.Of(spec.Game);
            var rows = new (string label, string value)[]
            {
                ("遊ぶゲーム", game.Title),
                ("挑戦の条件", $"Lv.{spec.RequiredLevel} 以上 · 参加費 {Labels.Money(spec.EntryFee)}"),
                ("勝利の条件", $"{spec.Rounds} ラウンドで {Labels.Money(spec.Stack)} → {Labels.Money(spec.Target)}"),
                ("初回の報酬", $"{Labels.Money(spec.Prize)} · {StoryNames.Item(spec.Trophy)}"),
            };
            float y = top - 600;
            foreach (var (label, value) in rows)
            {
                var row = Ui.Card(content, "Row", new Vector2(0, y), new Vector2(860, 76), Palette.NightDeep.WithAlpha(0.7f), 20, false);
                Ui.Text(row, "Label", label, Fonts.Body, 24, Palette.GoldLight, new Vector2(-300, 0), new Vector2(220, 34), TextAlignmentOptions.Left);
                Ui.Text(row, "Value", value, Fonts.Body, 26, Palette.Ivory, new Vector2(110, 0), new Vector2(600, 34), TextAlignmentOptions.Left);
                y -= 90;
            }
            Ui.Text(content, "Note", Note(status), Fonts.Body, 24, status == FloorStatus.Ready ? Palette.GoldLight : Palette.Muted, new Vector2(0, y - 6), new Vector2(880, 32));

            dialog.AddButton("閉じる", null);
            if (!locked)
                dialog.AddButton("このフロアへ", enter, true);
        }

        static string Note(FloorStatus status)
        {
            switch (status)
            {
                case FloorStatus.Cleared: return "制覇したフロアです。主にはまた挑めます（報酬は初回のみ）。";
                case FloorStatus.Ready: return "主に挑めます！ フロアで主に話しかけてください。";
                case FloorStatus.Open: return "レベルが上がると主に挑めます。テーブルで遊んで経験値を貯めましょう。";
                default: return "下のフロアの主に勝つと行けるようになります。";
            }
        }
    }
}
