namespace Vector.Core
{
    /// <summary>カジノにあるゲームの種類。成績・クエスト・フロアの解放条件などで使う。</summary>
    public enum GameId : byte
    {
        /// <summary>3×3 の盤面でラインの合計を競うカードゲーム。</summary>
        Vector,

        /// <summary>王冠の目が最強になるダイスの対決。</summary>
        Crown,

        /// <summary>獣の「次の一撃」を見て攻めるか守るかを選ぶ闘技場。</summary>
        Colosseum,

        /// <summary>カードで走る四神のレースに賭ける。</summary>
        Derby,

        /// <summary>同じ元素が融合して位が上がっていくスロット。</summary>
        Alchemia,

        /// <summary>扉を選んで迷宮を潜り、好きな深さで引き返す。</summary>
        Delve,

        /// <summary>3つの惑星が止まる位置と並び方に賭ける、天球儀のルーレット。</summary>
        Orbit,

        /// <summary>泳ぐ獲物を選んでモリを撃つ、深海の狩り。</summary>
        Harpoon,

        /// <summary>2つのサイコロのどちらで進むかを選び、小判を集めて上がりを目指す双六。</summary>
        Sugoroku,

        /// <summary>昇る花火が弾ける前に開く、引き際の打ち上げ花火。</summary>
        Hanabi,
    }
}
