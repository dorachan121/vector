using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        /// <summary>ダイアログが1つでも開いていれば true（その間はテーブルの持ち時間やショートカットを止める）。</summary>
        public static bool IsAnyOpen => open.Count > 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => open.Clear();

        public static Dialog Open(Transform root, string title, string body, Vector2 size)
        {
            var shade = Ui.Image(root, "Dialog", null, new Color(0.03f, 0.01f, 0.08f, 0.8f), Vector2.zero, Vector2.zero, true);
            var shadeRect = shade.rectTransform;
            // 画面の枠の外（端末の縦横比で余った所）まで暗くする
            Ui.Cover(shadeRect);
            var dialog = shade.gameObject.AddComponent<Dialog>();
            dialog.size = size;
            open.Add(dialog);

            var body0 = Ui.Plate(shadeRect, "Panel", Vector2.zero, size, Palette.PanelTop, 28, true, true);
            dialog.panel = (RectTransform)body0.transform.parent;
            Ui.Image(dialog.panel, "Glow", Art.Glow, Palette.Haze.WithAlpha(0.22f), new Vector2(0, size.y / 2 - 60), new Vector2(size.x * 0.9f, 220));
            if (!string.IsNullOrEmpty(title))
                Ui.Ribbon(dialog.panel, "Title", title, new Vector2(0, size.y / 2 - 2), Mathf.Min(size.x * 0.8f, 640), Palette.Ruby, 32);
            Ui.IconButton(dialog.panel, "Close", IconKind.Close, new Vector2(size.x / 2 - 34, size.y / 2 - 34), 50, new Color(1, 1, 1, 0.1f), dialog.Close);
            float bodyHeight = 0;
            if (!string.IsNullOrEmpty(body))
            {
                var text = Ui.Paragraph(dialog.panel, "Body", body, 24, Palette.Ivory, Vector2.zero, new Vector2(size.x - 80, 10), TMPro.TextAlignmentOptions.Top);
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
            var color = primary ? Palette.Gold : new Color(1, 1, 1, 0.1f);
            var button = Ui.Button(panel, "Button" + label, label, Vector2.zero, new Vector2(240, ButtonHeight), color, primary ? Palette.Ink : Palette.Ivory, () =>
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

        /// <summary>閉じられるまで待つ（コルーチンから yield return で使う）。</summary>
        public IEnumerator WaitUntilClosed()
        {
            while (this != null)
                yield return null;
        }

        public void Close()
        {
            if (this != null)
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
