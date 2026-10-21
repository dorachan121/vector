using System;
using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>フロアの画面の上部（ホームへ戻る・フロア名・メニュー、レベルと会員ランク・経験値・所持金・コンプ）。画面の上端に付く。</summary>
    public sealed class WorldHud : MonoBehaviour
    {
        public const float Height = 220f;

        /// <summary>バーの下の目標の帯の高さ（見取り図はその下から見せる）。</summary>
        public const float ObjectiveHeight = 76f;

        TextMeshProUGUI floorName;
        TextMeshProUGUI floorJapanese;
        TextMeshProUGUI level;
        Meter xp;
        CurrencyPill chips;
        CurrencyPill comps;
        TextMeshProUGUI objective;
        bool shown;

        public event Action HomeClicked;
        public event Action MenuClicked;

        public static WorldHud Create(Transform parent)
        {
            float w = Ui.Portrait.x;
            var rt = Ui.Edge(parent, "WorldHud", true, w, Height);
            var hud = rt.gameObject.AddComponent<WorldHud>();
            Ui.Image(rt, "Fade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0.97f), Palette.NightDeep.WithAlpha(0f)), Color.white, new Vector2(0, -14), new Vector2(w * 3, Height + 28));
            float left = -w / 2, right = w / 2;

            const float top = 52;
            Ui.IconButton(rt, "Home", IconKind.Home, new Vector2(left + 64, top), 80, new Color(1, 1, 1, 0.1f), () => hud.HomeClicked?.Invoke());
            hud.floorName = Ui.Headline(rt, "Floor", "", Fonts.Display, 46, new Vector2(left + 380, top + 10), new Vector2(520, 56), TextAlignmentOptions.Left);
            hud.floorJapanese = Ui.Text(rt, "FloorJa", "", Fonts.Body, 20, Palette.Muted, new Vector2(left + 380, top - 30), new Vector2(520, 26), TextAlignmentOptions.Left);
            Ui.Button(rt, "Menu", "メニュー", new Vector2(right - 110, top), new Vector2(190, 76), Palette.Gold, Palette.Ink, () => hud.MenuClicked?.Invoke(), 30, 24).Label.font = Fonts.Body;

            const float row = -50;
            hud.level = Ui.Text(rt, "Level", "", Fonts.Label, 26, Palette.GoldLight, new Vector2(left + 190, row + 16), new Vector2(320, 30), TextAlignmentOptions.Left);
            Ui.Outline(hud.level, Palette.Ink, 0.18f);
            hud.xp = Meter.Create(rt, "Xp", new Vector2(left + 190, row - 18), new Vector2(320, 20), Palette.Emerald);
            hud.chips = CurrencyPill.Create(rt, new Vector2(40, row), 330, Chrome.Coin, Color.white, Labels.Money);
            hud.comps = CurrencyPill.Create(rt, new Vector2(350, row), 250, Chrome.Jewel, Palette.Cyan, v => v.ToString("N0"));

            // バーのすぐ下に、いまの目標
            var goal = new Vector2(0, -Height / 2 - ObjectiveHeight / 2 - 4);
            Ui.Shadow(rt, "GoalShadow", goal + new Vector2(0, -3), new Vector2(980, 60), 30, 0.5f);
            Ui.Sliced(rt, "Goal", Chrome.Shade, Palette.Violet.WithAlpha(0.92f), goal, new Vector2(980, 60), 30);
            Ui.Sliced(rt, "GoalRim", Chrome.Rim, Palette.GoldMid.WithAlpha(0.8f), goal, new Vector2(980, 60), 30);
            Ui.Icon(rt, "GoalIcon", IconKind.Crown, Palette.Gold, goal + new Vector2(-450, 0), 40);
            hud.objective = Ui.Text(rt, "Objective", "", Fonts.Body, 24, Palette.Ivory, goal + new Vector2(24, 0), new Vector2(880, 34), TextAlignmentOptions.Left);
            return hud;
        }

        public void SetFloor(FloorId floor)
        {
            floorName.text = StoryNames.Floor(floor);
            floorJapanese.text = StoryNames.FloorJapanese(floor);
        }

        /// <summary>いまの目標（バーの下の帯）。</summary>
        public void SetObjective(string text) => objective.text = text;

        public void Refresh(RpgState state, long balance)
        {
            var rank = Progression.RankFor(state.level);
            long need = Progression.XpToNext(state.level);
            level.text = $"Lv {state.level}  ·  {StoryNames.Rank(rank)}";
            xp.SetFraction(need > 0 ? state.xp / (float)need : 1, shown);
            chips.SetValue(balance, shown);
            comps.SetValue(state.comps, shown);
            shown = true;
        }
    }
}
