using System;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ServerService : IServerService
    {
        public bool IsRunning => _networkServer.IsRunning;
        private readonly INetworkServer _networkServer;
        private readonly IPlayerService _playerService;

        public ServerService(INetworkServer networkServer,
            IEventBus eventBus)
        {
            _networkServer = networkServer;
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
    }
}
