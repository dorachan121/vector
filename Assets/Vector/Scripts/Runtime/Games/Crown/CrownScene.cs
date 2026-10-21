using UnityEngine;
using Vector.Core;
using Vector.Core.Crown;
using Vector.Game.Casino;

namespace Vector.Game.Crown
{
    /// <summary>
    /// CROWN DICE シーンの起点（コンポジションルート）。ルール・乱数・エンジン・最善戦略・賭け金の管理を組み立て、
    /// 画面を作って進行役に渡す。シーンにはこのコンポーネントを1つ置くだけでよい。
    /// </summary>
    public sealed class CrownScene : MonoBehaviour
    {
        void Start()
        {
            var rules = CrownRules.Standard;
            var engine = new CrownEngine(rules, new CryptoRandom());
            var wallet = ChallengeFlow.WalletFor(GameId.Crown, rules.MinBet);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = CrownView.Build(rules);
            gameObject.AddComponent<CrownDirector>().Begin(rules, engine, new CrownAdvisor(rules), view, session, wallet);
        }
    }
}
