using System.Collections.Generic;
using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Services
{
    public class PlayerService : IPlayerService, IStartable
    {
        private readonly INetworkClient _networkClient;
        private readonly IClientPacketDispatcher _dispatcher;
        private readonly IClientEventBus _eventBus;
        private readonly ILogger<PlayerService> _logger;
        private readonly Dictionary<int, PlayerInfo> _players = new Dictionary<int, PlayerInfo>();
        private readonly Dictionary<int, ulong> _memberships = new Dictionary<int, ulong>();
        public PlayerInfo LocalPlayer { get; private set; } = new PlayerInfo(0, "", Platform.PC, false);
        public IEnumerable<PlayerInfo> Players => _players.Values;
        public ulong LocalMembershipId
        {
            get { ulong id; return _memberships.TryGetValue(LocalPlayer.Id, out id) ? id : 0; }
        }
        public PlayerService(INetworkClient networkClient, IClientPacketDispatcher dispatcher,
            IClientEventBus eventBus, ILogger<PlayerService> logger)
        { _networkClient = networkClient; _dispatcher = dispatcher; _eventBus = eventBus; _logger = logger; }
        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<S2CPlayerJoinedPacket>(OnPlayerJoined);
            _dispatcher.RegisterStruct<S2CPlayerLeftPacket>(OnPlayerLeft);
            _dispatcher.RegisterStruct<S2CIsInGameUpdatePacket>(OnIsInGameUpdate);
            _eventBus.Subscribe<ServerHandshakeEvent>(OnServerHandshake);
            _eventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnected);
        }
        public bool TryGetPlayer(int playerId, out PlayerInfo player) => _players.TryGetValue(playerId, out player);
        public bool AcceptsScope(int playerId, RoomPacketScope scope)
        {
            ulong membership;
            return LocalMembershipId != 0 && scope.RecipientMembershipId == LocalMembershipId
                && _memberships.TryGetValue(playerId, out membership) && membership == scope.PlayerMembershipId;
        }
        public void ReplaceRoomRoster(IEnumerable<RoomMemberInfo> members)
        {
            _players.Clear(); _memberships.Clear();
            foreach (var member in members)
            {
                // A scene change may already be queued after the room request. Do not overwrite
                // the local scene fact with an older server snapshot of ourselves.
                _players.Add(member.Player.Id, member.Player.Id == LocalPlayer.Id ? LocalPlayer : member.Player);
                _memberships.Add(member.Player.Id, member.MembershipId);
            }
        }
        public void SetLocalPlayerInfo(PlayerInfo info) { LocalPlayer = info; }
        public void SetIsInGame(bool isInGame)
        {
            LocalPlayer = LocalPlayer.WithIsInGame(isInGame);
            _players[LocalPlayer.Id] = LocalPlayer;
            _networkClient.Send(new C2SIsInGameUpdatePacket(isInGame), DeliveryMethod.ReliableOrdered);
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }
        private void OnServerHandshake(ServerHandshakeEvent e)
        {
            _players.Clear(); _memberships.Clear();
            LocalPlayer = new PlayerInfo(e.PlayerId, LocalPlayer.Name, LocalPlayer.Platform, LocalPlayer.IsInGame);
            _players[e.PlayerId] = LocalPlayer;
            _networkClient.Send(new C2SClientHandShakePacket
            { PlayerName = LocalPlayer.Name, Platform = LocalPlayer.Platform, IsInGame = LocalPlayer.IsInGame }, DeliveryMethod.ReliableOrdered);
            _logger.Info("Local player identity ready: {PlayerId}.", LocalPlayer.Id);
            _eventBus.Publish(new LocalPlayerReadyEvent(LocalPlayer));
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }
        private void OnServerDisconnected(ServerDisconnectedEvent e)
        {
            _players.Clear(); _memberships.Clear();
            LocalPlayer = new PlayerInfo(0, LocalPlayer.Name, LocalPlayer.Platform, false);
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }
        private void OnPlayerJoined(S2CPlayerJoinedPacket packet, PacketSender _)
        {
            if (LocalMembershipId == 0 || packet.Scope.RecipientMembershipId != LocalMembershipId
                || packet.Scope.PlayerMembershipId == 0 || packet.PlayerId == LocalPlayer.Id) return;
            ulong existing;
            if (_memberships.TryGetValue(packet.PlayerId, out existing)) return;
            _players[packet.PlayerId] = new PlayerInfo(packet.PlayerId, packet.PlayerName, packet.Platform, packet.IsInGame);
            _memberships[packet.PlayerId] = packet.Scope.PlayerMembershipId;
            _logger.Info("Player {PlayerId} joined the room.", packet.PlayerId);
            _eventBus.Publish(new PlayerJoinedEvent(packet.PlayerId, packet.PlayerName, packet.Platform, packet.IsInGame));
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }
        private void OnPlayerLeft(S2CPlayerLeftPacket packet, PacketSender _)
        {
            if (!AcceptsScope(packet.PlayerId, packet.Scope) || packet.PlayerId == LocalPlayer.Id) return;
            var player = _players[packet.PlayerId];
            _players.Remove(packet.PlayerId); _memberships.Remove(packet.PlayerId);
            _logger.Info("Player {PlayerId} left the room.", player.Id);
            _eventBus.Publish(new PlayerLeftEvent(player.Id, player.Name, player.Platform));
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }
        private void OnIsInGameUpdate(S2CIsInGameUpdatePacket packet, PacketSender _)
        {
            if (!AcceptsScope(packet.PlayerId, packet.Scope) || packet.PlayerId == LocalPlayer.Id) return;
            var player = _players[packet.PlayerId];
            var updated = player.WithIsInGame(packet.IsInGame);
            _players[packet.PlayerId] = updated;
            if (player.IsInGame != updated.IsInGame)
            {
                if (updated.IsInGame) _eventBus.Publish(new PlayerEnteredGameEvent(updated));
                else _eventBus.Publish(new PlayerQuitGameEvent(updated));
            }
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }
    }
}
