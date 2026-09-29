using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    /// <summary>
    /// A remote Opening snapshot was accepted and cached. State is the received snapshot;
    /// use ClientOpeningSync.TryGetOpening for its locally aged value when applying it later.
    /// </summary>
    public sealed class PlayerOpeningReceivedEvent
    {
        public int PlayerId { get; private set; }
        public OpeningState State { get; private set; }

        public PlayerOpeningReceivedEvent(int playerId, OpeningState state)
        {
            PlayerId = playerId;
            State = state;
        }
    }
}
