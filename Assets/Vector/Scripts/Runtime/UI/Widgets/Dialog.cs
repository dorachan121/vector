using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vector.Game.UI.Widgets
{
    /// <summary>
    /// 画面全体を暗くして前面に出すモーダル。タイトル・本文・任意の中身・下部のボタン列を持つ。
    /// ボタンを押すと閉じてから、そのボタンの処理を呼ぶ。
    /// </summary>
    public sealed class Dialog : MonoBehaviour
    {
        const float ButtonHeight = 64f;

        /// <summary>開いているダイアログ（後ろほど前面）。</summary>
        static readonly List<Dialog> open = new List<Dialog>();

        readonly List<CasinoButton> buttons = new List<CasinoButton>();
        RectTransform panel;
        Vector2 size;

        /// <summary>本文の下の自由に使える領域（設定画面などのコントロールを置く）。</summary>
        public RectTransform Content { get; private set; }

        public event Action Closed;

        /// <summary>ダイアログが1つでも開いていれば true（その間はテーブルの持ち時間やショートカットを止める）。</summary>
        public static bool IsAnyOpen => open.Count > 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => open.Clear();

        public static Dialog Open(Transform root, string title, string body, Vector2 size)
        {
            var shade = Ui.Image(root, "Dialog", null, new Color(0, 0, 0, 0.7f), Vector2.zero, Vector2.zero, true);
            var shadeRect = shade.rectTransform;
            shadeRect.anchorMin = Vector2.zero;
            shadeRect.anchorMax = Vector2.one;
            var dialog = shade.gameObject.AddComponent<Dialog>();
            dialog.size = size;
            open.Add(dialog);

            dialog.panel = Ui.Panel(shadeRect, "Panel", Vector2.zero, size, new Color(0.05f, 0.08f, 0.07f, 0.98f), 24, true).rectTransform;
            Ui.Outline(dialog.panel, "Outline", Vector2.zero, size, Theme.Gold.WithAlpha(0.8f), 24);
            Ui.Text(dialog.panel, "Title", title, Fonts.Label, 38, Theme.Gold, new Vector2(0, size.y / 2 - 44), new Vector2(size.x - 60, 50));
            float bodyHeight = 0;
            if (!string.IsNullOrEmpty(body))
            {
                var text = Ui.Paragraph(dialog.panel, "Body", body, 24, Theme.Ivory, Vector2.zero, new Vector2(size.x - 80, 10), TMPro.TextAlignmentOptions.Top);
                bodyHeight = text.GetPreferredValues(body, size.x - 80, 0).y + 10;
                text.rectTransform.sizeDelta = new Vector2(size.x - 80, bodyHeight);
                text.rectTransform.anchoredPosition = new Vector2(0, size.y / 2 - 84 - bodyHeight / 2);
            }
            float contentTop = size.y / 2 - 90 - bodyHeight;
            float contentBottom = -size.y / 2 + ButtonHeight + 40;
            dialog.Content = Ui.Rect(dialog.panel, "Content", new Vector2(0, (contentTop + contentBottom) / 2), new Vector2(size.x - 60, Mathf.Max(0, contentTop - contentBottom)));

            var group = Ui.Group(shade.gameObject);
            group.alpha = 0;
            Tween.Fade(group, 1, 0.18f);
            dialog.panel.localScale = Vector3.one * 0.92f;
            Tween.Scale(dialog.panel, Vector3.one, 0.25f, Ease.OutBack);
            return dialog;
        }

        public Dialog AddButton(string label, Action onClick, bool primary = false)
        {
            var color = primary ? Theme.Gold : new Color(1, 1, 1, 0.1f);
            var button = Ui.Button(panel, "Button" + label, label, Vector2.zero, new Vector2(240, ButtonHeight), color, primary ? Theme.Ink : Theme.Ivory, () =>
            {
                Close();
                onClick?.Invoke();
            }, 28);
            buttons.Add(button);
            float spacing = 260f;
            for (int i = 0; i < buttons.Count; i++)
                ((RectTransform)buttons[i].transform).anchoredPosition = new Vector2((i - (buttons.Count - 1) / 2f) * spacing, -size.y / 2 + ButtonHeight / 2 + 26);
            return this;
        }

        public void Close()
        {
            if (this == null)
                return;
            Closed?.Invoke();
            Closed = null;
            Destroy(gameObject);
        }

        void Update()
        {
            // Esc で最前面のダイアログだけを閉じる
            if (Input.GetKeyDown(KeyCode.Escape) && open.Count > 0 && open[open.Count - 1] == this)
                Close();
        }

        void OnDestroy() => open.Remove(this);
    }
}
