using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Vector.Game.Audio;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>
    /// 画面下の会話の窓。話す人の立ち絵と名札、1文字ずつ出る台詞、選択肢を出す。画面の下端に付く。
    /// タップ・Space・Enter で台詞を最後まで出し、もう一度で次へ進む。
    /// </summary>
    public sealed class DialogueBox : MonoBehaviour, IPointerClickHandler
    {
        static readonly Vector2 PanelSize = new Vector2(1020, 360);
        static readonly Vector2 PanelCenter = new Vector2(0, -60);
        const float CharactersPerSecond = 48f;

        readonly List<CasinoButton> choices = new List<CasinoButton>();
        RectTransform box;
        RectTransform portraitSlot;
        GameObject nameplate;
        TextMeshProUGUI speaker;
        TextMeshProUGUI title;
        TextMeshProUGUI body;
        TextMeshProUGUI next;
        CanvasGroup group;

        /// <summary>いま立ち絵を出している話し手（同じ人が続けて話すあいだは描き直さない）。</summary>
        string portraitOf;
        bool advance;
        int chosen = -1;

        public static DialogueBox Create(Transform parent)
        {
            var shade = Ui.Image(parent, "Dialogue", null, new Color(0, 0, 0, 0.001f), Vector2.zero, Vector2.zero, true);
            shade.rectTransform.anchorMin = Vector2.zero;
            shade.rectTransform.anchorMax = Vector2.one;
            var dialogue = shade.gameObject.AddComponent<DialogueBox>();
            dialogue.group = Ui.Group(shade.gameObject);
            var box = dialogue.box = Ui.Edge(shade.rectTransform, "Box", false, Ui.Portrait.x, 560);
            Ui.Image(box, "Fade", Chrome.Gradient(Palette.NightDeep.WithAlpha(0f), Palette.NightDeep.WithAlpha(0.9f)), Color.white, new Vector2(0, -40), new Vector2(Ui.Portrait.x * 3, 640));
            dialogue.portraitSlot = Ui.Rect(box, "Portrait", new Vector2(-300, 230), new Vector2(380, 470));

            Ui.Plate(box, "Panel", PanelCenter, PanelSize, Palette.PanelTop, 30);
            float left = -PanelSize.x / 2, top = PanelCenter.y + PanelSize.y / 2;
            var plate = Ui.Plate(box, "Nameplate", new Vector2(left + 220, top), new Vector2(380, 70), Palette.Ruby, 22);
            dialogue.nameplate = plate.transform.parent.gameObject;
            dialogue.speaker = Ui.Text(plate.transform.parent, "Speaker", "", Fonts.Body, 32, Palette.Ivory, new Vector2(0, 1), new Vector2(360, 44));
            Ui.Outline(dialogue.speaker, Palette.Ink, 0.2f);
            dialogue.title = Ui.Text(box, "Title", "", Fonts.Body, 20, Palette.GoldLight, new Vector2(left + 600, top + 16), new Vector2(360, 30), TextAlignmentOptions.Left);
            dialogue.body = Ui.Paragraph(box, "Body", "", 30, Palette.Ivory, PanelCenter + new Vector2(0, -24), PanelSize - new Vector2(80, 120));
            dialogue.body.richText = true;
            dialogue.next = Ui.Text(box, "Next", "▼", Fonts.Body, 26, Palette.GoldLight, PanelCenter + new Vector2(PanelSize.x / 2 - 44, -PanelSize.y / 2 + 34), new Vector2(40, 34));
            shade.gameObject.SetActive(false);
            return dialogue;
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
                box.anchoredPosition = new Vector2(0, -40);
                Tween.Move(box, Vector2.zero, 0.25f, Ease.OutCubic);
            }
            nameplate.SetActive(!string.IsNullOrEmpty(name));
            speaker.text = name;
            title.text = role;
            string portrait = look.HasValue ? name : null;
            if (portrait == portraitOf)
                return;
            portraitOf = portrait;
            foreach (Transform child in portraitSlot)
                Destroy(child.gameObject);
            if (look.HasValue)
            {
                Ui.Image(portraitSlot, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.3f), Vector2.zero, new Vector2(420, 420));
                PortraitView.Create(portraitSlot, "Bust", look.Value, Vector2.zero, 470);
            }
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

        /// <summary>台詞と選択肢を出し、選ばれた番号を done に渡す。選択肢は会話の窓の右上に縦に並べる。</summary>
        public IEnumerator Choose(string name, string role, CharacterLook? look, string text, IReadOnlyList<string> options, Action<int> done)
        {
            Open(name, role, look);
            yield return Type(text);
            next.enabled = false;
            chosen = -1;
            float bottom = PanelCenter.y + PanelSize.y / 2 + 70;
            for (int i = 0; i < options.Count; i++)
            {
                int index = i;
                var button = Ui.Button(box, "Choice" + i, options[i], new Vector2(250, bottom + (options.Count - 1 - i) * 96), new Vector2(460, 82),
                    i == 0 ? Palette.Gold : new Color(1, 1, 1, 0.12f), i == 0 ? Palette.Ink : Palette.Ivory, () => chosen = index, 30, 22);
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
