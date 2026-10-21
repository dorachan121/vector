using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Alchemia;
using Vector.Game.UI;

namespace Vector.Game.Alchemia
{
    /// <summary>盤面の1つのシンボル。元素の色と、位ごとの形（素の球 → … → 賢者の石）で描く。</summary>
    public sealed class SymbolView : MonoBehaviour
    {
        Image glow;
        Image shape;
        Image shine;
        float cellSize;

        public Symbol Symbol { get; private set; }
        public RectTransform Rect => (RectTransform)transform;

        public static SymbolView Create(Transform parent, float cellSize, Vector2 position)
        {
            var rt = Ui.Rect(parent, "Symbol", position, Vector2.one * cellSize);
            var view = rt.gameObject.AddComponent<SymbolView>();
            view.cellSize = cellSize;
            view.glow = Ui.Image(rt, "Glow", Art.Glow, Color.clear, Vector2.zero, Vector2.one * cellSize * 1.4f);
            view.shape = Ui.Image(rt, "Shape", null, Color.white, Vector2.zero, Vector2.one * cellSize);
            view.shine = Ui.Image(rt, "Shine", Art.Glow, new Color(1, 1, 1, 0.35f), new Vector2(-cellSize * 0.14f, cellSize * 0.16f), Vector2.one * cellSize * 0.32f);
            return view;
        }

        public void Set(Symbol symbol)
        {
            Symbol = symbol;
            gameObject.SetActive(!symbol.IsEmpty);
            if (symbol.IsEmpty)
                return;
            int tier = symbol.Tier;
            var color = AlchemiaLabels.ColorOf(symbol.Element);
            shape.sprite = Art.Gem(tier);
            shape.color = Color.Lerp(color, Color.white, 0.08f * (tier - 1));
            float scale = 0.62f + 0.08f * tier;
            shape.rectTransform.sizeDelta = Vector2.one * cellSize * scale;
            glow.color = color.WithAlpha(0.12f + 0.12f * tier);
            glow.rectTransform.sizeDelta = Vector2.one * cellSize * (1.1f + 0.15f * tier);
            shine.enabled = tier >= 2;
        }

        /// <summary>融合の前に塊を光らせる。</summary>
        public void Highlight() => Tween.Tint(glow, Color.white.WithAlpha(0.85f), 0.15f);

        /// <summary>位が上がった瞬間の弾み。</summary>
        public void Pop()
        {
            Rect.localScale = Vector3.one * 0.4f;
            Tween.Scale(Rect, Vector3.one, 0.32f, Ease.OutBack);
        }
    }
}
