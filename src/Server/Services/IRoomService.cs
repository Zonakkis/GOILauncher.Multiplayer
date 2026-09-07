using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Server.Services
{
    public sealed class RoomMembership
    {
        public int PlayerId { get; private set; }
        public int RoomId { get; private set; }
        public ulong Id { get; private set; }
        public RoomMembership(int playerId, int roomId, ulong id)
        { PlayerId = playerId; RoomId = roomId; Id = id; }
    }

    public interface IRoomService
    {
        IEnumerable<RoomInfo> Rooms { get; }
        bool TryGetMembership(int playerId, out RoomMembership membership);
        bool IsCurrentMembership(int playerId, ulong membershipId);
        /// <summary>Read-only live view on the Poll thread; do not mutate rooms while enumerating.</summary>
        IEnumerable<RoomMembership> GetMembers(int playerId);
        bool TryGetScope(int recipientId, int playerId, out RoomPacketScope scope);
    }
}
