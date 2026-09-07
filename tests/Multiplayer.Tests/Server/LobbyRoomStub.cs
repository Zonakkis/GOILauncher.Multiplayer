using System.Collections.Generic;
using System.Linq;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Server.Services;

namespace GOILauncher.Multiplayer.Tests.Server
{
    /// <summary>One-room fixture for the existing isolated relay tests. Room rules have real-service tests.</summary>
    internal sealed class LobbyRoomStub : IRoomService
    {
        private readonly IPlayerService _players;
        public LobbyRoomStub(IPlayerService players) { _players = players; }
        public IEnumerable<RoomInfo> Rooms => new[] { new RoomInfo(0, "大厅", false, 0, _players.Players.Count, null) };
        public bool TryGetMembership(int playerId, out RoomMembership membership)
        {
            membership = _players.Players.ContainsKey(playerId) ? new RoomMembership(playerId, 0, (ulong)playerId + 1) : null;
            return membership != null;
        }
        public bool IsCurrentMembership(int playerId, ulong membershipId) => _players.Players.ContainsKey(playerId) && membershipId == (ulong)playerId + 1;
        public IEnumerable<RoomMembership> GetMembers(int playerId) => _players.Players.Keys.Select(id => new RoomMembership(id, 0, (ulong)id + 1));
        public bool TryGetScope(int recipientId, int playerId, out RoomPacketScope scope)
        {
            scope = new RoomPacketScope((ulong)recipientId + 1, (ulong)playerId + 1);
            return _players.Players.ContainsKey(recipientId) && _players.Players.ContainsKey(playerId);
        }
    }
}
