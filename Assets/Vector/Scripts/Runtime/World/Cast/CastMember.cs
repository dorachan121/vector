using System.Collections.Generic;

namespace Vector.Game.World
{
    /// <summary>登場人物1人の見た目と台詞（はじめて会ったとき・ふだん・クエスト・主としての挑戦）。</summary>
    public sealed class CastMember
    {
        public CharacterLook Look;
        public string[] Intro;
        public string[] Idle;
        public readonly Dictionary<string, QuestLines> Quests = new Dictionary<string, QuestLines>();

        /// <summary>フロアの主なら、その台詞（ほかの人は null）。</summary>
        public MasterLines Master;
    }
}
