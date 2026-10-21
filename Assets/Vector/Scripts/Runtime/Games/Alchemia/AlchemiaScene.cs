using UnityEngine;
using Vector.Core;
using Vector.Core.Alchemia;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Alchemia
{
    /// <summary>
    /// ALCHEMIA シーンの起点（コンポジションルート）。ルール・乱数・エンジン・賭け金の管理を組み立て、画面を作って進行役に渡す。
    /// </summary>
    public sealed class AlchemiaScene : MonoBehaviour
    {
        void Start()
        {
            Ui.EnsureEventSystem();
            var profiles = GameContext.Profiles;
            var rules = AlchemiaRules.Standard;
            var engine = new AlchemiaEngine(rules, new CryptoRandom());
            var wallet = new ProfileWallet(profiles, GameId.Alchemia);
            var session = new WagerSession(rules, new Bankroll(wallet.Balance));
            var view = AlchemiaView.Build(rules);
            gameObject.AddComponent<AlchemiaDirector>().Begin(rules, engine, view, session, wallet);
        }
    }
}
