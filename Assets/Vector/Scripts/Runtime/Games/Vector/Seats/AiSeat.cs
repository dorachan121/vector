using System;
using System.Collections;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>観戦モードのAIプレイヤー。戦略アドバイザーの推奨どおりに打つ。</summary>
    public sealed class AiSeat : IPlayerSeat
    {
        const float ThinkingPause = 0.45f;

        readonly TableView view;
        readonly PlayerAdvisor advisor;

        public AiSeat(TableView view, PlayerAdvisor advisor)
        {
            this.view = view;
            this.advisor = advisor;
        }

        public IEnumerator ChooseFamily(RoundEngine round, Action<LineFamily> done)
        {
            view.Casino.Status.Show("PLAYER IS CHOOSING", "AIプレイヤーがラインの種類を選んでいます");
            var values = round.Board.ToValues();
            var hand = round.HandValues();
            FamilyEstimate[] estimates = null;
            yield return Background.Run(() => advisor.EstimateFamilies(values, hand), r => estimates = r);
            yield return Tween.Wait(ThinkingPause);
            done(PlayerAdvisor.Best(estimates));
        }

        public IEnumerator ChoosePlacement(RoundEngine round, Action<Placement> done)
        {
            view.Casino.Status.Show("PLAYER'S TURN", "AIプレイヤーが考えています");
            var values = round.Board.ToValues();
            var hand = round.HandValues();
            var family = round.PlayerFamily;
            Placement placement = default;
            yield return Background.Run(() => advisor.SuggestPlacement(values, hand, family), p => placement = p);
            yield return Tween.Wait(ThinkingPause);
            done(placement);
        }
    }
}
