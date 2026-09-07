using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    /// <summary>All membership mutations run on the server's Poll thread.</summary>
    public sealed class RoomService : IRoomService, IStartable
    {
        private sealed class Room
        {
            public int Id;
            public string Name;
            public int MaxPlayers;
            public int? Owner;
            public RoomPassword Password;
            public readonly List<RoomMembership> Members = new List<RoomMembership>();
            public readonly ReadOnlyCollection<RoomMembership> MemberView;
            public Room() { MemberView = Members.AsReadOnly(); }
            public RoomInfo Snapshot => new RoomInfo(Id, Name, Password != null, MaxPlayers, Members.Count, Owner);
        }
        private readonly Dictionary<int, Room> _rooms = new Dictionary<int, Room>();
        private readonly Dictionary<int, RoomMembership> _members = new Dictionary<int, RoomMembership>();
        private readonly IPlayerService _players;
        private readonly INetworkServer _network;
        private readonly IServerPacketDispatcher _dispatcher;
        private readonly IServerEventBus _events;
        private readonly ILogger<RoomService> _logger;
        private static readonly RoomMembership[] EmptyMembers = new RoomMembership[0];
        private int _nextRoomId;
        private ulong _nextMembershipId;

        public RoomService(IPlayerService players, INetworkServer network, IServerPacketDispatcher dispatcher,
            IServerEventBus events, ILogger<RoomService> logger)
        {
            _players = players; _network = network; _dispatcher = dispatcher; _events = events; _logger = logger;
            Reset();
        }
        public IEnumerable<RoomInfo> Rooms => _rooms.Values.OrderBy(r => r.Id).Select(r => r.Snapshot);
        public bool TryGetMembership(int playerId, out RoomMembership membership) => _members.TryGetValue(playerId, out membership);
        public bool IsCurrentMembership(int playerId, ulong membershipId)
        {
            RoomMembership member;
            return membershipId != 0 && _members.TryGetValue(playerId, out member) && member.Id == membershipId;
        }
        public IEnumerable<RoomMembership> GetMembers(int playerId)
        {
            RoomMembership member;
            return _members.TryGetValue(playerId, out member)
                ? (IEnumerable<RoomMembership>)_rooms[member.RoomId].MemberView : EmptyMembers;
        }
        public bool TryGetScope(int recipientId, int playerId, out RoomPacketScope scope)
        {
            RoomMembership recipient, player;
            if (_members.TryGetValue(recipientId, out recipient) && _members.TryGetValue(playerId, out player)
                && recipient.RoomId == player.RoomId)
            {
                scope = new RoomPacketScope(recipient.Id, player.Id);
                return true;
            }
            scope = default(RoomPacketScope);
            return false;
        }
        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<C2SRoomOperationPacket>(OnOperation);
            _events.Subscribe<ClientHandshakeEvent>(e => Move(e.PlayerId, _rooms[RoomConstants.LobbyId]));
            _events.Subscribe<ClientDisconnectedEvent>(OnDisconnected);
            _events.Subscribe<PlayerStatusChangedEvent>(OnStatusChanged);
            _events.Subscribe<ServerStoppedEvent>(e => Reset());
        }
        private void Reset()
        {
            _members.Clear(); _rooms.Clear(); _nextRoomId = 1; _nextMembershipId = 1;
            _rooms.Add(RoomConstants.LobbyId, new Room { Id = RoomConstants.LobbyId, Name = RoomConstants.LobbyName });
        }
        private void OnOperation(C2SRoomOperationPacket packet, PacketSender sender)
        {
            RoomError error;
            RoomMembership member;
            if (!_members.TryGetValue(sender.Id, out member)) error = RoomError.NotReady;
            else if (member.Id != packet.MembershipId) error = RoomError.StaleMembership;
            else error = Execute(packet, member);

            _network.Send(sender.Id, new S2CRoomOperationResultPacket
            {
                Result = new RoomOperationResult { RequestId = packet.RequestId, Operation = packet.Operation, Error = error }
            }, DeliveryMethod.ReliableOrdered);
        }
        private RoomError Execute(C2SRoomOperationPacket packet, RoomMembership member)
        {
            Room target;
            switch (packet.Operation)
            {
                case RoomOperation.Refresh:
                    SendDirectory(member.PlayerId);
                    return RoomError.None;
                case RoomOperation.Create:
                    var validation = ValidateAttributes(packet, 1);
                    if (validation != RoomError.None) return validation;
                    if (_nextRoomId <= 0) return RoomError.Forbidden;
                    target = new Room { Id = _nextRoomId++, Name = packet.Name.Trim(), MaxPlayers = packet.MaxPlayers };
                    ApplyPassword(target, packet);
                    _rooms.Add(target.Id, target);
                    Move(member.PlayerId, target);
                    return RoomError.None;
                case RoomOperation.Join:
                    if (!_rooms.TryGetValue(packet.RoomId, out target)) return RoomError.RoomNotFound;
                    if (target.Id == member.RoomId) return RoomError.None;
                    if (target.MaxPlayers > 0 && target.Members.Count >= target.MaxPlayers) return RoomError.RoomFull;
                    if (packet.Password != null && packet.Password.Length > RoomConstants.MaxPasswordLength) return RoomError.InvalidPassword;
                    if (target.Password != null && !target.Password.Matches(packet.Password)) return RoomError.IncorrectPassword;
                    Move(member.PlayerId, target);
                    return RoomError.None;
                case RoomOperation.Leave:
                    if (member.RoomId != RoomConstants.LobbyId) Move(member.PlayerId, _rooms[RoomConstants.LobbyId]);
                    return RoomError.None;
                case RoomOperation.Update:
                    if (!_rooms.TryGetValue(packet.RoomId, out target)) return RoomError.RoomNotFound;
                    if (target.Id == RoomConstants.LobbyId) return RoomError.Forbidden;
                    if (member.RoomId != target.Id || target.Owner != member.PlayerId) return RoomError.NotOwner;
                    validation = ValidateAttributes(packet, target.Members.Count);
                    if (validation != RoomError.None) return validation;
                    // Derive a new verifier before changing any public attributes.
                    ApplyPassword(target, packet);
                    target.Name = packet.Name.Trim(); target.MaxPlayers = packet.MaxPlayers;
                    _logger.Info("Room {RoomId} updated by owner {PlayerId}.", target.Id, member.PlayerId);
                    PublishDirectory();
                    return RoomError.None;
                default:
                    return RoomError.Forbidden;
            }
        }
        private static RoomError ValidateAttributes(C2SRoomOperationPacket packet, int playerCount)
        {
            if (packet.Name == null || packet.Name.Trim().Length == 0 || packet.Name.Trim().Length > RoomConstants.MaxNameLength)
                return RoomError.InvalidName;
            if (packet.MaxPlayers < 0 || (packet.MaxPlayers != 0 && packet.MaxPlayers < playerCount)) return RoomError.InvalidCapacity;
            if (packet.PasswordChange != RoomPasswordChange.Keep && packet.PasswordChange != RoomPasswordChange.Set
                && packet.PasswordChange != RoomPasswordChange.Remove) return RoomError.InvalidPassword;
            if (packet.PasswordChange == RoomPasswordChange.Set && (string.IsNullOrEmpty(packet.Password)
                || packet.Password.Length > RoomConstants.MaxPasswordLength)) return RoomError.InvalidPassword;
            return RoomError.None;
        }
        private static void ApplyPassword(Room room, C2SRoomOperationPacket packet)
        {
            if (packet.PasswordChange == RoomPasswordChange.Set) room.Password = new RoomPassword(packet.Password);
            else if (packet.PasswordChange == RoomPasswordChange.Remove) room.Password = null;
        }
        private void Move(int playerId, Room target)
        {
            if (!_players.Players.ContainsKey(playerId)) return;
            RemoveMember(playerId);
            var member = new RoomMembership(playerId, target.Id, _nextMembershipId++);
            _members.Add(playerId, member); target.Members.Add(member);
            if (target.Id != RoomConstants.LobbyId && !target.Owner.HasValue) target.Owner = playerId;

            var player = _players.Players[playerId];
            _network.Send(playerId, new S2CPlayerListPacket
            {
                Room = target.Snapshot,
                Members = target.Members.Select(m => new RoomMemberInfo(_players.Players[m.PlayerId], m.Id)).ToList()
            }, DeliveryMethod.ReliableOrdered);
            foreach (var recipient in target.Members.Where(m => m.PlayerId != playerId))
                _network.Send(recipient.PlayerId, new S2CPlayerJoinedPacket
                {
                    Scope = new RoomPacketScope(recipient.Id, member.Id), PlayerId = playerId,
                    PlayerName = player.Name, Platform = player.Platform, IsInGame = player.IsInGame
                }, DeliveryMethod.ReliableOrdered);
            PublishDirectory();
            _events.Publish(new PlayerRoomEnteredEvent(playerId));
            _logger.Info("Player {PlayerId} entered room {RoomId} (membership {MembershipId}).", playerId, target.Id, member.Id);
        }
        private bool RemoveMember(int playerId)
        {
            RoomMembership member;
            if (!_members.TryGetValue(playerId, out member)) return false;
            var room = _rooms[member.RoomId];
            room.Members.Remove(member); _members.Remove(playerId);
            if (room.Owner == playerId) room.Owner = room.Members.Count == 0 ? (int?)null : room.Members[0].PlayerId;
            if (room.Id != RoomConstants.LobbyId && room.Members.Count == 0) _rooms.Remove(room.Id);
            foreach (var recipient in room.Members)
                _network.Send(recipient.PlayerId, new S2CPlayerLeftPacket
                {
                    Scope = new RoomPacketScope(recipient.Id, member.Id), PlayerId = playerId
                }, DeliveryMethod.ReliableOrdered);
            return true;
        }
        private void OnDisconnected(ClientDisconnectedEvent e)
        { if (RemoveMember(e.ClientId)) PublishDirectory(); }
        private void OnStatusChanged(PlayerStatusChangedEvent e)
        {
            foreach (var recipient in GetMembers(e.Player.Id).Where(m => m.PlayerId != e.Player.Id))
            {
                RoomPacketScope scope;
                if (!TryGetScope(recipient.PlayerId, e.Player.Id, out scope)) continue;
                _network.Send(recipient.PlayerId, new S2CIsInGameUpdatePacket
                { Scope = scope, PlayerId = e.Player.Id, IsInGame = e.Player.IsInGame }, DeliveryMethod.ReliableOrdered);
            }
        }
        private void SendDirectory(int playerId)
        { _network.Send(playerId, new S2CRoomListPacket { Rooms = Rooms.ToList() }, DeliveryMethod.ReliableOrdered); }
        private void PublishDirectory()
        {
            var packet = new S2CRoomListPacket { Rooms = Rooms.ToList() };
            _network.Multicast(_members.Keys.ToArray(), packet, DeliveryMethod.ReliableOrdered);
        }
    }
}
