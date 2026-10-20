using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>盤面の左右に置く陣営の札（陣営名・使うラインの種類・現在の得点）。</summary>
    public sealed class ScorePlate : MonoBehaviour
    {
        static readonly Vector2 PlateSize = new Vector2(170, 168);

        Image plate;
        TextMeshProUGUI family;
        TextMeshProUGUI score;
        Color accent;

        public static ScorePlate Create(Transform parent, Vector2 position, string title, Color accent)
        {
            var rt = Ui.Rect(parent, "Plate" + title, position, PlateSize);
            var view = rt.gameObject.AddComponent<ScorePlate>();
            view.accent = accent;
            view.plate = Ui.Panel(rt, "Plate", Vector2.zero, PlateSize, new Color(0, 0, 0, 0.45f), 20);
            Ui.Outline(rt, "Outline", Vector2.zero, PlateSize, accent.WithAlpha(0.7f), 20);
            Ui.Text(rt, "Title", title, Fonts.Label, 26, accent, new Vector2(0, 56), new Vector2(PlateSize.x, 34));
            view.family = Ui.Text(rt, "Family", "", Fonts.Label, 20, Theme.Ivory.WithAlpha(0.8f), new Vector2(0, 24), new Vector2(PlateSize.x, 26));
            view.score = Ui.Text(rt, "Score", "", Fonts.Display, 64, Theme.Ivory, new Vector2(0, -32), new Vector2(PlateSize.x, 80));
            view.Clear();
            return view;
        }

        public void SetFamily(string text) => family.text = text;

        public void SetScore(int value) => score.text = value.ToString();

        public void SetWinner(bool winner)
        {
            plate.color = winner ? accent.WithAlpha(0.35f) : new Color(0, 0, 0, 0.45f);
            Tween.Scale(transform, Vector3.one * (winner ? 1.08f : 1f), 0.3f, Ease.OutBack);
        }

        public void Clear()
        {
            family.text = "—";
            score.text = "";
            SetWinner(false);
        }
    }
}
