using System;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>ログインボーナスを受け取ったときの演出（暦に今日の判が押され、受け取ったものが出る）。</summary>
    public static class LoginBonusPopup
    {
        public static void Show(Transform root, int claimed, RewardGrant grant, int level, Action closed)
        {
            var dialog = Dialog.Open(root, "LOGIN BONUS", null, new Vector2(960, 1060));
            var content = dialog.Content;
            float top = content.sizeDelta.y / 2;
            Ui.Text(content, "Lead", $"ログイン {claimed} 日目のボーナスです！", Fonts.Body, 34, Palette.GoldLight, new Vector2(0, top - 40), new Vector2(880, 46));
            LoginCalendar.Create(content, new Vector2(0, top - 330), claimed, true);
            var rewards = RewardDisplay.From(grant);
            var line = new System.Text.StringBuilder();
            foreach (var reward in rewards)
                line.Append(line.Length > 0 ? "  ·  " : "").Append(reward.Amount);
            Ui.Text(content, "Reward", "本日の報酬  " + line, Fonts.Body, 32, Palette.Ivory, new Vector2(0, top - 640), new Vector2(880, 44));
            if (grant.LevelsGained > 0)
                Ui.Text(content, "LevelUp", $"LEVEL UP!  Lv.{level}", Fonts.Display, 40, Palette.GoldLight, new Vector2(0, top - 696), new Vector2(880, 50));
            Ui.Text(content, "Note", "ログインボーナスは、休んでも途中からそのまま続きます。", Fonts.Body, 22, Palette.Muted, new Vector2(0, top - 750), new Vector2(880, 30));
            dialog.AddButton("OK", closed, true);
            ParticleBurst.Confetti(root, new Vector2(0, 300), 80);
            Sfx.Play(SfxKind.Sparkle);
        }
    }
}
