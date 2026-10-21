using UnityEngine;
using Vector.Core;
using Vector.Core.Sugoroku;
using Vector.Game.Casino;

namespace Vector.Game.Sugoroku
{
    /// <summary>
    /// SUGOROKU シーンの起点（コンポジションルート）。ルール・乱数・エンジン・最善手の表・賭け金の管理を組み立て、画面を作って進行役に渡す。
    /// </summary>
    public sealed class SugorokuScene : MonoBehaviour
    {
        void Start()
        {
            var rules = SugorokuRules.Standard;
            var engine = new SugorokuEngine(rules, new CryptoRandom());
            var solver = SugorokuSolver.Solve(rules);
            var wallet = ChallengeFlow.WalletFor(GameId.Sugoroku, rules.MinBet);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = SugorokuView.Build(rules);
            gameObject.AddComponent<SugorokuDirector>().Begin(rules, engine, solver, view, session, wallet);
        }
    }
}
