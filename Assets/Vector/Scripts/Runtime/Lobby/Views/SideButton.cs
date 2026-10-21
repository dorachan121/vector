using System;
using UnityEngine;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>ホームの横に並べる丸いメニュー（お知らせ・ログボ・ミッション・メニュー）。光沢の円・アイコン・下の名前・バッジ。</summary>
    public static class SideButton
    {
        public const float Size = 112f;

        public static Badge Create(Transform parent, string name, Vector2 position, IconKind icon, Color color, string label, Action onClick)
        {
            var button = Ui.IconButton(parent, name, icon, position, Size, color, onClick);
            var text = Ui.Text(button.transform, "Caption", label, Fonts.Body, 22, Palette.Ivory, new Vector2(0, -Size / 2 - 16), new Vector2(170, 30));
            Ui.Outline(text, Palette.Ink, 0.28f);
            var badge = Badge.Create(button.transform, new Vector2(Size / 2 - 14, Size / 2 - 14), 36);
            badge.SetCount(0);
            return badge;
        }
    }
}
