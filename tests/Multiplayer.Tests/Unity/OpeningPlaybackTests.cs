using System;
using System.Collections.Generic;
using FluentAssertions;
using GOILauncher.Multiplayer.Unity.Opening;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Unity
{
    [TestFixture]
    public class OpeningPlaybackTests
    {
        private FakeAnimation _animation;
        private OpeningPlayback _playback;
        private double _now;

        [SetUp]
        public void Setup()
        {
            _now = 100.0;
            _animation = new FakeAnimation();
            _playback = new OpeningPlayback(_animation, () => _now);
            _playback.Prepare();
        }

        [Test]
        public void Preparation_DoesNotSeekBeforeTheGameHasInitializedThePose()
        {
            _animation.Prepared.Should().BeTrue();
            _animation.Applied.Should().BeEmpty();
        }

        [Test]
        public void Apply_WaitsOneFrameAndSubtractsThatLocalWait()
        {
            var routine = _playback.ApplyAfterOneFrame(5f);
            _animation.Applied.Should().BeEmpty();
            routine.MoveNext().Should().BeTrue();
            routine.Current.Should().BeNull();
            _animation.Applied.Should().BeEmpty();

            _now = 101.25;
            routine.MoveNext().Should().BeFalse();

            _animation.Applied.Should().Equal(3.75f);
        }

        [Test]
        public void Snapshot_ExpiresWhileWaitingForItsFirstApply()
        {
            var routine = _playback.ApplyAfterOneFrame(0.5f);
            routine.MoveNext();
            _now = 101.0;

            routine.MoveNext().Should().BeFalse();

            _animation.Applied.Should().Equal(0f);
        }

        [Test]
        public void ReturnBeforeFirstApply_ClearsPoseAndInvalidatesTheDeferredRequest()
        {
            var routine = _playback.ApplyAfterOneFrame(5f);
            routine.MoveNext();

            _playback.Reset();
            _now = 101.0;
            routine.MoveNext().Should().BeFalse();

            _animation.Applied.Should().Equal(0f);
        }

        [Test]
        public void Reset_WithoutAnyPreviousRequest_StillAppliesCompletedState()
        {
            _playback.Reset();

            _animation.Applied.Should().Equal(0f);
        }

        [Test]
        public void ReusedInstance_CannotApplyItsPreviousOccupantsRequest()
        {
            var old = _playback.ApplyAfterOneFrame(5f);
            old.MoveNext();
            _playback.Reset();
            _animation.Applied.Clear();
            _now = 101.0;
            var current = _playback.ApplyAfterOneFrame(2f);
            current.MoveNext();
            _now = 101.5;

            old.MoveNext().Should().BeFalse();
            current.MoveNext().Should().BeFalse();

            _animation.Applied.Should().Equal(1.5f);
        }

        [Test]
        public void NewAnnouncement_SupersedesAnUnknownCompletedRequest()
        {
            var old = _playback.ApplyAfterOneFrame(0f);
            old.MoveNext();
            var current = _playback.ApplyAfterOneFrame(4f);
            current.MoveNext();

            old.MoveNext().Should().BeFalse();
            current.MoveNext().Should().BeFalse();

            _animation.Applied.Should().Equal(4f);
        }

        [Test]
        public void Disable_CancelsPendingWorkWithoutEvaluatingAnInactiveAnimator()
        {
            var routine = _playback.ApplyAfterOneFrame(4f);
            routine.MoveNext();

            _playback.Cancel();
            routine.MoveNext().Should().BeFalse();

            _animation.Applied.Should().BeEmpty();
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidInput_AppliesCompletedState(float remaining)
        {
            var routine = _playback.ApplyAfterOneFrame(remaining);
            routine.MoveNext();
            routine.MoveNext();

            _animation.Applied.Should().Equal(0f);
        }

        [Test]
        public void FailedApply_IsReportedButCancelledRequestHasNoCallback()
        {
            var results = new List<bool>();
            var old = _playback.ApplyAfterOneFrame(4f, results.Add);
            old.MoveNext();
            var current = _playback.ApplyAfterOneFrame(2f, results.Add);
            current.MoveNext();
            _animation.Succeeds = false;

            old.MoveNext();
            current.MoveNext();

            results.Should().Equal(false);
        }

        private sealed class FakeAnimation : IOpeningAnimation
        {
            public bool Prepared;
            public bool Succeeds = true;
            public readonly List<float> Applied = new List<float>();

            public void Prepare() { Prepared = true; }

            public bool Apply(float remainingSeconds)
            {
                if (!Prepared) throw new InvalidOperationException("The animation must be prepared before use.");
                Applied.Add(remainingSeconds);
                return Succeeds;
            }
        }
    }
}
