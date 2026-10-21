using System.Collections;
using System.Collections.Generic;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.World;

namespace Vector.Game.Table
{
    /// <summary>
    /// VECTOR のテーブルの進行役。配札 → ライン選択 → 交互の配置 → 勝負 を受け持つ。
    /// ベット受付・精算・チップの演出・障害時の返金・離席などの共通の流れは CasinoDirector が受け持つ。
    /// </summary>
    public sealed class TableDirector : CasinoDirector
    {
        const float ResultSeconds = 2.8f;

        TableRules rules;
        GameMode mode;
        RoundEngine round;
        TableView view;
        TableFx fx;
        RoundPresenter presenter;
        IPlayerSeat seat;
        readonly Scoreboard scoreboard = new Scoreboard();

        public void Begin(TableRules rules, GameMode mode, RoundEngine round, TableView view, WagerSession session, ITableWallet wallet, PlayerAdvisor advisor)
        {
            this.rules = rules;
            this.mode = mode;
            this.round = round;
            this.view = view;
            fx = new TableFx(view);
            presenter = new RoundPresenter(view);
            seat = mode == GameMode.Player
                ? new HumanSeat(view, advisor, rules, () => ShowStrategy, () => Paused)
                : (IPlayerSeat)new AiSeat(view, advisor);
            Begin(view.Casino, session, wallet);
        }

        protected override GameId Game => GameId.Vector;
        protected override float BettingSeconds => rules.BettingSeconds;
        protected override long TableMinimum => rules.MinBet;

        protected override string BettingHint => mode == GameMode.Player
            ? "MAIN にチップを置いて DEAL（BONUS は任意）"
            : "PLAYER / DEALER / TIE にチップを置いて DEAL";

        protected override string SpotName(int spot) => Labels.Spot((BetSpot)spot);

        protected override IReadOnlyList<(string title, string body)> HelpPages => VectorHelp.Pages(rules);

        protected override IEnumerator BeforeBetting()
        {
            if (round.BeginBetting())
            {
                scoreboard.Clear();
                yield return Shuffle();
            }
            RefreshTableInfo();
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            round.Deal(mode, inPlay);
            yield return AnnounceTurnOrder(mode == GameMode.Player ? You : new TurnSeat("PLAYER", Theme.PlayerBlue, Cast.Player), Dealer);
            yield return Deal();
            yield return ChooseFamily();
            while (round.Phase != RoundPhase.Showdown)
                yield return round.Phase == RoundPhase.PlayerTurn ? PlayerTurn() : DealerTurn();
            yield return Showdown();
        }

        protected override void AbortRound() => round.Abort();

        protected override IEnumerator ResetTable()
        {
            presenter.Reset();
            yield return fx.CollectCards();
        }

        /// <summary>精算したラウンドの出来事（クエストの条件用）。</summary>
        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (RoundResult)outcome;
            var tags = new List<string>();
            if (result.Winner == RoundWinner.Player && result.PlayerFamily == LineFamily.Slash)
                tags.Add(RoundTags.VectorSlashWin);
            foreach (var bet in result.Bets)
            {
                if (bet.Spot == (int)BetSpot.Bonus && bet.Outcome == BetOutcome.Win)
                    tags.Add(RoundTags.VectorBonus);
            }
            return tags;
        }

        // ------------------------------------------------------------------
        // ラウンド
        // ------------------------------------------------------------------

        IEnumerator Shuffle()
        {
            View.Status.Show("SHUFFLING", "新しいシューを用意しています");
            Sfx.Play(SfxKind.Shuffle);
            view.Scoreboard.Render(scoreboard, round.Shoe.ShoeNumber);
            yield return Tween.Wait(1.1f);
            View.Toast.Show($"新しいシュー #{round.Shoe.ShoeNumber}  ·  バーンカード {Labels.CardName(round.Shoe.BurnedCard)}");
        }

        IEnumerator Deal()
        {
            View.Status.Show("NO MORE BETS");
            RefreshTableInfo();
            yield return Tween.Wait(0.4f);

            yield return fx.DealToBoard(round.Board[Lines.CenterCell].Value, Lines.CenterCell, Seat.House);
            for (int i = 0; i < round.Hand.Count; i++)
            {
                yield return fx.DealToHand(round.Hand[i], i);
                yield return Tween.Wait(0.06f);
            }
            RefreshShoe();
            yield return Tween.Wait(0.3f);

            if (HasBonusBet && rules.BonusPays.ContainsKey(round.BonusHand))
            {
                view.Paytable.Highlight(VectorPaytable.Key(round.BonusHand));
                View.Toast.Show($"BONUS HIT!  {Labels.HandName(round.BonusHand)}", Theme.GoldBright);
                Sfx.Play(SfxKind.Win, 0.7f);
            }
        }

        IEnumerator ChooseFamily()
        {
            LineFamily family = default;
            yield return seat.ChooseFamily(round, f => family = f);
            round.ChooseFamily(family);
            Sfx.Play(SfxKind.Chip);
            UpdateScores();
        }

        IEnumerator PlayerTurn()
        {
            Placement placement = default;
            yield return seat.ChoosePlacement(round, p => placement = p);
            round.PlacePlayerCard(placement.HandIndex, placement.Cell);
            yield return fx.MoveFromHand(placement.HandIndex, placement.Cell, Seat.Player);
            UpdateScores();
        }

        IEnumerator DealerTurn()
        {
            View.Status.Show("DEALER'S TURN", "ディーラーがシューから1枚引いて置きます");
            yield return Tween.Wait(0.3f);
            DealerMove move = default;
            var engine = round;
            yield return Background.Run(() => engine.PlayDealerCard(), m => move = m);
            yield return fx.DealToBoard(move.Card, move.Cell, Seat.Dealer);
            RefreshShoe();
            UpdateScores();
        }

        IEnumerator Showdown()
        {
            var result = round.Settle();
            scoreboard.Add(result.Winner);
            yield return presenter.ShowResult(result, HasBonusBet);
            yield return Settle(result, result.Bets);
            view.Scoreboard.Render(scoreboard, round.Shoe.ShoeNumber);
            yield return Tween.Wait(ResultSeconds);
        }

        // ------------------------------------------------------------------
        // 補助
        // ------------------------------------------------------------------

        bool HasBonusBet => round.Bets[(int)BetSpot.Bonus] > 0;

        void UpdateScores() => presenter.ShowScores(round.Board.ToValues(), round.PlayerFamily);

        void RefreshTableInfo()
        {
            // ベット受付中は、これから配るラウンドの番号を出す
            int number = round.Phase == RoundPhase.Betting ? round.RoundNumber + 1 : round.RoundNumber;
            View.Hud.SetInfo($"SHOE #{round.Shoe.ShoeNumber}  ·  ROUND {number}");
            view.Scoreboard.Render(scoreboard, round.Shoe.ShoeNumber);
            RefreshShoe();
        }

        void RefreshShoe() => view.Shoe.SetRemaining(round.Shoe.Remaining, round.Shoe.TotalCards);
    }
}
