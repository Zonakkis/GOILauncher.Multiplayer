using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerStateReceivedEvent
    {
        public int PlayerId { get; private set; }
        public PlayerState State { get; private set; }

        public PlayerStateReceivedEvent(int playerId, PlayerState state)
        {
            PlayerId = playerId;
            State = state;
        }
    }
}
