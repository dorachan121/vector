using UnityEngine;
using Vector.Core;

namespace Vector.Game
{
    /// <summary>カジノにあるゲーム1つの紹介（名前・ジャンル・遊び方・条件・テーブルのシーン・色）。</summary>
    public sealed class GameEntry
    {
        public GameId Id;
        public string Title;

        /// <summary>ジャンルを一言で（英字）。</summary>
        public string Genre;

        /// <summary>遊び方の要約（日本語）。</summary>
        public string Pitch;

        /// <summary>テーブルの条件（リミットと主な配当）。</summary>
        public string Terms;

        public string Scene;
        public Color Accent;
    }
}
