using NUnit.Framework;
using Vector.Core;
using Vector.Game.Persistence;

namespace Vector.Tests
{
    public class PersistenceTests
    {
        static string Encode(long balance, long pendingStake = 0)
        {
            var profile = new PlayerProfile { balance = balance, pendingStake = pendingStake };
            return ProfileCodec.Encode(profile);
        }

        [Test]
        public void Codec_RoundTripsProfile()
        {
            var profile = new PlayerProfile { balance = 12_345 };
            profile.stats.roundsPlayed = 7;
            profile.settings.showStrategy = false;
            Assert.IsTrue(ProfileCodec.TryDecode(ProfileCodec.Encode(profile), out var decoded));
            Assert.AreEqual(12_345, decoded.balance);
            Assert.AreEqual(7, decoded.stats.roundsPlayed);
            Assert.IsFalse(decoded.settings.showStrategy);
        }

        [Test]
        public void Codec_RejectsTamperedPayload()
        {
            string text = Encode(100);
            string tampered = text.Replace("\\\"balance\\\":100", "\\\"balance\\\":999999");
            Assert.AreNotEqual(text, tampered, "テストの前提: 残高の書き換えが成功していること");
            Assert.IsFalse(ProfileCodec.TryDecode(tampered, out _));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not json")]
        [TestCase("{\"format\":1}")]
        public void Codec_RejectsGarbage(string text)
        {
            Assert.IsFalse(ProfileCodec.TryDecode(text, out _));
        }

        [Test]
        public void Sanitize_ClampsOutOfRangeValues()
        {
            var profile = new PlayerProfile { balance = -8080, pendingStake = -1 };
            profile.settings.sfxVolume = 5f;
            profile.stats.roundsWon = -3;
            profile.Sanitize();
            Assert.AreEqual(0, profile.balance);
            Assert.AreEqual(0, profile.pendingStake);
            Assert.AreEqual(1f, profile.settings.sfxVolume);
            Assert.AreEqual(0, profile.stats.roundsWon);
        }

        [Test]
        public void Load_CreatesNewProfileWhenNothingSaved()
        {
            var storage = new MemoryProfileStorage();
            var service = new ProfileService(storage);
            service.Load();
            Assert.AreEqual(ProfileLoadResult.New, service.LoadResult);
            Assert.AreEqual(PlayerProfile.StartingBalance, service.Profile.balance);
            Assert.AreEqual(1, storage.Writes);
        }

        [Test]
        public void Load_RefundsStakeOfInterruptedRound()
        {
            var storage = new MemoryProfileStorage { Primary = Encode(900, pendingStake: 100) };
            var service = new ProfileService(storage);
            service.Load();
            Assert.AreEqual(100, service.RefundedStake);
            Assert.AreEqual(1000, service.Profile.balance);
            Assert.AreEqual(0, service.Profile.pendingStake);

            var reloaded = new ProfileService(storage);
            reloaded.Load();
            Assert.AreEqual(0, reloaded.RefundedStake, "返金は一度だけ");
            Assert.AreEqual(1000, reloaded.Profile.balance);
        }

        [Test]
        public void Load_RestoresFromBackupWhenPrimaryIsCorrupt()
        {
            var storage = new MemoryProfileStorage { Primary = "broken", Backup = Encode(4321) };
            var service = new ProfileService(storage);
            service.Load();
            Assert.AreEqual(ProfileLoadResult.RestoredFromBackup, service.LoadResult);
            Assert.AreEqual(4321, service.Profile.balance);
        }

        [Test]
        public void Load_ResetsWhenEverythingIsCorrupt()
        {
            var storage = new MemoryProfileStorage { Primary = "broken", Backup = "also broken" };
            var service = new ProfileService(storage);
            service.Load();
            Assert.AreEqual(ProfileLoadResult.ResetAfterCorruption, service.LoadResult);
            Assert.AreEqual(PlayerProfile.StartingBalance, service.Profile.balance);
        }

        [Test]
        public void RecordDealAndSettlement_TrackPendingStakeAndStats()
        {
            var storage = new MemoryProfileStorage();
            var service = new ProfileService(storage);
            service.Load();
            service.RecordDeal(9_900, 100);
            Assert.AreEqual(100, service.Profile.pendingStake);

            var result = new RoundResult(LineFamily.Row, LineFamily.Column, 30, 20, new Line[0], new Line[0], PokerHand.Nothing,
                new[] { new SpotResult((int)BetSpot.Main, 100, BetOutcome.Win, 200) });
            service.RecordSettlement(10_100, result, GameId.Vector);
            Assert.AreEqual(0, service.Profile.pendingStake);
            Assert.AreEqual(10_100, service.Profile.balance);
            Assert.AreEqual(1, service.Profile.stats.roundsWon);
            Assert.AreEqual(100, service.Profile.stats.biggestWin);
            var vector = service.Profile.stats.For(GameId.Vector);
            Assert.AreEqual(1, vector.rounds);
            Assert.AreEqual(1, vector.wins);
            Assert.AreEqual(100, vector.wagered);
            Assert.AreEqual(0, service.Profile.stats.For(GameId.Crown).rounds);

            Assert.IsTrue(ProfileCodec.TryDecode(storage.Primary, out var saved));
            Assert.AreEqual(10_100, saved.balance);
            Assert.AreEqual(1, saved.stats.For(GameId.Vector).rounds, "ゲームごとの成績も保存される");
        }

        [Test]
        public void Reload_OnlyWhenBelowTableMinimum()
        {
            var service = new ProfileService(new MemoryProfileStorage { Primary = Encode(500) });
            service.Load();
            Assert.AreEqual(0, service.Reload(10));

            var broke = new ProfileService(new MemoryProfileStorage { Primary = Encode(3) });
            broke.Load();
            Assert.AreEqual(PlayerProfile.StartingBalance - 3, broke.Reload(10));
            Assert.AreEqual(PlayerProfile.StartingBalance, broke.Profile.balance);
            Assert.AreEqual(1, broke.Profile.stats.reloads);
        }
    }
}
