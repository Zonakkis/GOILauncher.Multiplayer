using System;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Core.Utils
{
    /// <summary>
    /// Ages a snapshot on one process's monotonic clock. No timestamp crosses the network;
    /// forwarding creates a fresh countdown on the receiver's clock.
    /// </summary>
    public struct OpeningCountdown
    {
        private readonly float _remainingSeconds;
        private readonly double _capturedAtSeconds;

        public OpeningCountdown(OpeningState state, double capturedAtSeconds)
        {
            if (!state.IsValid)
                throw new ArgumentOutOfRangeException(nameof(state));

            _remainingSeconds = state.RemainingSeconds;
            _capturedAtSeconds = capturedAtSeconds;
        }

        public OpeningState GetState(double nowSeconds)
        {
            var elapsed = Math.Max(0.0, nowSeconds - _capturedAtSeconds);
            return new OpeningState
            {
                RemainingSeconds = (float)Math.Max(0.0, _remainingSeconds - elapsed)
            };
        }
    }
}
