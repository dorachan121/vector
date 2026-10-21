namespace Vector.Game.Audio
{
    public enum SfxKind
    {
        Button,
        Chip,
        ChipsCollect,
        CardDeal,
        CardFlip,
        Tick,
        Win,
        BigWin,
        Lose,
        Push,
        Shuffle,
        Error,

        /// <summary>ダイスがテーブルを転がる音。</summary>
        DiceRoll,

        /// <summary>扉がきしんで開く音。</summary>
        DoorOpen,

        /// <summary>獣・魔物のうなり声。</summary>
        Growl,

        /// <summary>シンボルが融合するきらめき。</summary>
        Sparkle,

        /// <summary>モリを撃ち出す「ドシュッ」。</summary>
        Harpoon,

        /// <summary>水しぶきと泡。</summary>
        Splash,

        /// <summary>花火の打ち上げ（昇っていく笛の音）。</summary>
        Launch,

        /// <summary>花火が開く「ドーン」とパチパチ。</summary>
        Boom,
    }
}
