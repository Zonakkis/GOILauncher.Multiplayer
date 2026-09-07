using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public struct RoomListUpdatedEvent { }
    public struct CurrentRoomChangedEvent { }
    /// <summary>New roster is already installed; clean old-room resources before roster-received is published.</summary>
    public struct RoomMembershipChangedEvent { }
    public struct ChatHistoryResetEvent { }
    public sealed class RoomOperationCompletedEvent
    {
        public RoomOperationResult Result { get; private set; }
        public RoomOperationCompletedEvent(RoomOperationResult result) { Result = result; }
    }
}
