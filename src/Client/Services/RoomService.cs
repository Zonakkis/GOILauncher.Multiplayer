using System.Collections.Generic;
using System.Linq;
using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Services
{
    public sealed class RoomService : IRoomService, IStartable
    {
        private readonly INetworkClient _network;
        private readonly IClientPacketDispatcher _dispatcher;
        private readonly IClientEventBus _events;
        private readonly IPlayerService _players;
        private readonly Dictionary<int, RoomInfo> _rooms = new Dictionary<int, RoomInfo>();
        private int? _currentRoomId;
        private RoomOperationResult? _pending;
        private uint _nextRequestId = 1;
        public IEnumerable<RoomInfo> Rooms => _rooms.Values.OrderBy(r => r.Id);
        public RoomInfo CurrentRoom
        {
            get { RoomInfo room; return _currentRoomId.HasValue && _rooms.TryGetValue(_currentRoomId.Value, out room) ? room : null; }
        }
        public bool IsOperationPending => _pending.HasValue;
        public RoomService(INetworkClient network, IClientPacketDispatcher dispatcher, IClientEventBus events, IPlayerService players)
        { _network = network; _dispatcher = dispatcher; _events = events; _players = players; }
        void IStartable.Start()
        {
            _dispatcher.RegisterClass<S2CRoomListPacket>(OnDirectory);
            _dispatcher.RegisterClass<S2CPlayerListPacket>(OnRoomEntered);
            _dispatcher.RegisterStruct<S2CRoomOperationResultPacket>(OnResult);
            _events.Subscribe<ServerDisconnectedEvent>(OnDisconnected);
        }
        public void RefreshRooms() { Submit(new C2SRoomOperationPacket { Operation = RoomOperation.Refresh }); }
        public void CreateRoom(string name, string password, int maxPlayers)
        {
            Submit(new C2SRoomOperationPacket { Operation = RoomOperation.Create, Name = name, Password = password,
                MaxPlayers = maxPlayers, PasswordChange = string.IsNullOrEmpty(password) ? RoomPasswordChange.Remove : RoomPasswordChange.Set });
        }
        public void JoinRoom(int roomId, string password)
        { Submit(new C2SRoomOperationPacket { Operation = RoomOperation.Join, RoomId = roomId, Password = password }); }
        public void LeaveRoom() { Submit(new C2SRoomOperationPacket { Operation = RoomOperation.Leave }); }
        public void UpdateRoom(int roomId, string name, int maxPlayers, RoomPasswordChange passwordChange, string password)
        {
            Submit(new C2SRoomOperationPacket { Operation = RoomOperation.Update, RoomId = roomId, Name = name,
                MaxPlayers = maxPlayers, PasswordChange = passwordChange, Password = password });
        }
        private void Submit(C2SRoomOperationPacket packet)
        {
            packet.RequestId = _nextRequestId++;
            packet.MembershipId = _players.LocalMembershipId;
            var error = IsOperationPending ? RoomError.Busy : (!_network.IsConnected || CurrentRoom == null || packet.MembershipId == 0)
                ? RoomError.NotReady : RoomError.None;
            if (error != RoomError.None)
            {
                _events.Publish(new RoomOperationCompletedEvent(new RoomOperationResult
                { RequestId = packet.RequestId, Operation = packet.Operation, Error = error }));
                return;
            }
            _pending = new RoomOperationResult { RequestId = packet.RequestId, Operation = packet.Operation };
            _network.Send(packet, DeliveryMethod.ReliableOrdered);
        }
        private void OnDirectory(S2CRoomListPacket packet, PacketSender _)
        {
            if (!_network.IsConnected) return;
            // Room-entry snapshots and directory updates share the same ordered control channel.
            _rooms.Clear();
            foreach (var room in packet.Rooms) _rooms[room.Id] = room;
            _events.Publish(new RoomListUpdatedEvent());
            _events.Publish(new CurrentRoomChangedEvent());
        }
        private void OnRoomEntered(S2CPlayerListPacket packet, PacketSender _)
        {
            if (!_network.IsConnected || packet.Room == null || packet.Members.Count == 0) return;
            var ids = new HashSet<int>();
            ulong localMembership = 0;
            foreach (var member in packet.Members)
            {
                if (member.Player == null || member.MembershipId == 0 || !ids.Add(member.Player.Id)) return;
                if (member.Player.Id == _players.LocalPlayer.Id) localMembership = member.MembershipId;
            }
            if (localMembership == 0 || localMembership <= _players.LocalMembershipId) return;
            _rooms[packet.Room.Id] = packet.Room; _currentRoomId = packet.Room.Id;
            _players.ReplaceRoomRoster(packet.Members);
            // Three explicit phases, not subscriber-order-dependent cleanup vs construction.
            _events.Publish(new RoomMembershipChangedEvent());
            _events.Publish(new PlayerRosterReceivedEvent());
            _events.Publish(new PlayerListUpdatedEvent());
            _events.Publish(new RoomListUpdatedEvent());
            _events.Publish(new CurrentRoomChangedEvent());
        }
        private void OnResult(S2CRoomOperationResultPacket packet, PacketSender _)
        {
            if (!_pending.HasValue || packet.Result.RequestId != _pending.Value.RequestId
                || packet.Result.Operation != _pending.Value.Operation) return;
            _pending = null;
            _events.Publish(new RoomOperationCompletedEvent(packet.Result));
        }
        private void OnDisconnected(ServerDisconnectedEvent e)
        {
            _rooms.Clear(); _currentRoomId = null; _pending = null;
            _events.Publish(new RoomListUpdatedEvent());
            _events.Publish(new CurrentRoomChangedEvent());
        }
    }
}
