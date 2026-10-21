namespace Vector.Core.Sugoroku
{
    /// <summary>双六のマスの種類（止まったときだけ効果がある。通り過ぎるだけなら何も起きない）。</summary>
    public enum Square : byte
    {
        /// <summary>何もないマス（振り出しもこれ）。</summary>
        Plain,

        /// <summary>小判。倍率の段が 1 上がる。</summary>
        Coin,

        /// <summary>宝。倍率の段が 2 上がる。</summary>
        Treasure,

        /// <summary>凶。倍率の段が 1 下がる。</summary>
        Omen,

        /// <summary>橋。さらに先へ進む（渡った先のマスの効果は無い）。</summary>
        Bridge,

        /// <summary>落とし穴。少し戻される（戻った先のマスの効果は無い）。</summary>
        Pit,
    }
}
