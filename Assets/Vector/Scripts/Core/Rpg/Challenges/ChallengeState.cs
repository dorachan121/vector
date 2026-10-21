namespace Vector.Core.Rpg
{
    public enum ChallengeState : byte
    {
        InProgress,

        /// <summary>目標額に届いた。</summary>
        Won,

        /// <summary>ラウンドを使い切った、またはチップが最低額を下回った。</summary>
        Lost,

        /// <summary>途中で席を立った（負け扱い）。</summary>
        Abandoned,
    }
}
