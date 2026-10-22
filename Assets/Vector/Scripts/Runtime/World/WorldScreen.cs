using UnityEngine;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>
    /// フロアの画面（縦向き）の部品（上部のバー・見取り図を置く場所・下のタッチパッド・会話の窓・お知らせ）。シーンの起点が作り、進行役が使う。
    /// </summary>
    public sealed class WorldScreen
    {
        WorldScreen(RectTransform root)
        {
            Root = root;
            // 見取り図のまわりは暗くする（端末の縦横比で画面の枠の外に余った所も、上のバーや下のパッドと同じ暗さで続く）
            var surround = Ui.Image(root, "Surround", null, Palette.NightDeep, Vector2.zero, Vector2.zero);
            Ui.Cover(surround.rectTransform);
            // 見取り図が見える場所は、上のバーと目標の帯の下から、タッチパッドの上まで
            Stage = Ui.Between(root, "Stage", WorldHud.Height + WorldHud.ObjectiveHeight, TouchPad.Height - 40, Ui.Portrait.x);
            Hud = WorldHud.Create(root);
            Pad = TouchPad.Create(root);
            Dialogue = DialogueBox.Create(root);
            // お知らせは目標の帯を隠さないよう、そのすぐ下に出す
            Toast = Toast.Create(root, WorldHud.Height + WorldHud.ObjectiveHeight + 20 + Toast.Height / 2);
        }

        /// <summary>画面の土台（ダイアログはここに開く）。</summary>
        public RectTransform Root { get; }

        /// <summary>見取り図が見える場所（フロアを移るたびに中身を作り直す）。</summary>
        public RectTransform Stage { get; }

        public WorldHud Hud { get; }
        public TouchPad Pad { get; }
        public DialogueBox Dialogue { get; }
        public Toast Toast { get; }

        public static WorldScreen Build() => new WorldScreen(Ui.CreateScreen("World", ScreenShape.Portrait));
    }
}
