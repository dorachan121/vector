using UnityEngine;
using Vector.Core;
using Vector.Core.Derby;
using Vector.Game.Casino;

namespace Vector.Game.Derby
{
    /// <summary>
    /// FOUR BEASTS DERBY シーンの起点（コンポジションルート）。ルール・乱数・エンジン・賭け金の管理を組み立て、画面を作って進行役に渡す。
    /// </summary>
    public sealed class DerbyScene : MonoBehaviour
    {
        void Start()
        {
            var rules = DerbyRules.Standard;
            var engine = new DerbyEngine(rules, new CryptoRandom());
            var wallet = ChallengeFlow.WalletFor(GameId.Derby, rules.ExactaMinBet);
            var session = new WagerSession(engine.Limits, new Bankroll(wallet.Balance));
            var view = DerbyView.Build(rules);
            gameObject.AddComponent<DerbyDirector>().Begin(rules, engine, view, session, wallet);
        }
    }
}
