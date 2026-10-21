namespace Vector.Core.Colosseum
{
    /// <summary>プレイヤーの1手。</summary>
    public enum BattleAction : byte
    {
        /// <summary>引いたカードの点数だけ獣を斬る（K は会心で2倍）。倒しきれなければ獣の一撃を受ける。</summary>
        Strike,

        /// <summary>引いたカードで獣の一撃を受け止める。上回った分は反撃、下回った分だけ受ける。</summary>
        Guard,
    }
}
