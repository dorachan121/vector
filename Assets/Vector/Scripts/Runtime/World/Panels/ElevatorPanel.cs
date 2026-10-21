using System;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>エレベーターの行き先の選択。まだ主に勝っていないフロアは押せない。</summary>
    public static class ElevatorPanel
    {
        const float RowHeight = 72f;

        public static void Open(Transform root, RpgSession session, FloorId current, Action<FloorId> go)
        {
            var dialog = Dialog.Open(root, "ELEVATOR", "行き先のフロアを選んでください。", new Vector2(760, 300 + FloorOrder.TopDown.Length * RowHeight));
            float top = dialog.Content.sizeDelta.y / 2 - RowHeight / 2;
            for (int i = 0; i < FloorOrder.TopDown.Length; i++)
            {
                var floor = FloorOrder.TopDown[i];
                bool open = session.IsFloorOpen(floor);
                string label = floor == current ? $"{StoryNames.Floor(floor)}  ·  HERE" : open ? StoryNames.Floor(floor) : $"{StoryNames.Floor(floor)}  ·  LOCKED";
                var button = Ui.Button(dialog.Content, "Floor" + floor, label, new Vector2(0, top - i * RowHeight), new Vector2(600, RowHeight - 10),
                    floor == current ? Theme.Gold.WithAlpha(0.35f) : new Color(1, 1, 1, 0.1f), open ? Theme.Ivory : Theme.Muted, () =>
                    {
                        dialog.Close();
                        go(floor);
                    }, 26);
                button.Interactable = open && floor != current;
            }
            dialog.AddButton("CLOSE", null);
        }
    }
}
