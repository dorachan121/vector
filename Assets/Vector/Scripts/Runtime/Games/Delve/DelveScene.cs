using UnityEngine;
using Vector.Core;
using Vector.Core.Delve;
using Vector.Game.Casino;

namespace Vector.Game.Delve
{
    /// <summary>
    /// DELVE シーンの起点（コンポジションルート）。ルール・乱数・エンジン・賭け金の管理を組み立て、画面を作って進行役に渡す。
    /// </summary>
    public sealed class DelveScene : MonoBehaviour
    {
        void Start()
        {
            var rules = DelveRules.Standard;
            var random = new CryptoRandom();
            var engine = new DelveEngine(rules, random);
            var wallet = ChallengeFlow.WalletFor(GameId.Delve, rules.MinBet);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = DelveView.Build(rules);
            view.LayoutDoors(rules.Floors[0].Doors);
            gameObject.AddComponent<DelveDirector>().Begin(rules, engine, random, view, session, wallet);
        }
    }
}
