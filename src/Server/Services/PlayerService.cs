using System.Collections.Generic;
using System.Linq;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly INetworkServer _networkServer;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PlayerService> _logger;
        public Dictionary<int, ServerPlayer> Players { get; }
            = new Dictionary<int, ServerPlayer>();

        public PlayerService(INetworkServer networkServer,
            IPacketDispatcher packetDispatcher,
            IEventBus eventBus,
            ILogger<PlayerService> logger)
        {
            _networkServer = networkServer;
            _eventBus = eventBus;
            _logger = logger;
            packetDispatcher.RegisterStruct<C2SClientHandShakePacket>(OnClientHandshake);
            eventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
        }

        private void OnClientHandshake(C2SClientHandShakePacket packet, NetPeer peer)
        {
            var playerId = peer.Id;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            var player = new ServerPlayer
            {
                Peer = peer,
                Info = new PlayerInfo
                {
                    Id = playerId,
                    Name = playerName,
                    Platform = platform
                }
            };
            Players[player.Info.Id] = player;
            // Notify existing players about the new player
            var playerJoinedPacket = new S2CPlayerJoinedPacket
            { PlayerId = playerId, PlayerName = playerName, Platform = platform };
            var existingPlayerIds = Players.Keys.Where(id => id != playerId);
            _networkServer.Multicast(existingPlayerIds, playerJoinedPacket, DeliveryMethod.ReliableUnordered);
            // Notify the new player about the existing players
            var playerListPacket = new S2CPlayerListPacket
            {
                Players = Players.Values.Select(p => (IPlayerInfo)new PlayerInfo
                {
                    Id = p.Info.Id,
                    Name = p.Info.Name,
                    Platform = p.Info.Platform,
                    IsInGame = p.Info.IsInGame
                }).Where(p => p.Id != playerId).ToList()
            };
            _networkServer.Send(playerId, playerListPacket, DeliveryMethod.ReliableUnordered);
            _eventBus.Publish(new ClientHandshakeEvent(playerName, platform));
        }

        private void OnClientDisconnected(ClientDisconnectedEvent e)
        {
            var playerId = e.ClientId;
            if (Players.TryGetValue(playerId, out var player))
            {
                Players.Remove(playerId);
                var otherPlayerIds = Players.Keys.ToList();
                var playerLeftPacket = new S2CPlayerLeftPacket { PlayerId = player.Info.Id };
                _networkServer.Multicast(otherPlayerIds, playerLeftPacket, DeliveryMethod.ReliableUnordered);
            }
        }
    }
}
