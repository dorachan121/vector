using System;
using UnityEngine;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Casino
{
    /// <summary>ベット受付中の操作ボタン（取り消し・クリア・リベット・倍掛け・ディール）。</summary>
    public sealed class ActionBar : MonoBehaviour
    {
        static readonly Color ButtonColor = new Color(0.12f, 0.16f, 0.14f, 0.95f);

        CasinoButton undo;
        CasinoButton clear;
        CasinoButton rebet;
        CasinoButton doubleUp;
        CasinoButton deal;

        public event Action UndoClicked;
        public event Action ClearClicked;
        public event Action RebetClicked;
        public event Action DoubleClicked;
        public event Action DealClicked;

        public static ActionBar Create(Transform parent, Vector2 position)
        {
            var rt = Ui.Rect(parent, "Actions", position, new Vector2(760, 90));
            var bar = rt.gameObject.AddComponent<ActionBar>();
            var small = new Vector2(122, 64);
            bar.undo = Ui.Button(rt, "Undo", "UNDO", new Vector2(-310, 0), small, ButtonColor, Theme.Ivory, () => bar.UndoClicked?.Invoke(), 26);
            bar.clear = Ui.Button(rt, "Clear", "CLEAR", new Vector2(-180, 0), small, ButtonColor, Theme.Ivory, () => bar.ClearClicked?.Invoke(), 26);
            bar.rebet = Ui.Button(rt, "Rebet", "REBET", new Vector2(-50, 0), small, ButtonColor, Theme.Ivory, () => bar.RebetClicked?.Invoke(), 26);
            bar.doubleUp = Ui.Button(rt, "Double", "x2", new Vector2(80, 0), small, ButtonColor, Theme.Ivory, () => bar.DoubleClicked?.Invoke(), 30);
            bar.deal = Ui.Button(rt, "Deal", "DEAL", new Vector2(265, 0), new Vector2(210, 80), Theme.Gold, Theme.Ink, () => bar.DealClicked?.Invoke(), 38, 40);
            return bar;
        }

        /// <summary>キーボード操作: Space / Enter でディール、Backspace で取り消し。</summary>
        void Update()
        {
            if (Dialog.IsAnyOpen)
                return;
            if (deal.Interactable && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                DealClicked?.Invoke();
            else if (undo.Interactable && Input.GetKeyDown(KeyCode.Backspace))
                UndoClicked?.Invoke();
        }

        /// <summary>開始ボタンの文言（ゲームによって DEAL / FIGHT / SPIN など）。</summary>
        public void SetDealLabel(string label) => deal.SetLabel(label);

        public void Refresh(bool betting, bool canUndo, bool canRebet, bool canDouble, bool canDeal)
        {
            undo.Interactable = betting && canUndo;
            clear.Interactable = betting && canUndo;
            rebet.Interactable = betting && canRebet;
            doubleUp.Interactable = betting && canDouble;
            deal.Interactable = betting && canDeal;
        }
    }
}
