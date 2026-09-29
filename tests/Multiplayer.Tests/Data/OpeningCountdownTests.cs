using System;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Utils;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Data
{
    [TestFixture]
    public class OpeningCountdownTests
    {
        [TestCase(100.0, 5f)]
        [TestCase(102.0, 3f)]
        [TestCase(105.0, 0f)]
        [TestCase(110.0, 0f)]
        public void Snapshot_SubtractsOnlyTimeSpentInThisCache(double now, float remaining)
        {
            var countdown = new OpeningCountdown(new OpeningState { RemainingSeconds = 5f }, 100.0);

            countdown.GetState(now).RemainingSeconds.Should().Be(remaining);
        }

        [Test]
        public void RepeatedReads_DoNotRestartOrDoubleSubtractTheCountdown()
        {
            var countdown = new OpeningCountdown(new OpeningState { RemainingSeconds = 5f }, 100.0);

            countdown.GetState(102.0).RemainingSeconds.Should().Be(3f);
            countdown.GetState(102.0).RemainingSeconds.Should().Be(3f);
            countdown.GetState(103.0).RemainingSeconds.Should().Be(2f);
        }

        [Test]
        public void CompletedState_RemainsCompleted()
        {
            var countdown = new OpeningCountdown(default(OpeningState), 100.0);

            countdown.GetState(100.0).RemainingSeconds.Should().Be(0f);
            countdown.GetState(200.0).RemainingSeconds.Should().Be(0f);
        }

        [Test]
        public void ReceivingAForwardedState_StartsOnTheReceiversOwnClock()
        {
            var sender = new OpeningCountdown(new OpeningState { RemainingSeconds = 5f }, 100.0);
            var receiver = new OpeningCountdown(sender.GetState(102.0), 9000.0);

            receiver.GetState(9000.0).RemainingSeconds.Should().Be(3f);
            receiver.GetState(9001.0).RemainingSeconds.Should().Be(2f);
        }

        [Test]
        public void InvalidState_CannotBecomeACachedCountdown()
        {
            Action create = () => new OpeningCountdown(
                new OpeningState { RemainingSeconds = float.NaN }, 100.0);

            create.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
