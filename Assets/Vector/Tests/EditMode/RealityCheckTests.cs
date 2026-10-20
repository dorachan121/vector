using System;
using NUnit.Framework;
using Vector.Core;

namespace Vector.Tests
{
    public class RealityCheckTests
    {
        static readonly DateTime Start = new DateTime(2026, 1, 1, 12, 0, 0);

        [Test]
        public void BecomesDueAfterIntervalAndRearmsOnAcknowledge()
        {
            var check = new RealityCheck(TimeSpan.FromMinutes(30), 10_000, Start);
            Assert.IsFalse(check.IsDue(Start.AddMinutes(29)));
            Assert.IsTrue(check.IsDue(Start.AddMinutes(30)));

            check.Acknowledge(Start.AddMinutes(35));
            Assert.IsFalse(check.IsDue(Start.AddMinutes(64)));
            Assert.IsTrue(check.IsDue(Start.AddMinutes(65)));
            Assert.AreEqual(TimeSpan.FromMinutes(65), check.Elapsed(Start.AddMinutes(65)));
        }

        [Test]
        public void NetExcludesCashierDeposits()
        {
            var check = new RealityCheck(TimeSpan.FromMinutes(30), 10_000, Start);
            Assert.AreEqual(-9_995, check.Net(5));
            check.RecordDeposit(9_995);
            Assert.AreEqual(-9_995, check.Net(10_000), "補充したチップは勝ちに数えない");
            Assert.AreEqual(-8_995, check.Net(11_000));
        }

        [Test]
        public void RejectsInvalidArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RealityCheck(TimeSpan.Zero, 0, Start));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RealityCheck(TimeSpan.FromMinutes(1), 0, Start).RecordDeposit(-1));
        }
    }
}
