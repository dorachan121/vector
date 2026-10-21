using UnityEngine;
using Vector.Core;
using Vector.Core.Orbit;
using Vector.Game.Casino;

namespace Vector.Game.Orbit
{
    /// <summary>
    /// ORBIT シーンの起点（コンポジションルート）。ルール・乱数・エンジン・賭け金の管理を組み立て、画面を作って進行役に渡す。
    /// </summary>
    public sealed class OrbitScene : MonoBehaviour
    {
        void Start()
        {
            var rules = OrbitRules.Standard;
            var engine = new OrbitEngine(rules, new CryptoRandom());
            var wallet = ChallengeFlow.WalletFor(GameId.Orbit, rules.MinBet);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = OrbitView.Build(rules);
            gameObject.AddComponent<OrbitDirector>().Begin(rules, engine, view, session, wallet);
        }
    }
}
