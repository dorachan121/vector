using System;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.UI;

namespace Vector.Game.Casino
{
    /// <summary>
    /// ベット受付中の操作（チップを置く・取り消す・リベット・倍掛け・開始）を WagerSession に取り次ぎ、結果を画面に反映する。
    /// ルールの判断はすべて WagerSession 側にある。
    /// </summary>
    public sealed class WagerInput
    {
        readonly WagerSession session;
        readonly CasinoView view;
        readonly Func<int, string> spotName;
        bool active;

        public bool DealRequested { get; private set; }

        public WagerInput(WagerSession session, CasinoView view, Func<int, string> spotName)
        {
            this.session = session;
            this.view = view;
            this.spotName = spotName;
            foreach (var target in view.Targets.Values)
                target.Clicked += OnSpotClicked;
            view.Actions.UndoClicked += OnUndo;
            view.Actions.ClearClicked += OnClear;
            view.Actions.RebetClicked += OnRebet;
            view.Actions.DoubleClicked += OnDouble;
            view.Actions.DealClicked += OnDeal;
        }

        public void Begin()
        {
            active = true;
            DealRequested = false;
            Refresh();
        }

        public void End()
        {
            active = false;
            Refresh();
        }

        void OnSpotClicked(int spot)
        {
            if (!active)
                return;
            long amount = view.Rack.Selected;
            var error = session.TryAddChip(spot, amount);
            if (error != ChipError.None)
            {
                Sfx.Play(SfxKind.Error);
                view.Toast.Show(Labels.ChipProblem(error, spotName(spot), session.Limits.MaxFor(spot)), Theme.Negative);
                return;
            }
            Sfx.Play(SfxKind.Chip);
            ChipFlight.Fly(view.Fx, amount, view.Rack.WorldPositionOf(amount), view.Targets[spot].ChipWorldPosition);
            Refresh();
        }

        void OnUndo()
        {
            if (active && session.TryUndo(out _, out _))
                Sfx.Play(SfxKind.Chip, 0.6f);
            Refresh();
        }

        void OnClear()
        {
            if (!active)
                return;
            session.ClearBets();
            Sfx.Play(SfxKind.ChipsCollect, 0.7f);
            Refresh();
        }

        void OnRebet()
        {
            if (active && session.TryRebet())
                Sfx.Play(SfxKind.ChipsCollect);
            Refresh();
        }

        void OnDouble()
        {
            if (active && session.TryDouble())
                Sfx.Play(SfxKind.ChipsCollect);
            Refresh();
        }

        void OnDeal()
        {
            if (!active)
                return;
            var check = session.CheckBets();
            if (!check.IsValid)
            {
                Sfx.Play(SfxKind.Error);
                view.Toast.Show(Labels.WagerProblem(check, spotName), Theme.Negative);
                return;
            }
            DealRequested = true;
        }

        public void Refresh()
        {
            foreach (var pair in view.Targets)
            {
                pair.Value.SetAmount(active ? session.Bets[pair.Key] : session.InPlay[pair.Key]);
                pair.Value.SetInteractable(active && session.Limits.IsSpotAvailable(pair.Key));
            }
            view.Rack.Refresh(session.Available, active);
            view.Actions.Refresh(active, session.Bets.CanUndo, session.CanRebet, session.CanDouble, session.CheckBets().IsValid);
            view.Hud.SetBalance(session.Available, false);
        }
    }
}
