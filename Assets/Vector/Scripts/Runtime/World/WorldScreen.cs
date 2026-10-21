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
            Stage = Ui.Stretch(root, "Stage");
            Hud = WorldHud.Create(root);
            Pad = TouchPad.Create(root);
            Dialogue = DialogueBox.Create(root);
            Toast = Toast.Create(root, new Vector2(0, 690));
        }

        /// <summary>画面の土台（ダイアログはここに開く）。</summary>
        public RectTransform Root { get; }

        /// <summary>見取り図を置く場所（フロアを移るたびに中身を作り直す）。</summary>
        public RectTransform Stage { get; }

        public WorldHud Hud { get; }
        public TouchPad Pad { get; }
        public DialogueBox Dialogue { get; }
        public Toast Toast { get; }

        public static WorldScreen Build() => new WorldScreen(Ui.CreateScreen("World", ScreenShape.Portrait));
    }
}
