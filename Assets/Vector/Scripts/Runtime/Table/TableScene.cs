using UnityEngine;
using Vector.Core;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>
    /// テーブルシーンの起点（コンポジションルート）。ルール・乱数・シュー・ハウスウェイ・セッションを組み立て、
    /// 画面を作って進行役に渡す。シーンにはこのコンポーネントを1つ置くだけでよい。
    /// </summary>
    public sealed class TableScene : MonoBehaviour
    {
        void Start()
        {
            Ui.EnsureEventSystem();
            var profiles = GameContext.Profiles;
            var rules = GameContext.Rules;
            var mode = GameContext.SelectedMode;

            // 配札は暗号論的乱数。AIの先読みは速さ優先のシード付き乱数で、シードは暗号論的乱数から取る
            var random = new CryptoRandom();
            var shoe = new Shoe(rules.DeckCount, random);
            var house = new HouseWay(new SeededRandom(Seed(random)), rules.HouseWayPlayouts);
            var round = new RoundEngine(rules, shoe, house);
            var session = new TableSession(rules, new Bankroll(profiles.Profile.balance), round, mode);
            var advisor = new PlayerAdvisor(new SeededRandom(Seed(random)), rules);

            var view = TableView.Build(mode, rules);
            gameObject.AddComponent<TableDirector>().Begin(session, view, profiles, advisor);
        }

        static ulong Seed(IRandomSource random) => ((ulong)(uint)random.Next(int.MaxValue) << 32) | (uint)random.Next(int.MaxValue);
    }
}
