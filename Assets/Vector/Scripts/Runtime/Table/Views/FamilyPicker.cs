using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vector.Core;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Table
{
    /// <summary>
    /// 勝負に使うラインの種類を選ぶ3つのボタン。戦略ヒントが有効なら各種類の勝率と推奨を添える。
    /// </summary>
    public sealed class FamilyPicker : MonoBehaviour
    {
        static readonly Vector2 ButtonSize = new Vector2(250, 96);
        static readonly Color ButtonColor = new Color(0.07f, 0.24f, 0.17f, 0.97f);

        readonly Dictionary<LineFamily, CasinoButton> buttons = new Dictionary<LineFamily, CasinoButton>();
        readonly Dictionary<LineFamily, TextMeshProUGUI> notes = new Dictionary<LineFamily, TextMeshProUGUI>();

        public event Action<LineFamily> Chosen;

        public static FamilyPicker Create(Transform parent, Vector2 position)
        {
            var rt = Ui.Rect(parent, "FamilyPicker", position, new Vector2(ButtonSize.x * 3 + 40, ButtonSize.y));
            var view = rt.gameObject.AddComponent<FamilyPicker>();
            for (int i = 0; i < Lines.Families.Count; i++)
            {
                var family = Lines.Families[i];
                var button = Ui.Button(rt, "Pick" + family, "", new Vector2((i - 1) * (ButtonSize.x + 20), 0), ButtonSize,
                    ButtonColor, Theme.GoldBright, () => view.Chosen?.Invoke(family), 34);
                button.Label.rectTransform.anchoredPosition = new Vector2(30, 14);
                button.SetLabel(Labels.Family(family));
                Ui.Outline(button.transform, "Outline", Vector2.zero, ButtonSize, Theme.Gold.WithAlpha(0.7f), 14);
                DrawGlyph(button.transform, family, new Vector2(-ButtonSize.x / 2 + 46, 0));
                view.notes[family] = Ui.Text(button.transform, "Note", "", Fonts.Body, 17, Theme.Ivory, new Vector2(30, -24), new Vector2(170, 24));
                view.buttons[family] = button;
            }
            view.gameObject.SetActive(false);
            return view;
        }

        /// <summary>種類ごとのラインを小さな図で示す。</summary>
        static void DrawGlyph(Transform parent, LineFamily family, Vector2 center)
        {
            const float box = 54f, cell = box / 3f;
            var glyph = Ui.Rect(parent, "Glyph", center, new Vector2(box, box));
            Ui.Panel(glyph, "Back", Vector2.zero, new Vector2(box + 8, box + 8), new Color(0, 0, 0, 0.35f), 8);
            for (int i = 0; i < Lines.CellCount; i++)
            {
                var p = new Vector2((i % 3 - 1) * cell, (1 - i / 3) * cell);
                Ui.Panel(glyph, "Dot" + i, p, new Vector2(cell - 5, cell - 5), Theme.Ivory.WithAlpha(0.18f), 3);
            }
            foreach (var line in Lines.Of(family))
            {
                Vector2 a = new Vector2((line.A % 3 - 1) * cell, (1 - line.A / 3) * cell);
                Vector2 c = new Vector2((line.C % 3 - 1) * cell, (1 - line.C / 3) * cell);
                var bar = Ui.Panel(glyph, "Line" + line.Index, (a + c) / 2, new Vector2(Vector2.Distance(a, c) + 10, 5), Theme.Gold, 2.5f);
                bar.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(c.y - a.y, c.x - a.x) * Mathf.Rad2Deg);
            }
        }

        /// <summary>表示する。estimates が null なら勝率は出さない。</summary>
        public void Show(IReadOnlyList<FamilyEstimate> estimates)
        {
            gameObject.SetActive(true);
            LineFamily? best = estimates != null ? PlayerAdvisor.Best(estimates) : (LineFamily?)null;
            foreach (var family in Lines.Families)
            {
                string note = "ディーラーは残り2種類";
                if (estimates != null)
                {
                    foreach (var e in estimates)
                    {
                        if (e.Family == family)
                            note = $"勝率 {e.WinRate:P0}" + (family == best ? "  ★推奨" : "");
                    }
                }
                notes[family].text = note;
                notes[family].color = family == best ? Theme.GoldBright : Theme.Ivory.WithAlpha(0.75f);
                buttons[family].Interactable = true;
            }
            transform.localScale = Vector3.one * 0.9f;
            Tween.Scale(transform, Vector3.one, 0.25f, Ease.OutBack);
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>キーボード操作: 1 / 2 / 3 で ROW / COLUMN / SLASH。</summary>
        void Update()
        {
            if (Dialog.IsAnyOpen)
                return;
            for (int i = 0; i < Lines.Families.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                    Chosen?.Invoke(Lines.Families[i]);
            }
        }
    }
}
