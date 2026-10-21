using System;
using UnityEngine;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>ホームのメニュー（遊び方・設定・タイトル画面）。</summary>
    public static class MenuPopup
    {
        public static void Show(Transform root, Action help, Action settings, Action title)
        {
            var dialog = Dialog.Open(root, "MENU", null, new Vector2(760, 760));
            var items = new (IconKind icon, string label, Action open)[]
            {
                (IconKind.Help, "遊び方", help),
                (IconKind.Settings, "設定", settings),
                (IconKind.Home, "タイトル画面", title),
            };
            float y = dialog.Content.sizeDelta.y / 2 - 90;
            foreach (var (icon, label, open) in items)
            {
                var button = Ui.Button(dialog.Content, label, label, new Vector2(30, y), new Vector2(560, 112), Palette.VioletBright, Palette.Ivory, () =>
                {
                    dialog.Close();
                    open();
                }, 36, 30);
                button.Label.font = Fonts.Body;
                Ui.Icon(button.transform, "Icon", icon, Palette.GoldLight, new Vector2(-230, 0), 60);
                y -= 140;
            }
            dialog.AddButton("閉じる", null, true);
        }
    }
}
