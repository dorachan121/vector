using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Story;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ログインボーナスの暦（7日で1巡。上に4日、下に3日。7日目は大きな特別の枠）。受け取った日には判を押す。
    /// </summary>
    public static class LoginCalendar
    {
        public static readonly Vector2 CellSize = new Vector2(196, 236);
        const float Gap = 16;

        /// <param name="parent">置く親。</param>
        /// <param name="center">暦の中心。</param>
        /// <param name="claimed">いまの巡で受け取った日数（0〜7）。</param>
        /// <param name="stampToday">最後に受け取った日の判を、押す演出つきで出す。</param>
        public static void Create(Transform parent, Vector2 center, int claimed, bool stampToday)
        {
            for (int day = 0; day < LoginBonus.CycleLength; day++)
            {
                bool last = day == LoginBonus.CycleLength - 1;
                Vector2 at, size;
                if (day < 4)
                {
                    at = center + new Vector2((day - 1.5f) * (CellSize.x + Gap), CellSize.y / 2 + Gap / 2);
                    size = CellSize;
                }
                else if (!last)
                {
                    at = center + new Vector2((day - 4 - 1.5f) * (CellSize.x + Gap), -CellSize.y / 2 - Gap / 2);
                    size = CellSize;
                }
                else
                {
                    // 下の段の右2つぶんの枠
                    size = new Vector2(CellSize.x * 2 + Gap, CellSize.y);
                    at = center + new Vector2(CellSize.x + Gap, -CellSize.y / 2 - Gap / 2);
                }
                bool done = day < claimed;
                bool today = stampToday && day == claimed - 1;
                Cell(parent, at, size, day, last, done, today);
            }
        }

        static void Cell(Transform parent, Vector2 at, Vector2 size, int day, bool last, bool done, bool today)
        {
            if (today)
            {
                var glow = Ui.Image(parent, "TodayGlow", Art.Glow, Palette.GoldLight.WithAlpha(0.7f), at, size * 1.5f);
                Pulse.Add(glow, 0.06f, 2f);
            }
            var rt = Ui.Card(parent, "Day" + (day + 1), at, size, last ? Palette.GoldDark : Palette.Violet, 24, today || last);
            if (last)
            {
                var rays = Ui.Image(rt, "Rays", Chrome.Rays(14), Palette.GoldLight.WithAlpha(0.25f), new Vector2(0, 10), Vector2.one * 300);
                Spin.Add(rays, 10f);
            }
            Ui.Sliced(rt, "Tag", Chrome.Gloss, last ? Palette.Ruby : Palette.VioletBright, new Vector2(0, size.y / 2 - 28), new Vector2(size.x - 30, 40), 20);
            Ui.Text(rt, "Label", last ? "7日目  SPECIAL" : $"{day + 1}日目", Fonts.Body, 22, Palette.Ivory, new Vector2(0, size.y / 2 - 27), new Vector2(size.x - 30, 34));
            var reward = LoginBonus.RewardFor(day);
            var shown = RewardDisplay.From(reward)[0];
            var icon = Ui.Image(rt, "Icon", shown.Icon, shown.Tint, new Vector2(last ? -60 : 0, 6), Vector2.one * (last ? 112 : 92));
            icon.preserveAspect = true;
            if (last)
                Ui.Icon(rt, "Gift", IconKind.Gift, Palette.Magenta, new Vector2(60, 6), 96);
            var amount = Ui.Text(rt, "Amount", reward.Chips > 0 ? Labels.Money(reward.Chips) : $"{reward.Comps} pt", Fonts.Display, 34, Palette.Ivory, new Vector2(0, -size.y / 2 + 36), new Vector2(size.x - 16, 44));
            Ui.Outline(amount, Palette.Ink, 0.22f);
            if (!done)
                return;
            Ui.Sliced(rt, "Dim", Chrome.Shade, Palette.NightDeep.WithAlpha(0.5f), Vector2.zero, size, 24);
            var stamp = Ui.Rect(rt, "Stamp", new Vector2(0, 0), Vector2.one * 130);
            Ui.Image(stamp, "Ring", Art.Ring(0.1f), Palette.Ruby, Vector2.zero, Vector2.one * 130);
            Ui.Text(stamp, "Mark", "済", Fonts.Body, 64, Palette.Ruby, new Vector2(0, 2), Vector2.one * 120);
            stamp.localEulerAngles = new Vector3(0, 0, -14);
            if (!today)
                return;
            // 今日の分は、大きな判が降りてきて押される
            stamp.localScale = Vector3.one * 2.4f;
            Tween.Scale(stamp, Vector3.one, 0.35f, Ease.OutBack, () => Sfx.Play(SfxKind.Chip), 0.45f);
        }
    }
}
