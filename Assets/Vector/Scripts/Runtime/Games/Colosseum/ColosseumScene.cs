using UnityEngine;
using Vector.Core;
using Vector.Core.Colosseum;
using Vector.Game.Casino;

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
            var rules = BattleRules.Standard;
            var engine = new BattleEngine(rules, new CryptoRandom());
            var wallet = ChallengeFlow.WalletFor(GameId.Colosseum, rules.MinBet);
            // 挑戦では特別な相手と戦う（オーナーの獣など）
            var opponent = (wallet as ChallengeWallet)?.Run.Spec.Opponent;
            string title = opponent.HasValue ? "THE HOUSE" : "ARENA BEAST";
            // 状態は体力と予告だけなので、最善戦略の表は一瞬で解ける
            var solver = BattleSolver.Solve(opponent ?? rules.Setup);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = ColosseumView.Build(rules);
            gameObject.AddComponent<ColosseumDirector>().Begin(rules, engine, solver, view, session, wallet, opponent, title);
        }
    }
}
