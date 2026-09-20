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

    /// <summary>
    /// A chat message that passed membership validation and was relayed. RoomId is the
    /// sender's room at relay time; Timestamp is the server's receive time, not the
    /// client-sent one — the sender's clock is not a diagnostics-grade source.
    /// </summary>
    public sealed class ChatRelayedEvent
    {
        public int RoomId { get; }
        public int PlayerId { get; }
        public string PlayerName { get; }
        public string Content { get; }
        public long Timestamp { get; }

        public ChatRelayedEvent(int roomId, int playerId, string playerName, string content, long timestamp)
        {
            RoomId = roomId; PlayerId = playerId; PlayerName = playerName;
            Content = content; Timestamp = timestamp;
        }
    }
}
