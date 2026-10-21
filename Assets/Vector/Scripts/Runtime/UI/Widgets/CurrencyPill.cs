using System;
using TMPro;
using UnityEngine;

namespace Vector.Game.UI.Widgets
{
    /// <summary>
    /// ヘッダーの所持金・ポイントの表示（左に大きなアイコン、数字、右に「+」ボタン）。数字は変わるときに数え上げる。
    /// </summary>
    public sealed class CurrencyPill : MonoBehaviour
    {
        TextMeshProUGUI value;
        Func<long, string> format;
        long shown = -1;

        public RectTransform Icon { get; private set; }

        public static CurrencyPill Create(Transform parent, Vector2 position, float width, Sprite icon, Color iconColor, Func<long, string> format, Action onPlus = null)
        {
            const float height = 56f;
            var rt = Ui.Rect(parent, "Currency", position, new Vector2(width, height));
            var pill = rt.gameObject.AddComponent<CurrencyPill>();
            pill.format = format;
            Ui.Shadow(rt, "Shadow", new Vector2(0, -3), new Vector2(width, height), height / 2, 0.55f);
            Ui.Sliced(rt, "Body", Chrome.Shade, Palette.NightDeep.WithAlpha(0.92f), Vector2.zero, new Vector2(width, height), height / 2);
            Ui.Sliced(rt, "Rim", Chrome.Rim, Palette.GoldMid.WithAlpha(0.85f), Vector2.zero, new Vector2(width, height), height / 2);
            float right = onPlus != null ? 56 : 18;
            pill.value = Ui.Text(rt, "Value", "", Fonts.Display, 34, Palette.Ivory, new Vector2((height * 0.9f - right) / 2, 1), new Vector2(width - height - right, height), TextAlignmentOptions.Right);
            pill.Icon = Ui.Image(rt, "Icon", icon, iconColor, new Vector2(-width / 2 + 8, 0), Vector2.one * height * 1.18f).rectTransform;
            if (onPlus != null)
                Ui.IconButton(rt, "Plus", IconKind.Plus, new Vector2(width / 2 - 30, 0), 42, Palette.Emerald, onPlus);
            return pill;
        }

        public void SetValue(long amount, bool animate = true)
        {
            long from = shown;
            shown = amount;
            if (!animate || from < 0 || from == amount)
            {
                value.text = format(amount);
                return;
            }
            Tween.Run(value, "count", 0.7f, Ease.OutCubic, t => value.text = format((long)Math.Round(from + (amount - from) * (double)t)));
            Tween.Scale(Icon, Vector3.one * 1.18f, 0.12f, Ease.OutQuad, () => Tween.Scale(Icon, Vector3.one, 0.25f, Ease.OutBack));
        }
    }
}
