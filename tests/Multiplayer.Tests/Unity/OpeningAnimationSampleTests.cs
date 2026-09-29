using FluentAssertions;
using GOILauncher.Multiplayer.Unity.Opening;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Unity
{
    [TestFixture]
    public class OpeningAnimationSampleTests
    {
        [Test]
        public void TransitionIntoWakeUp_SelectsDestinationHashAndProgress()
        {
            OpeningAnimationSample result;
            OpeningAnimationSample.TrySelect(true,
                Sample(false, 11, 0.9f), Sample(true, 81, 0.3f), out result).Should().BeTrue();

            result.StateHash.Should().Be(81);
            result.NormalizedTime.Should().Be(0.3f);
            result.Length.Should().Be(13.2167f);
        }

        [Test]
        public void OutsideTransition_IgnoresStaleNextState()
        {
            OpeningAnimationSample result;
            OpeningAnimationSample.TrySelect(false,
                Sample(true, 42, 0.6f), Sample(true, 81, 0.3f), out result).Should().BeTrue();

            result.StateHash.Should().Be(42);
            result.NormalizedTime.Should().Be(0.6f);
        }

        [Test]
        public void TransitionAwayFromWakeUp_StillUsesItsCurrentState()
        {
            OpeningAnimationSample result;
            OpeningAnimationSample.TrySelect(true,
                Sample(true, 42, 0.9f), Sample(false, 81, 0f), out result).Should().BeTrue();

            result.StateHash.Should().Be(42);
            result.NormalizedTime.Should().Be(0.9f);
        }

        [Test]
        public void TransitionToSameStateHash_UsesDestinationProgressRatherThanOldProgress()
        {
            OpeningAnimationSample result;
            OpeningAnimationSample.TrySelect(true,
                Sample(true, 42, 0.9f), Sample(true, 42, 0.1f), out result).Should().BeTrue();

            result.NormalizedTime.Should().Be(0.1f);
        }

        [Test]
        public void UnrelatedAnimations_DoNotBecomeAnOpening()
        {
            OpeningAnimationSample result;
            OpeningAnimationSample.TrySelect(false,
                Sample(false, 42, 0.5f), Sample(true, 81, 0f), out result).Should().BeFalse();
        }

        private static OpeningAnimationSample Sample(bool opening, int hash, float progress)
        {
            return new OpeningAnimationSample
            {
                IsWakeUp = opening, StateHash = hash, Length = 13.2167f, NormalizedTime = progress
            };
        }
    }
}
