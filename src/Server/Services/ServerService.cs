using System;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ServerService : IServerService
    {
        public bool IsRunning => _networkServer.IsRunning;
        private readonly INetworkServer _networkServer;
        private readonly IEventBus _eventBus;
        private readonly IPlayerService _playerService;

        public ServerService(INetworkServer networkServer,
            IPacketDispatcher dispatcher,
            IEventBus eventBus,
            IPlayerService playerService)
        {
            _networkServer = networkServer;
            _eventBus = eventBus;
            _playerService = playerService;
            dispatcher.RegisterStruct<C2SClientHandShakePacket>(OnClientHandshake);
            dispatcher.RegisterStruct<C2SChatMessagePacket>(OnChatMessage);
            eventBus.Subscribe<ClientConnectedEvent>(OnClientConnected);
            eventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
        }

        public void Dispose()
        {
            _networkServer.Dispose();
        }

        public void Start(int port)
        {
            _networkServer.Start(port);
        }

        public void Stop()
        {
            _networkServer.Stop();
            _playerService.Clear();
        }

        public void Poll()
        {
            _networkServer.Poll();
        }

        public void Broadcast(INetSerializable packet, Func<ServerPlayer, bool> predicate = null)
        {
            foreach (var player in _playerService.Players.Values)
            {
                if (predicate != null && !predicate(player)) continue;
                _networkServer.Send(player.Id, packet, DeliveryMethod.ReliableUnordered);
            }
        }

        private void OnClientConnected(ClientConnectedEvent e)
        {
            var packet = new S2CServerHandShakePacket { PlayerId = e.ClientId };
            _networkServer.Send(e.ClientId, packet, DeliveryMethod.ReliableUnordered);
        }

        private void OnClientDisconnected(ClientDisconnectedEvent e)
        {
            if (_playerService.TryRemove(e.ClientId, out var player))
            {
                var playerLeftPacket = new S2CPlayerLeftPacket { PlayerId = player.Id };
                Broadcast(playerLeftPacket);
            }
        }

        private void OnClientHandshake(C2SClientHandShakePacket packet, NetPeer peer)
        {
            var playerId = peer.Id;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            var player = new ServerPlayer
            {
                Peer = peer,
                Name = playerName,
                Platform = platform
            };
            _playerService.AddOrUpdate(player);
            var playerJoinedPacket = new S2CPlayerJoinedPacket
            { PlayerId = playerId, PlayerName = playerName, Platform = platform };
            // Notify existing players about the new player
            Broadcast(playerJoinedPacket, p => p.Id != playerId);
            _eventBus.Publish(
                new ClientHandshakeEvent(playerName, platform));
        }

        private void OnChatMessage(C2SChatMessagePacket packet, NetPeer peer)
        {
            var playerId = peer.Id;
            var content = packet.Content;
            var timestamp = packet.Timestamp;
            var chatPacket = new S2CChatMessagePacket
            {
                PlayerId = playerId,
                Content = content,
                Timestamp = timestamp
            };
            Broadcast(chatPacket);
            _eventBus.Publish(new ChatMessageEvent(playerId, content, timestamp));
        }

    }
}
