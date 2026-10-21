using System;
using System.Collections;
using System.Threading.Tasks;
using Vector.Core;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>
    /// 人間のプレイヤー。持ち時間つきでラインの種類と配置を選ばせ、戦略ヒントが有効なら推奨を示す。
    /// 時間切れはアドバイザーの推奨手で自動的に進める。
    /// </summary>
    public sealed class HumanSeat : IPlayerSeat
    {
        readonly TableView view;
        readonly PlayerAdvisor advisor;
        readonly TableRules rules;
        readonly Func<bool> showStrategy;
        readonly Func<bool> paused;

        public HumanSeat(TableView view, PlayerAdvisor advisor, TableRules rules, Func<bool> showStrategy, Func<bool> paused)
        {
            this.view = view;
            this.advisor = advisor;
            this.rules = rules;
            this.showStrategy = showStrategy;
            this.paused = paused;
        }

        public IEnumerator ChooseFamily(RoundEngine round, Action<LineFamily> done)
        {
            view.Casino.Status.Show("CHOOSE YOUR VECTOR", "手札を見て、勝負するラインの種類を選んでください");
            var values = round.Board.ToValues();
            var hand = round.HandValues();
            // アドバイザーは1スレッドでしか使えないので、推奨の計算は常に1つだけ走らせ、終わるまで待ってから先へ進む
            var estimates = Task.Run(() => advisor.EstimateFamilies(values, hand));
            bool hintShown = false;

            LineFamily? choice = null;
            void OnChosen(LineFamily f) => choice = f;
            view.Picker.Chosen += OnChosen;
            view.Picker.Show(null);

            var timer = new CountdownTimer(rules.DecisionSeconds);
            while (choice == null && !timer.Expired)
            {
                if (!hintShown && estimates.IsCompleted && showStrategy())
                {
                    hintShown = true;
                    if (estimates.Status == TaskStatus.RanToCompletion)
                        view.Picker.Show(estimates.Result);
                }
                timer.Tick(view.Casino.Status, paused());
                yield return null;
            }
            view.Picker.Chosen -= OnChosen;
            view.Picker.Hide();
            view.Casino.Status.HideTimer();

            while (!estimates.IsCompleted)
                yield return null;
            if (choice == null)
            {
                if (estimates.IsFaulted)
                    throw estimates.Exception.GetBaseException();
                choice = PlayerAdvisor.Best(estimates.Result);
                view.Casino.Toast.Show($"時間切れ: 推奨の {Labels.Family(choice.Value)} を選びました");
            }
            done(choice.Value);
        }

        public IEnumerator ChoosePlacement(RoundEngine round, Action<Placement> done)
        {
            view.Casino.Status.Show("YOUR TURN", "手札のカードを選び、置きたいマスをクリック");
            var values = round.Board.ToValues();
            var hand = round.HandValues();
            var family = round.PlayerFamily;
            var suggestion = Task.Run(() => advisor.SuggestPlacement(values, hand, family));
            bool hintShown = false;

            int selected = round.Hand.Count == 1 ? 0 : -1;
            Placement? placement = null;
            void OnCard(int index)
            {
                selected = selected == index ? -1 : index;
                view.Hand.Select(selected);
                Sfx.Play(SfxKind.Button, 0.6f);
            }
            void OnCell(int cell)
            {
                if (selected < 0)
                {
                    view.Casino.Toast.Show("先に手札からカードを選んでください");
                    return;
                }
                placement = new Placement(selected, cell);
            }
            view.Hand.CardClicked += OnCard;
            view.Board.CellClicked += OnCell;
            view.Hand.SetSelectable(true);
            view.Hand.Select(selected);
            view.Board.SetSelectable(true);

            var timer = new CountdownTimer(rules.DecisionSeconds);
            while (placement == null && !timer.Expired)
            {
                if (!hintShown && suggestion.Status == TaskStatus.RanToCompletion && showStrategy())
                {
                    hintShown = true;
                    view.Hand.SetHint(suggestion.Result.HandIndex);
                    view.Board.SetHint(suggestion.Result.Cell);
                }
                timer.Tick(view.Casino.Status, paused());
                yield return null;
            }
            view.Hand.CardClicked -= OnCard;
            view.Board.CellClicked -= OnCell;
            view.Hand.SetSelectable(false);
            view.Hand.SetHint(-1);
            view.Board.SetSelectable(false);
            view.Board.SetHint(-1);
            view.Casino.Status.HideTimer();

            while (!suggestion.IsCompleted)
                yield return null;
            if (placement == null)
            {
                if (suggestion.IsFaulted)
                    throw suggestion.Exception.GetBaseException();
                placement = suggestion.Result;
                view.Casino.Toast.Show("時間切れ: 推奨の手で配置しました");
            }
            done(placement.Value);
        }
    }
}
