using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>1つのフロア（見取り図・遊べるゲーム・いる人・主の挑戦・開く条件）。名前や台詞は表示側（Runtime）が持つ。</summary>
    public sealed class FloorDef
    {
        public FloorId Id;

        /// <summary>このフロアのテーブルのゲーム。</summary>
        public GameId Game;

        public TileMap Map;

        /// <summary>見取り図の人の文字 → 人の id。</summary>
        public IReadOnlyDictionary<char, string> People;

        /// <summary>フロアの主の挑戦。</summary>
        public ChallengeSpec Master;

        /// <summary>このフロアに行くために倒しておく主（null なら最初から行ける）。</summary>
        public string UnlockedBy;
    }
}
