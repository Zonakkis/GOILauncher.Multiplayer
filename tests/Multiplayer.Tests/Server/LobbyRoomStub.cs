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
        public IEnumerable<RoomInfo> Rooms => new[] { new RoomInfo(0, "大厅", false, 0, _players.Players.Count(), null) };
        public bool TryGetMembership(int playerId, out RoomMembership membership)
        {
            PlayerInfo player;
            membership = _players.TryGetPlayer(playerId, out player) ? new RoomMembership(playerId, 0, (ulong)playerId + 1) : null;
            return membership != null;
        }
        public bool IsCurrentMembership(int playerId, ulong membershipId)
        {
            PlayerInfo player;
            return _players.TryGetPlayer(playerId, out player) && membershipId == (ulong)playerId + 1;
        }

        public IEnumerable<RoomMembership> GetMembers(int playerId)
        {
            return _players.Players.Select(player => new RoomMembership(player.Id, 0, (ulong)player.Id + 1));
        }
        public bool TryGetScope(int recipientId, int playerId, out RoomPacketScope scope)
        {
            scope = new RoomPacketScope((ulong)recipientId + 1, (ulong)playerId + 1);
            PlayerInfo recipient;
            PlayerInfo player;
            return _players.TryGetPlayer(recipientId, out recipient) && _players.TryGetPlayer(playerId, out player);
        }
        public IEnumerable<RoomPeer> Peers(int subjectPlayerId)
        {
            PlayerInfo subject;
            if (!_players.TryGetPlayer(subjectPlayerId, out subject)) yield break;
            var subjectMembership = (ulong)subjectPlayerId + 1;
            foreach (var knownPlayer in _players.Players)
            {
                var id = knownPlayer.Id;
                if (id != subjectPlayerId)
                    yield return new RoomPeer(id, new RoomPacketScope((ulong)id + 1, subjectMembership));
            }
        }
    }
}
