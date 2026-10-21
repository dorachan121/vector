using UnityEngine;
using Vector.Core;
using Vector.Core.Harpoon;
using Vector.Game.Casino;

namespace Vector.Game.Harpoon
{
    /// <summary>
    /// HARPOON シーンの起点（コンポジションルート）。ルール・乱数・エンジン・賭け金の管理を組み立て、画面を作って進行役に渡す。
    /// </summary>
    public sealed class HarpoonScene : MonoBehaviour
    {
        void Start()
        {
            var rules = HarpoonRules.Standard;
            var engine = new HarpoonEngine(rules, new CryptoRandom());
            var wallet = ChallengeFlow.WalletFor(GameId.Harpoon, rules.BetLevels[0]);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = HarpoonView.Build(rules);
            gameObject.AddComponent<HarpoonDirector>().Begin(rules, engine, view, session, wallet);
        }
    }
}
