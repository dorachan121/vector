using UnityEngine;

namespace Vector.Game.Lobby
{
    /// <summary>ホーム画面のタブ1つ分の画面。表示するたびに Refresh で最新の状態にする。</summary>
    public interface IHomeTab
    {
        RectTransform Root { get; }

        void Refresh();
    }
}
