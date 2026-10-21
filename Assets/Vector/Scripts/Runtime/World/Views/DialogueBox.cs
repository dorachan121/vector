using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector.Game.Audio;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>
    /// 画面下の会話の窓。話す人の顔と名前、1文字ずつ出る台詞、選択肢を出す。
    /// クリック・Space・Enter で台詞を最後まで出し、もう一度で次へ進む。
    /// </summary>
    public sealed class DialogueBox : MonoBehaviour, IPointerClickHandler
    {
        static readonly Vector2 BoxSize = new Vector2(1640, 230);
        const float CharactersPerSecond = 48f;

        readonly List<CasinoButton> choices = new List<CasinoButton>();
        TextMeshProUGUI speaker;
        TextMeshProUGUI title;
        TextMeshProUGUI body;
        TextMeshProUGUI next;
        RectTransform portraitSlot;
        CanvasGroup group;
        bool advance;
        int chosen = -1;

        public bool IsOpen => gameObject.activeSelf;

        public static DialogueBox Create(Transform parent)
        {
            var shade = Ui.Image(parent, "Dialogue", null, new Color(0, 0, 0, 0.001f), Vector2.zero, Vector2.zero, true);
            shade.rectTransform.anchorMin = Vector2.zero;
            shade.rectTransform.anchorMax = Vector2.one;
            var box = shade.gameObject.AddComponent<DialogueBox>();
            box.group = Ui.Group(shade.gameObject);
            var panel = Ui.Panel(shade.rectTransform, "Panel", new Vector2(0, -400), BoxSize, new Color(0.03f, 0.03f, 0.05f, 0.95f), 26);
            Ui.Outline(panel.transform, "Outline", Vector2.zero, BoxSize, Theme.Gold.WithAlpha(0.8f), 26);
            box.portraitSlot = Ui.Rect(panel.transform, "Portrait", new Vector2(-BoxSize.x / 2 + 120, 0), new Vector2(180, 180));
            Ui.Image(box.portraitSlot, "Glow", Art.Glow, Theme.Gold.WithAlpha(0.2f), Vector2.zero, new Vector2(220, 220));
            box.speaker = Ui.Text(panel.transform, "Speaker", "", Fonts.Body, 30, Theme.GoldBright, new Vector2(-BoxSize.x / 2 + 440, 80), new Vector2(400, 40), TextAlignmentOptions.Left);
            box.title = Ui.Text(panel.transform, "Title", "", Fonts.Body, 18, Theme.Muted, new Vector2(-BoxSize.x / 2 + 640, 80), new Vector2(400, 30), TextAlignmentOptions.Left);
            box.body = Ui.Paragraph(panel.transform, "Body", "", 28, Theme.Ivory, new Vector2(110, -18), new Vector2(BoxSize.x - 480, 150));
            box.body.richText = true;
            box.next = Ui.Text(panel.transform, "Next", "▼", Fonts.Body, 22, Theme.GoldBright, new Vector2(BoxSize.x / 2 - 40, -BoxSize.y / 2 + 28), new Vector2(40, 30));
            shade.gameObject.SetActive(false);
            return box;
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (choices.Count == 0)
                advance = true;
        }

        void Update()
        {
            if (Dialog.IsAnyOpen || choices.Count > 0)
                return;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Z))
                advance = true;
        }

        void Open(string name, string role, CharacterLook? look)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                transform.SetAsLastSibling();
                group.alpha = 0;
                Tween.Fade(group, 1, 0.15f);
            }
            speaker.text = name;
            title.text = role;
            foreach (Transform child in portraitSlot)
            {
                if (child.name != "Glow")
                    Destroy(child.gameObject);
            }
            if (look.HasValue)
                CharacterView.Create(portraitSlot, "Face", look.Value, 170, new Vector2(0, -10));
        }

        /// <summary>台詞を1つ出し、プレイヤーが読み進めるまで待つ。</summary>
        public IEnumerator Say(string name, string role, CharacterLook? look, string text)
        {
            Open(name, role, look);
            yield return Type(text);
            next.enabled = true;
            advance = false;
            while (!advance)
                yield return null;
            advance = false;
            Sfx.Play(SfxKind.Button, 0.4f);
        }

        /// <summary>台詞と選択肢を出し、選ばれた番号を done に渡す。</summary>
        public IEnumerator Choose(string name, string role, CharacterLook? look, string text, IReadOnlyList<string> options, Action<int> done)
        {
            Open(name, role, look);
            yield return Type(text);
            next.enabled = false;
            chosen = -1;
            for (int i = 0; i < options.Count; i++)
            {
                int index = i;
                var button = Ui.Button(transform, "Choice" + i, options[i], new Vector2(560, -236 + (options.Count - 1 - i) * 74), new Vector2(380, 64),
                    i == 0 ? Theme.Gold : new Color(1, 1, 1, 0.12f), i == 0 ? Theme.Ink : Theme.Ivory, () => chosen = index, 24, 18);
                button.Label.font = Fonts.Body;
                choices.Add(button);
            }
            while (chosen < 0)
            {
                for (int i = 0; i < choices.Count && i < 9; i++)
                {
                    if (!Dialog.IsAnyOpen && (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)))
                        chosen = i;
                }
                if (!Dialog.IsAnyOpen && Input.GetKeyDown(KeyCode.Escape))
                    chosen = options.Count - 1;
                yield return null;
            }
            foreach (var b in choices)
                Destroy(b.gameObject);
            choices.Clear();
            done(chosen);
        }

        IEnumerator Type(string text)
        {
            next.enabled = false;
            body.text = text;
            body.maxVisibleCharacters = 0;
            advance = false;
            float shown = 0;
            int total = text.Length;
            while (shown < total)
            {
                if (advance)
                {
                    advance = false;
                    break;
                }
                shown += Time.unscaledDeltaTime * CharactersPerSecond * Tween.Speed;
                body.maxVisibleCharacters = (int)shown;
                yield return null;
            }
            body.maxVisibleCharacters = 99999;
        }

        public void Close()
        {
            if (!gameObject.activeSelf)
                return;
            foreach (var b in choices)
                Destroy(b.gameObject);
            choices.Clear();
            gameObject.SetActive(false);
        }
    }
}
