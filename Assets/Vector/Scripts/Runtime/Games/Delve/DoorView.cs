using System;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Delve
{
    /// <summary>迷宮の扉1枚。選ぶと開いて、奥にいたもの（宝か魔物）を見せる。</summary>
    public sealed class DoorView : MonoBehaviour
    {
        public static readonly Vector2 Size = new Vector2(190, 285);

        Image door;
        Image glow;
        Image treasure;
        Image monster;
        Image[] eyes;
        ClickTarget click;

        public RectTransform Rect => (RectTransform)transform;

        public event Action Clicked;

        public static DoorView Create(Transform parent, string name, Vector2 position)
        {
            var rt = Ui.Rect(parent, name, position, Size);
            var view = rt.gameObject.AddComponent<DoorView>();
            view.glow = Ui.Image(rt, "Glow", Art.Glow, Color.clear, new Vector2(0, -10), Size * 1.5f);
            Ui.Image(rt, "Dark", Art.Door, new Color(0.05f, 0.04f, 0.03f, 1), Vector2.zero, Size);
            view.treasure = Ui.Image(rt, "Treasure", Art.Gem(4), Theme.GoldBright, new Vector2(0, -20), new Vector2(110, 110));
            view.monster = Ui.Image(rt, "Monster", Art.BeastHead, Theme.Hex("6B1414"), new Vector2(0, -10), new Vector2(160, 160));
            view.eyes = new[]
            {
                Ui.Image(view.monster.transform, "EyeL", Art.Glow, Theme.DealerRed, new Vector2(-20, -3), new Vector2(26, 26)),
                Ui.Image(view.monster.transform, "EyeR", Art.Glow, Theme.DealerRed, new Vector2(20, -3), new Vector2(26, 26)),
            };
            view.door = Ui.Image(rt, "Door", Art.Door, Color.white, Vector2.zero, Size, true);
            view.door.rectTransform.pivot = new Vector2(0, 0.5f);
            view.door.rectTransform.anchoredPosition = new Vector2(-Size.x / 2, 0);
            view.click = view.door.gameObject.AddComponent<ClickTarget>();
            view.click.Clicked += () => view.Clicked?.Invoke();
            view.click.Hovered += h =>
            {
                if (view.click.Interactable)
                    Tween.Tint(view.glow, h ? Theme.GoldBright.WithAlpha(0.45f) : Theme.Gold.WithAlpha(0.15f), 0.12f);
            };
            view.Close();
            return view;
        }

        /// <summary>扉を閉じた状態に戻す。</summary>
        public void Close()
        {
            Tween.Stop(door.rectTransform);
            door.rectTransform.localScale = Vector3.one;
            door.color = Color.white;
            treasure.enabled = monster.enabled = false;
            foreach (var eye in eyes)
                eye.enabled = false;
            glow.color = Color.clear;
            SetSelectable(false);
        }

        public void SetSelectable(bool selectable)
        {
            click.Interactable = selectable;
            Tween.Tint(glow, selectable ? Theme.Gold.WithAlpha(0.15f) : Color.clear, 0.2f);
        }

        /// <summary>扉を開ける。chosen は自分が選んだ扉（他の扉は少し遅れて暗めに開く）。</summary>
        public void Open(bool hasMonster, bool chosen, float delay)
        {
            SetSelectable(false);
            treasure.enabled = !hasMonster;
            monster.enabled = hasMonster;
            foreach (var eye in eyes)
                eye.enabled = hasMonster;
            var tint = chosen ? Color.white : new Color(1, 1, 1, 0.55f);
            treasure.color = Theme.GoldBright * tint;
            monster.color = Theme.Hex("6B1414") * tint;
            // 蝶番（左端）を軸に、横に潰して開いたように見せる
            Tween.Scale(door.rectTransform, new Vector3(0.12f, 1, 1), 0.35f, Ease.OutCubic, null, delay);
            Tween.Tint(door, new Color(0.55f, 0.55f, 0.55f, 1), 0.35f, Ease.OutQuad, null, delay);
            if (chosen)
                Tween.Tint(glow, (hasMonster ? Theme.DealerRed : Theme.GoldBright).WithAlpha(0.7f), 0.3f, Ease.OutQuad, null, delay + 0.2f);
        }
    }
}
