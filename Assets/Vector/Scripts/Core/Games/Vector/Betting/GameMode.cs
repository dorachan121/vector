namespace Vector.Core
{
    public enum GameMode : byte
    {
        /// <summary>プレイヤー席に座り、自分でカードを置いてディーラーと勝負する。</summary>
        Player,

        /// <summary>観戦席（バックベット）。AIプレイヤーとディーラーの勝負に賭ける。</summary>
        Audience,
    }
}
