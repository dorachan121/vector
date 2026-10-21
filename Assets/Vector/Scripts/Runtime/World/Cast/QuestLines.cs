namespace Vector.Game.World
{
    /// <summary>クエストの台詞（依頼・引き受けた・途中・達成）。Progress の {0} は残りの回数（レベルのクエストは今のレベル）。</summary>
    public sealed class QuestLines
    {
        public string Offer;
        public string Accepted;
        public string Progress;
        public string Done;
    }
}
