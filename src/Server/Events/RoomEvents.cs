using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Server.Events
{
    public sealed class PlayerStatusChangedEvent
    {
        public PlayerInfo Player { get; private set; }
        public PlayerStatusChangedEvent(PlayerInfo player) { Player = player; }
    }

    /// <summary>Published only after membership and ordered roster notifications are committed.</summary>
    public sealed class PlayerRoomEnteredEvent
    {
        public int PlayerId { get; private set; }
        public PlayerRoomEnteredEvent(int playerId) { PlayerId = playerId; }
    }
}
