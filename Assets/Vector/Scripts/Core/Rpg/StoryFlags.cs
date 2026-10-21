namespace Vector.Core.Rpg
{
    /// <summary>物語の進行の印の名前。</summary>
    public static class StoryFlags
    {
        /// <summary>オープニング（招待状）を見た。</summary>
        public const string Opening = "story.opening";

        /// <summary>エンディングを見た。</summary>
        public const string Ending = "story.ending";

        public static string Defeated(string master) => "defeated." + master;

        public static string Met(string person) => "met." + person;
    }
}
