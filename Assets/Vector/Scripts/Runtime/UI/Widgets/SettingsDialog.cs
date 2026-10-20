using System;
using UnityEngine;
using Vector.Game.Persistence;

namespace Vector.Game.UI.Widgets
{
    /// <summary>設定画面（効果音の音量・高速ディール・戦略ヒント）。変更はその場で保存して反映する。</summary>
    public static class SettingsDialog
    {
        static readonly float[] volumes = { 0f, 0.25f, 0.5f, 0.75f, 1f };
        static readonly Color Off = new Color(1, 1, 1, 0.1f);

        public static Dialog Open(Transform root, ProfileService profiles, Action changed)
        {
            var dialog = Dialog.Open(root, "SETTINGS", null, new Vector2(760, 520));
            var content = dialog.Content;
            var settings = profiles.Profile.settings;

            void Apply(Action<ProfileSettings> change)
            {
                profiles.UpdateSettings(change);
                changed?.Invoke();
            }

            Ui.Text(content, "SoundLabel", "効果音", Fonts.Body, 24, Theme.Ivory, new Vector2(-250, 110), new Vector2(200, 40));
            var volumeButtons = new CasinoButton[volumes.Length];
            for (int i = 0; i < volumes.Length; i++)
            {
                float volume = volumes[i];
                string label = volume == 0 ? "OFF" : (int)(volume * 100) + "%";
                volumeButtons[i] = Ui.Button(content, "Volume" + i, label, new Vector2(-110 + i * 92, 110), new Vector2(84, 52), Off, Theme.Ivory, () =>
                {
                    Apply(s => s.sfxVolume = volume);
                    Highlight(volumeButtons, Array.IndexOf(volumes, volume));
                }, 22);
            }
            Highlight(volumeButtons, NearestVolume(settings.sfxVolume));

            Toggle(content, "高速ディール（演出を速くする）", 20, settings.fastDeal, on => Apply(s => s.fastDeal = on));
            Toggle(content, "戦略ヒント（勝率と推奨手を表示）", -70, settings.showStrategy, on => Apply(s => s.showStrategy = on));
            dialog.AddButton("CLOSE", null, true);
            return dialog;
        }

        static void Toggle(Transform parent, string label, float y, bool initial, Action<bool> onChange)
        {
            Ui.Text(parent, "Label", label, Fonts.Body, 24, Theme.Ivory, new Vector2(-90, y), new Vector2(480, 40), TMPro.TextAlignmentOptions.Left);
            bool state = initial;
            CasinoButton button = null;
            button = Ui.Button(parent, "Toggle", "", new Vector2(250, y), new Vector2(120, 52), Off, Theme.Ivory, () =>
            {
                state = !state;
                Paint(button, state);
                onChange(state);
            }, 24);
            Paint(button, state);
        }

        static void Paint(CasinoButton button, bool on)
        {
            button.SetLabel(on ? "ON" : "OFF");
            button.SetColor(on ? Theme.Gold : Off);
            button.Label.color = on ? Theme.Ink : Theme.Ivory;
        }

        static void Highlight(CasinoButton[] buttons, int selected)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetColor(i == selected ? Theme.Gold : Off);
                buttons[i].Label.color = i == selected ? Theme.Ink : Theme.Ivory;
            }
        }

        static int NearestVolume(float value)
        {
            int best = 0;
            for (int i = 1; i < volumes.Length; i++)
            {
                if (Mathf.Abs(volumes[i] - value) < Mathf.Abs(volumes[best] - value))
                    best = i;
            }
            return best;
        }
    }
}
