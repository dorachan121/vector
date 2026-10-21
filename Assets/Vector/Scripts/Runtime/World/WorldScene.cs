using UnityEngine;

namespace Vector.Game.World
{
    /// <summary>
    /// ストーリーモード（フロアを歩くシーン）の起点（コンポジションルート）。画面を作り、進み具合と所持金を進行役に渡す。
    /// シーンにはこのコンポーネントを1つ置くだけでよい。
    /// </summary>
    public sealed class WorldScene : MonoBehaviour
    {
        void Start()
        {
            gameObject.AddComponent<WorldDirector>().Begin(GameContext.Story, GameContext.Profiles, WorldScreen.Build());
        }
    }
}
