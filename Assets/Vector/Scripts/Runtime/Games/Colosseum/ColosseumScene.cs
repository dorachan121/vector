using UnityEngine;
using Vector.Core;
using Vector.Core.Colosseum;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Colosseum
{
    /// <summary>
    /// COLOSSEUM シーンの起点（コンポジションルート）。ルール・乱数・エンジン・最善戦略・賭け金の管理を組み立て、
    /// 画面を作って進行役に渡す。シーンにはこのコンポーネントを1つ置くだけでよい。
    /// </summary>
    public sealed class ColosseumScene : MonoBehaviour
    {
        void Start()
        {
            Ui.EnsureEventSystem();
            var profiles = GameContext.Profiles;
            var rules = BattleRules.Standard;
            var engine = new BattleEngine(rules, new CryptoRandom());
            // 状態は体力と予告だけなので、最善戦略の表は一瞬で解ける
            var solver = BattleSolver.Solve(rules.Setup);
            var wallet = new ProfileWallet(profiles, GameId.Colosseum);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = ColosseumView.Build(rules);
            gameObject.AddComponent<ColosseumDirector>().Begin(rules, engine, solver, view, session, wallet);
        }
    }
}
