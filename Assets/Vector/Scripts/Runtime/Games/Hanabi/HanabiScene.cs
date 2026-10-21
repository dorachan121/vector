using UnityEngine;
using Vector.Core;
using Vector.Core.Hanabi;
using Vector.Game.Casino;

namespace Vector.Game.Hanabi
{
    /// <summary>
    /// HANABI シーンの起点（コンポジションルート）。ルール・乱数・エンジン・賭け金の管理を組み立て、画面を作って進行役に渡す。
    /// </summary>
    public sealed class HanabiScene : MonoBehaviour
    {
        void Start()
        {
            var rules = HanabiRules.Standard;
            var engine = new HanabiEngine(rules, new CryptoRandom());
            var wallet = ChallengeFlow.WalletFor(GameId.Hanabi, rules.MinBet);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = HanabiView.Build(rules);
            gameObject.AddComponent<HanabiDirector>().Begin(rules, engine, view, session, wallet);
        }
    }
}
