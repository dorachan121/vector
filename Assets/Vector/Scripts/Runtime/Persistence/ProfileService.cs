using System;
using System.IO;
using Vector.Core;

namespace Vector.Game.Persistence
{
    /// <summary>
    /// プレイヤーデータの読み書きと更新。保存の失敗はゲームを止めずにログへ残す。
    /// </summary>
    public sealed class ProfileService
    {
        readonly IProfileStorage storage;

        public ProfileService(IProfileStorage storage)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            Profile = new PlayerProfile();
        }

        public PlayerProfile Profile { get; private set; }
        public ProfileLoadResult LoadResult { get; private set; }

        /// <summary>前回中断したラウンドから返金した額（0なら返金なし）。</summary>
        public long RefundedStake { get; private set; }

        public void Load()
        {
            string primary = SafeRead(storage.ReadPrimary);
            string backup = null;
            if (ProfileCodec.TryDecode(primary, out var profile))
            {
                LoadResult = ProfileLoadResult.Loaded;
            }
            else if (ProfileCodec.TryDecode(backup = SafeRead(storage.ReadBackup), out profile))
            {
                LoadResult = ProfileLoadResult.RestoredFromBackup;
                Log.Warn("セーブデータが読めなかったため、バックアップから復旧しました。");
            }
            else
            {
                profile = new PlayerProfile();
                LoadResult = primary == null && backup == null ? ProfileLoadResult.New : ProfileLoadResult.ResetAfterCorruption;
                if (LoadResult == ProfileLoadResult.ResetAfterCorruption)
                    Log.Warn("セーブデータが破損または改ざんされていたため、初期状態に戻しました。");
            }
            Profile = profile;

            RefundedStake = profile.pendingStake;
            if (RefundedStake > 0)
            {
                profile.balance = PlayerProfile.Clamp(profile.balance + RefundedStake);
                profile.pendingStake = 0;
                Log.Info($"中断したラウンドの賭け金 {RefundedStake} を返金しました。");
            }
            if (LoadResult != ProfileLoadResult.Loaded || RefundedStake > 0)
                Save();
        }

        public bool Save()
        {
            try
            {
                storage.Write(ProfileCodec.Encode(Profile));
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Log.Error($"セーブに失敗しました: {e.Message}");
                return false;
            }
        }

        /// <summary>配札した（賭け金を引き落とした）ことを記録する。</summary>
        public void RecordDeal(long balanceAfterDebit, long stake)
        {
            Profile.balance = PlayerProfile.Clamp(balanceAfterDebit);
            Profile.pendingStake = PlayerProfile.Clamp(stake);
            Save();
        }

        /// <summary>精算結果を記録する。</summary>
        public void RecordSettlement(long balance, IRoundOutcome result, GameId game)
        {
            Profile.balance = PlayerProfile.Clamp(balance);
            Profile.pendingStake = 0;
            Profile.stats.Record(result, game);
            Save();
        }

        /// <summary>障害で無効にしたラウンドの返金後の所持金を記録する。</summary>
        public void RecordVoid(long balance)
        {
            Profile.balance = PlayerProfile.Clamp(balance);
            Profile.pendingStake = 0;
            Save();
        }

        /// <summary>
        /// 手持ちがミニマム未満のとき、初期額までチップを補充する（遊戯用チップ）。補充した額を返す。
        /// </summary>
        public long Reload(long tableMinimum)
        {
            if (Profile.balance >= tableMinimum)
                return 0;
            long added = PlayerProfile.StartingBalance - Profile.balance;
            Profile.balance = PlayerProfile.StartingBalance;
            Profile.stats.reloads++;
            Save();
            return added;
        }

        /// <summary>読み込み時のお知らせ（返金・復旧）を表示し終えたら呼ぶ。</summary>
        public void ClearLoadNotice()
        {
            RefundedStake = 0;
            LoadResult = ProfileLoadResult.Loaded;
        }

        public void UpdateSettings(Action<ProfileSettings> change)
        {
            change(Profile.settings);
            Profile.settings.Sanitize();
            Save();
        }

        static string SafeRead(Func<string> read)
        {
            try
            {
                return read();
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Log.Error($"セーブデータを読めませんでした: {e.Message}");
                return null;
            }
        }
    }
}
