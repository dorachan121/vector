using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>
    /// ベット受付中の操作（チップを置く・取り消す・リベット・倍掛け・ディール）を TableSession に取り次ぎ、
    /// 結果を画面に反映する。ルールの判断はすべて TableSession 側にある。
    /// </summary>
    public sealed class BettingInput
    {
        readonly TableSession session;
        readonly TableView view;
        readonly TableFx fx;
        bool active;

        public bool DealRequested { get; private set; }

        public BettingInput(TableSession session, TableView view, TableFx fx)
        {
            this.session = session;
            this.view = view;
            this.fx = fx;
            foreach (var spot in view.Spots.Values)
                spot.Clicked += OnSpotClicked;
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

        void OnSpotClicked(int id)
        {
            var spot = (BetSpot)id;
            if (!active)
                return;
            long amount = view.Rack.Selected;
            var error = session.TryAddChip(spot, amount);
            if (error != ChipError.None)
            {
                Sfx.Play(SfxKind.Error);
                view.Toast.Show(Labels.ChipProblem(error, spot, session.Rules), Theme.Negative);
                return;
            }
            Sfx.Play(SfxKind.Chip);
            fx.FlyChips(amount, view.Rack.WorldPositionOf(amount), view.Spots[spot].Stack.TopWorldPosition);
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
                view.Toast.Show(Labels.BetProblem(check), Theme.Negative);
                return;
            }
            DealRequested = true;
        }

        public void Refresh()
        {
            foreach (var pair in view.Spots)
            {
                pair.Value.SetAmount(active ? session.Bets[pair.Key] : session.Round.Bets[pair.Key]);
                pair.Value.SetInteractable(active);
            }
            view.Rack.Refresh(session.Available, active);
            view.Actions.Refresh(active, session.Bets.CanUndo, session.CanRebet, session.CanDouble, session.CheckBets().IsValid);
            view.Hud.SetBalance(session.Available, false);
        }
    }
}
