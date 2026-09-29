using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Log;

namespace GOILauncher.Multiplayer.Unity.Opening
{
    /// <summary>Reads the game's actual animation outcome, never a FastStart setting or input timer.</summary>
    public class LocalOpeningReader
    {
        private readonly IGameManager _gameManager;
        private readonly ILogger<LocalOpeningReader> _logger;

        public LocalOpeningReader(IGameManager gameManager, ILogger<LocalOpeningReader> logger)
        {
            _gameManager = gameManager;
            _logger = logger;
        }

        public bool TryRead(out OpeningState state)
        {
            state = default(OpeningState);
            if (!_gameManager.IsInGame || _gameManager.Player == null)
                return false;

            // The code only establishes a child component, not a fixed Player/dude path.
            var pose = _gameManager.Player.GetComponentInChildren<PoseControl>();
            if (pose == null || pose.anim == null)
            {
                _logger.Warn("Cannot read opening: the local Player has no active PoseControl Animator.");
                return false;
            }

            var layer = pose.anim.GetLayerIndex(OpeningAnimation.LayerName);
            if (layer < 0)
            {
                _logger.Warn("Cannot read opening: the local Animator has no Animation layer.");
                return false;
            }

            OpeningAnimationSample animation;
            if (pose.blendAmt <= 0f || !OpeningAnimation.TryGetWakeUpState(pose.anim, layer, out animation))
                return true; // Loading a save or an already completed opening is a valid zero.

            state.RemainingSeconds = OpeningAnimationTiming.GetRemainingSeconds(
                animation.Length, animation.NormalizedTime);
            return true;
        }
    }
}
