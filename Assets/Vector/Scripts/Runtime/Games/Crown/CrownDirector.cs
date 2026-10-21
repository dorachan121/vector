using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vector.Core;
using Vector.Core.Crown;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Crown
{
    /// <summary>
    /// CROWN DICE の進行役。ベット受付 → 王冠のダイス → プレイヤーの振り直し → ディーラーのハウスウェイ → 精算 を繰り返す。
    /// </summary>
    public sealed class CrownDirector : CasinoDirector
    {
        const float ResultSeconds = 2.4f;
        const int AllHeld = (1 << DiceRank.DiceCount) - 1;

        CrownRules rules;
        CrownEngine engine;
        CrownAdvisor advisor;
        CrownView view;
        Task<double> optimalReturn;
        int holdMask;
        bool rerollRequested;
        bool standRequested;
        bool choosing;

        /// <summary>戦略ヒントを表示中なら、その計算に使った最善戦略（期待値の表示用）。</summary>
        CrownStrategy hintStrategy;

        public void Begin(CrownRules rules, CrownEngine engine, CrownAdvisor advisor, CrownView view, WagerSession session, ITableWallet wallet)
        {
            this.rules = rules;
            this.engine = engine;
            this.advisor = advisor;
            this.view = view;
            // 6通りの王冠の目の最善戦略を先に解いておく（ヒントがすぐ出せるうえ、遊び方に厳密な RTP を出せる）
            optimalReturn = Task.Run(advisor.ExpectedReturn);
            view.DieClicked += OnDieClicked;
            view.RerollClicked += () => rerollRequested = choosing && holdMask != AllHeld;
            view.StandClicked += () => standRequested = choosing;
            Begin(view.Casino, session, wallet);
        }

        protected override GameId Game => GameId.Crown;
        protected override float BettingSeconds => rules.BettingSeconds;
        protected override long TableMinimum => rules.MinBet;
        protected override string BettingHint => "ANTE にチップを置いて ROLL（OPENING は任意）";
        protected override string SpotName(int spot) => CrownLabels.Spot(spot);
        protected override IReadOnlyList<(string title, string body)> HelpPages
            => CrownHelp.Pages(rules, optimalReturn.Status == TaskStatus.RanToCompletion ? optimalReturn.Result : (double?)null);

        void Update() => view?.PollKeys();

        protected override IReadOnlyCollection<string> TagsFor(IRoundOutcome outcome)
        {
            var result = (CrownResult)outcome;
            var tags = new List<string>();
            if (result.PlayerRank.Kind == DiceHandKind.Yacht)
                tags.Add(RoundTags.CrownYacht);
            if (result.CrownYachtBonus)
                tags.Add(RoundTags.CrownCrownYacht);
            return tags;
        }

        protected override IEnumerator PlayRound(BetBoard inPlay)
        {
            engine.BeginBetting();
            engine.Deal(inPlay);
            int crown = engine.Crown;
            // 最善戦略はワーカースレッドで解いておく（王冠の目ごとに一度だけ）
            var strategy = Task.Run(() => advisor.For(crown));
            View.Hud.SetInfo($"ROUND {engine.RoundNumber}");
            yield return AnnounceTurnOrder(You, Dealer);

            View.Status.Show("CROWN ROLL", "王冠のダイスが、このラウンドの最強の目を決めます");
            yield return view.RollCrown(crown);
            View.Status.Show("YOUR ROLL");
            yield return view.RollPlayer(engine.PlayerDice, 0, crown);
            view.SetPlayerRank(CrownLabels.Rank(engine.PlayerRank, crown));
            if (inPlay[CrownRules.Opening] > 0 && rules.OpeningPays.ContainsKey(engine.Opening))
            {
                view.Paytable.Highlight(engine.Opening.ToString());
                View.Toast.Show($"OPENING HIT!  {CrownLabels.Opening(engine.Opening)}", Theme.GoldBright);
                Sfx.Play(SfxKind.Win, 0.7f);
            }

            while (engine.Phase == CrownPhase.PlayerTurn)
            {
                yield return ChooseHold(strategy);
                if (standRequested)
                {
                    engine.Stand();
                    break;
                }
                int held = holdMask;
                engine.Reroll(held);
                yield return view.RollPlayer(engine.PlayerDice, held, crown);
                view.SetPlayerRank(CrownLabels.Rank(engine.PlayerRank, crown));
            }
            view.ShowHolds(0);
            yield return DealerTurn(crown);

            var result = engine.Settle();
            view.ShowVerdict(result.Winner);
            if (result.Winner == RoundWinner.Player)
                view.Paytable.Highlight("win");
            else if (result.Winner == RoundWinner.Tie)
                view.Paytable.Highlight("tie");
            if (result.CrownYachtBonus)
                view.Paytable.Highlight("bonus");
            view.Flash(result.Winner == RoundWinner.Player ? "YOU WIN" : result.Winner == RoundWinner.Dealer ? "DEALER WINS" : "PUSH",
                Theme.ColorFor(result.Winner));
            View.Status.Show(result.Winner == RoundWinner.Player ? "YOU WIN" : result.Winner == RoundWinner.Dealer ? "DEALER WINS" : "PUSH",
                CrownLabels.Versus(result.PlayerRank, result.DealerRank, crown));
            yield return Settle(result, result.Bets);
            yield return Tween.Wait(ResultSeconds);
        }

        IEnumerator ChooseHold(Task<CrownStrategy> strategy)
        {
            holdMask = 0;
            rerollRequested = standRequested = false;
            choosing = true;
            view.ShowHolds(0);
            view.SetPlayerSelectable(true);
            View.Status.Show("HOLD OR STAND", "残す目をクリックして REROLL、この手で勝負するなら STAND");
            bool hintShown = false;
            HoldAdvice advice = default;
            hintStrategy = null;
            RefreshNotes();

            var timer = new CountdownTimer(rules.DecisionSeconds);
            while (!rerollRequested && !standRequested && !timer.Expired)
            {
                if (!hintShown && strategy.IsCompleted && ShowStrategy)
                {
                    hintShown = true;
                    if (strategy.Status == TaskStatus.RanToCompletion)
                    {
                        advice = strategy.Result.Advise(engine.PlayerDice, engine.PlayerRerollsLeft);
                        view.ShowHint(advice.Stand ? -1 : advice.HoldMask);
                        View.Status.Show("HOLD OR STAND", advice.Stand
                            ? $"推奨: STAND（期待値 x{advice.ExpectedReturn:F2}）"
                            : $"推奨: 光っている目を残して REROLL（期待値 x{advice.ExpectedReturn:F2}）");
                        hintStrategy = strategy.Result;
                        RefreshNotes();
                    }
                }
                timer.Tick(View.Status, Paused);
                yield return null;
            }
            choosing = false;
            view.SetPlayerSelectable(false);
            view.ShowHint(-1);
            view.ShowControls(false);
            View.Status.HideTimer();

            if (!rerollRequested && !standRequested)
            {
                while (!strategy.IsCompleted)
                    yield return null;
                if (strategy.IsFaulted)
                    throw strategy.Exception.GetBaseException();
                advice = strategy.Result.Advise(engine.PlayerDice, engine.PlayerRerollsLeft);
                if (advice.Stand)
                    standRequested = true;
                else
                    holdMask = advice.HoldMask;
                View.Toast.Show("時間切れ: 推奨の手で進めました");
            }
            Sfx.Play(SfxKind.Chip);
        }

        void OnDieClicked(int index)
        {
            if (!choosing)
                return;
            holdMask ^= 1 << index;
            view.ShowHolds(holdMask);
            Sfx.Play(SfxKind.Button, 0.6f);
            RefreshNotes();
        }

        /// <summary>REROLL / STAND のボタンの補足（ヒント表示中は期待値、そうでなければ振り直す個数）。</summary>
        void RefreshNotes()
        {
            var strategy = hintStrategy;
            int left = engine.PlayerRerollsLeft;
            string rerollNote, standNote;
            if (strategy != null)
            {
                rerollNote = holdMask == AllHeld ? "残す目を減らしてください" : $"期待値 x{strategy.HoldValue(engine.PlayerDice, holdMask, left):F2}";
                standNote = $"期待値 x{strategy.StandValue(engine.PlayerDice):F2}";
            }
            else
            {
                int held = 0;
                for (int i = 0; i < DiceRank.DiceCount; i++)
                    held += (holdMask >> i) & 1;
                rerollNote = $"{DiceRank.DiceCount - held} 個を振り直す";
                standNote = "この手で勝負";
            }
            view.ShowControls(true, left, rerollNote, standNote);
            view.SetRerollEnabled(holdMask != AllHeld);
        }

        IEnumerator DealerTurn(int crown)
        {
            View.Status.Show("DEALER'S TURN", "ディーラーはあなたの手を見て、勝つ見込みが最も高い目を残して振ります");
            yield return Tween.Wait(0.4f);
            while (engine.Phase == CrownPhase.DealerTurn)
            {
                var roll = engine.PlayDealer();
                if (roll.Stood)
                {
                    View.Toast.Show("DEALER STANDS");
                    break;
                }
                if (roll.HeldMask != 0)
                {
                    view.ShowDealerHolds(roll.HeldMask);
                    yield return Tween.Wait(0.5f);
                }
                yield return view.RollDealer(roll.Dice, roll.HeldMask, crown);
                view.ShowDealerHolds(0);
                view.SetDealerRank(CrownLabels.Rank(engine.DealerRank, crown));
                yield return Tween.Wait(0.35f);
            }
        }

        protected override void AbortRound()
        {
            engine.Abort();
            choosing = false;
            view.ShowControls(false);
            view.SetPlayerSelectable(false);
        }

        protected override IEnumerator ResetTable()
        {
            view.Reset();
            yield break;
        }
    }
}
