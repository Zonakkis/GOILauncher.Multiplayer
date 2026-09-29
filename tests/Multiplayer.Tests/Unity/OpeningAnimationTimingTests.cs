using FluentAssertions;
using GOILauncher.Multiplayer.Unity.Opening;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Unity
{
    [TestFixture]
    public class OpeningAnimationTimingTests
    {
        // Independent fixtures from the two in-game measurements, not helper-generated values.
        [TestCase(13.2167f, 0.2781f, 4.770568f)]
        [TestCase(13.2167f, 0.5896f, 2.712067f)]
        [TestCase(20f, 0.5f, 5f)]
        [TestCase(13.2167f, 1f, 0f)]
        [TestCase(13.2167f, 1.5f, 0f)]
        public void Capture_ConvertsClipProgressToGameSeconds(float length, float progress, float expected)
        {
            OpeningAnimationTiming.GetRemainingSeconds(length, progress)
                .Should().BeApproximately(expected, 0.00001f);
        }

        [TestCase(13.2167f, 2.712067f, 0.5896f)]
        [TestCase(20f, 5f, 0.5f)]
        [TestCase(20f, 0f, 1f)]
        [TestCase(20f, 100f, 0f)]
        public void Playback_SeeksTheTailWithoutChangingAnimatorSpeed(float length, float remaining, float expected)
        {
            OpeningAnimationTiming.GetNormalizedTime(length, remaining)
                .Should().BeApproximately(expected, 0.00001f);
        }

        [TestCase(0f, 0.5f)]
        [TestCase(float.NaN, 0.5f)]
        [TestCase(float.PositiveInfinity, 0.5f)]
        [TestCase(13.2167f, float.NaN)]
        [TestCase(13.2167f, float.PositiveInfinity)]
        public void UnusableCapture_IsCompletedRatherThanInvalidNetworkData(float length, float progress)
        {
            OpeningAnimationTiming.GetRemainingSeconds(length, progress).Should().Be(0f);
        }

        [TestCase(0f, 2f)]
        [TestCase(float.NaN, 2f)]
        [TestCase(13.2167f, float.NaN)]
        [TestCase(13.2167f, -1f)]
        [TestCase(13.2167f, float.PositiveInfinity)]
        public void UnusablePlayback_IsCompleted(float length, float remaining)
        {
            OpeningAnimationTiming.GetNormalizedTime(length, remaining).Should().Be(1f);
        }

        [Test]
        public void SecondaryAnimation_UsesElapsedGameSecondsNotDoubledBodyClipSeconds()
        {
            // Body: 12 clip seconds / 2 = 6 game seconds. Four remaining means two elapsed.
            var elapsed = OpeningAnimationTiming.GetElapsedSeconds(12f, 4f);

            elapsed.Should().Be(2f);
            // A separate four-second, 1x Rattle clip is halfway through, not at its end.
            OpeningAnimationTiming.GetSecondaryNormalizedTime(4f, 1f, elapsed).Should().Be(0.5f);
        }

        [Test]
        public void SecondaryAnimation_DoesNotLoopAfterItsOwnEnd()
        {
            OpeningAnimationTiming.GetSecondaryNormalizedTime(2f, 1f, 5f).Should().Be(1f);
        }
    }
}
