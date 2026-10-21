using UnityEngine;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>お知らせ（運営からの案内の一覧。分類の札・見出し・本文）。</summary>
    public static class NoticePopup
    {
        static readonly (string tag, Color color, string title, string body)[] Notices =
        {
            ("イベント", Palette.Ruby, "VECTOR CASINO RESORT グランドオープン！", "10のオリジナルゲームと、フロアの主に挑むストーリーモードをお楽しみください。新作 HANABI（打ち上げ花火）・SUGOROKU（和の双六）・HARPOON（深海の狩り）も登場！"),
            ("新機能", Palette.Emerald, "ログインボーナスとデイリーミッション", "毎日のログインと3つのミッションで、遊戯用チップやコンプが受け取れます。休んでもリセットされません。"),
            ("ゲーム", Palette.Sapphire, "すべてのゲームの配当と確率を公開中", "各テーブルの「遊び方」から、配当表と RTP（プレイヤーへの還元率）を確認できます。"),
            ("大切", Palette.Amber, "遊戯用チップについて", "このゲームのチップは遊戯用です。現金・景品とは交換できません。30分ごとにプレイ時間と収支をお知らせします。"),
        };

        public static void Show(Transform root)
        {
            var dialog = Dialog.Open(root, "お知らせ", null, new Vector2(960, 1240));
            var content = dialog.Content;
            float y = content.sizeDelta.y / 2 - 120;
            foreach (var (tag, color, title, body) in Notices)
            {
                var card = Ui.Card(content, "Notice", new Vector2(0, y), new Vector2(880, 230), Palette.PanelTop, 22, false);
                Ui.Sliced(card, "Tag", Chrome.Gloss, color, new Vector2(-340, 76), new Vector2(160, 46), 23);
                Ui.Text(card, "TagText", tag, Fonts.Body, 24, Palette.Ivory, new Vector2(-340, 77), new Vector2(150, 34));
                Ui.Text(card, "Title", title, Fonts.Body, 28, Palette.GoldLight, new Vector2(80, 76), new Vector2(660, 40), TMPro.TextAlignmentOptions.Left);
                Ui.Paragraph(card, "Body", body, 23, Palette.Ivory, new Vector2(0, -30), new Vector2(820, 130));
                y -= 254;
            }
            dialog.AddButton("閉じる", null, true);
        }
    }
}
