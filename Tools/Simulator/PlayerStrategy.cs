namespace Vector.Simulator
{
    enum PlayerStrategy
    {
        /// <summary>ゲーム内の戦略ヒントと同じ強さ（観戦モードのAIプレイヤーもこれ）。</summary>
        Advisor,

        /// <summary>1手先だけを見るカジュアルな打ち方。</summary>
        Greedy,

        /// <summary>ラインも配置も無作為。</summary>
        Random,
    }
}
