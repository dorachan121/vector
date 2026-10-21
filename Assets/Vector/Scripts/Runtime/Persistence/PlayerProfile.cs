using System;
using Vector.Core.Rpg;

namespace Vector.Game.Persistence
{
    /// <summary>
    /// 保存するプレイヤーデータ。JsonUtility で直列化するため public フィールドで持つ。
    /// 読み込んだ値は必ず <see cref="Sanitize"/> で範囲内に収める。
    /// </summary>
    [Serializable]
    public sealed class PlayerProfile
    {
        /// <summary>2: ストーリーモード（rpg）とゲームごとの成績を追加。1 のデータはそのまま読める（足りない値は初期値）。</summary>
        public const int CurrentVersion = 2;

        /// <summary>初回とリロード時に受け取るチップ。</summary>
        public const long StartingBalance = 10_000;

        /// <summary>所持金の上限（オーバーフローと改ざんの影響を抑える）。</summary>
        public const long MaxBalance = 1_000_000_000_000;

        public int version = CurrentVersion;
        public long balance = StartingBalance;

        /// <summary>配札済みで未精算の賭け金。強制終了で中断したラウンドは次回起動時に返金する。</summary>
        public long pendingStake;

        public ProfileStats stats = new ProfileStats();
        public ProfileSettings settings = new ProfileSettings();

        /// <summary>ストーリーモードの進み具合。</summary>
        public RpgState rpg = new RpgState();

        public void Sanitize()
        {
            version = CurrentVersion;
            balance = Clamp(balance);
            pendingStake = Clamp(pendingStake);
            stats ??= new ProfileStats();
            settings ??= new ProfileSettings();
            rpg ??= new RpgState();
            stats.Sanitize();
            settings.Sanitize();
            rpg.Sanitize();
        }

        internal static long Clamp(long value) => Math.Min(Math.Max(value, 0), MaxBalance);
    }
}
