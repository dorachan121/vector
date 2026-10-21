using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;

namespace Vector.Game.World
{
    /// <summary>フロアの画面の上部のバー（タイトルへ戻る・フロア名・レベルと会員ランク・経験値・所持金・コンプ・メニュー）。</summary>
    public sealed class WorldHud : MonoBehaviour
    {
        const float Height = 84f;
        static readonly Color ButtonColor = new Color(1, 1, 1, 0.08f);

        TextMeshProUGUI floorName;
        TextMeshProUGUI level;
        TextMeshProUGUI balance;
        TextMeshProUGUI comps;
        Image xpFill;
        long shownBalance = -1;

        public event Action TitleClicked;
        public event Action MenuClicked;

        public static WorldHud Create(Transform parent)
        {
            var rt = Ui.Rect(parent, "WorldHud", new Vector2(0, Ui.ReferenceResolution.y / 2 - Height / 2), new Vector2(Ui.ReferenceResolution.x, Height));
            var hud = rt.gameObject.AddComponent<WorldHud>();
            Ui.Image(rt, "Fade", Art.VerticalFade, new Color(0, 0, 0, 0.9f), Vector2.zero, new Vector2(Ui.ReferenceResolution.x, Height));
            float left = -Ui.ReferenceResolution.x / 2, right = Ui.ReferenceResolution.x / 2;
            Ui.Button(rt, "Title", "< TITLE", new Vector2(left + 100, 0), new Vector2(150, 50), ButtonColor, Theme.Ivory, () => hud.TitleClicked?.Invoke(), 24);
            hud.floorName = Ui.Text(rt, "Floor", "", Fonts.Display, 38, Theme.Gold, new Vector2(left + 410, 2), new Vector2(420, 56), TextAlignmentOptions.Left);

            hud.level = Ui.Text(rt, "Level", "", Fonts.Label, 24, Theme.Ivory, new Vector2(-30, 12), new Vector2(460, 30));
            Ui.Panel(rt, "XpBack", new Vector2(-30, -16), new Vector2(400, 10), new Color(1, 1, 1, 0.15f), 5);
            hud.xpFill = Ui.Panel(rt, "Xp", new Vector2(-30, -16), new Vector2(400, 10), Theme.TieGreen, 5);
            hud.xpFill.type = Image.Type.Filled;
            hud.xpFill.fillMethod = Image.FillMethod.Horizontal;

            Ui.Text(rt, "BalanceLabel", "BALANCE", Fonts.Label, 16, Theme.Muted, new Vector2(right - 520, 16), new Vector2(220, 22), TextAlignmentOptions.Right);
            hud.balance = Ui.Text(rt, "Balance", "", Fonts.Display, 36, Theme.GoldBright, new Vector2(right - 520, -10), new Vector2(220, 44), TextAlignmentOptions.Right);
            Ui.Text(rt, "CompLabel", "COMP", Fonts.Label, 16, Theme.Muted, new Vector2(right - 300, 16), new Vector2(140, 22), TextAlignmentOptions.Right);
            hud.comps = Ui.Text(rt, "Comps", "", Fonts.Display, 30, Theme.Ivory, new Vector2(right - 300, -10), new Vector2(140, 40), TextAlignmentOptions.Right);
            Ui.Button(rt, "Menu", "MENU", new Vector2(right - 110, 0), new Vector2(150, 54), Theme.Gold, Theme.Ink, () => hud.MenuClicked?.Invoke(), 26);
            return hud;
        }

        public void SetFloor(FloorId floor) => floorName.text = StoryNames.Floor(floor);

        public void Refresh(RpgState state, long chips)
        {
            var rank = Progression.RankFor(state.level);
            long need = Progression.XpToNext(state.level);
            level.text = need > 0
                ? $"Lv {state.level}  ·  {StoryNames.Rank(rank)}  ·  XP {state.xp} / {need}"
                : $"Lv {state.level}  ·  {StoryNames.Rank(rank)}  ·  MAX";
            xpFill.fillAmount = need > 0 ? state.xp / (float)need : 1;
            comps.text = state.comps.ToString("N0");
            if (shownBalance < 0 || shownBalance == chips)
            {
                balance.text = Labels.Money(chips);
                shownBalance = chips;
                return;
            }
            long from = shownBalance;
            shownBalance = chips;
            Tween.Run(balance, "count", 0.6f, Ease.OutCubic, t => balance.text = Labels.Money((long)Math.Round(from + (chips - from) * (double)t)));
        }
    }
}
