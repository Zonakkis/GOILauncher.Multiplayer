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

    /// <summary>A recipient in a room fan-out, paired with the scope to stamp on the packet sent to it.</summary>
    public struct RoomPeer
    {
        public int PlayerId { get; private set; }
        public RoomPacketScope Scope { get; private set; }
        public RoomPeer(int playerId, RoomPacketScope scope) { PlayerId = playerId; Scope = scope; }
    }

    public interface IRoomService
    {
        IEnumerable<RoomInfo> Rooms { get; }
        bool TryGetMembership(int playerId, out RoomMembership membership);
        bool IsCurrentMembership(int playerId, ulong membershipId);
        /// <summary>Read-only live view on the Poll thread; do not mutate rooms while enumerating.</summary>
        IEnumerable<RoomMembership> GetMembers(int playerId);
        bool TryGetScope(int recipientId, int playerId, out RoomPacketScope scope);
        /// <summary>
        /// The other current members of the subject's room, each paired with the scope to stamp on a
        /// packet addressed to it (recipient's own membership + the subject's membership). Skips the
        /// subject; empty if it has no current membership. Enumerate on the Poll thread without mutating rooms.
        /// </summary>
        IEnumerable<RoomPeer> Peers(int subjectPlayerId);
    }
}
