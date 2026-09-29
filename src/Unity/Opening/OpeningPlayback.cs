using System;
using System.Collections;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Utils;

namespace GOILauncher.Multiplayer.Unity.Opening
{
    /// <summary>The narrow Unity boundary used by the deferred playback lifecycle.</summary>
    internal interface IOpeningAnimation
    {
        void Prepare();
        bool Apply(float remainingSeconds);
    }

    /// <summary>
    /// Defers seeking until PoseControl.Start has run. Returning/reusing an instance invalidates
    /// its pending request even when the coroutine has not reached its first application.
    /// </summary>
    internal sealed class OpeningPlayback
    {
        private readonly IOpeningAnimation _animation;
        private readonly OpeningClock _clock;
        private uint _generation;

        internal OpeningPlayback(IOpeningAnimation animation, OpeningClock clock)
        {
            _animation = animation;
            _clock = clock;
        }

        internal void Prepare() { _animation.Prepare(); }

        internal IEnumerator ApplyAfterOneFrame(float remainingSeconds, Action<bool> applied = null)
        {
            Cancel();
            var state = new OpeningState { RemainingSeconds = remainingSeconds };
            if (!state.IsValid) state = default(OpeningState);
            return ApplyDeferred(new OpeningCountdown(state, _clock()), _generation, applied);
        }

        private IEnumerator ApplyDeferred(OpeningCountdown countdown, uint generation, Action<bool> applied)
        {
            yield return null;
            if (generation != _generation)
                yield break;

            var success = _animation.Apply(countdown.GetState(_clock()).RemainingSeconds);
            if (applied != null) applied(success);
        }

        internal bool Reset()
        {
            Cancel();
            return _animation.Apply(0f);
        }

        internal void Cancel() { _generation = unchecked(_generation + 1); }
    }
}
