using System.Collections.Generic;
using System.Linq;
using Autofac;
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
    public class PlayerService : IPlayerService, IStartable
    {
        private readonly INetworkServer _networkServer;
        private readonly IServerPacketDispatcher _packetDispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PlayerService> _logger;
        public Dictionary<int, PlayerInfo> Players { get; }
            = new Dictionary<int, PlayerInfo>();

        public PlayerService(INetworkServer networkServer,
            IServerPacketDispatcher packetDispatcher,
            IEventBus eventBus,
            ILogger<PlayerService> logger)
        {
            _networkServer = networkServer;
            _packetDispatcher = packetDispatcher;
            _eventBus = eventBus;
            _logger = logger;
        }

        void IStartable.Start()
        {
            _packetDispatcher.RegisterStruct<C2SClientHandShakePacket>(OnClientHandshake);
            _packetDispatcher.RegisterStruct<C2SIsInGameUpdatePacket>(OnIsInGameUpdate);
            _eventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
        }

        private void OnClientHandshake(C2SClientHandShakePacket packet, PacketSender sender)
        {
            var playerId = sender.Id;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            var player = new PlayerInfo(playerId, playerName, platform, packet.IsInGame);
            Players[player.Id] = player;
            // Notify existing players about the new player
            var playerJoinedPacket = new S2CPlayerJoinedPacket
            { PlayerId = playerId, PlayerName = playerName, Platform = platform, IsInGame = packet.IsInGame };
            var existingPlayerIds = Players.Keys.Where(id => id != playerId);
            _networkServer.Multicast(existingPlayerIds, playerJoinedPacket, DeliveryMethod.ReliableOrdered);
            // Notify the new player about the existing players
            var playerListPacket = new S2CPlayerListPacket
            {
                Players = Players.Values.Where(p => p.Id != playerId).ToList()
            };
            _networkServer.Send(playerId, playerListPacket, DeliveryMethod.ReliableOrdered);
            _eventBus.Publish(new ClientHandshakeEvent(playerId, playerName, platform));
        }

        private void OnClientDisconnected(ClientDisconnectedEvent e)
        {
            var playerId = e.ClientId;
            if (Players.TryGetValue(playerId, out var player))
            {
                Players.Remove(playerId);
                var otherPlayerIds = Players.Keys.ToList();
                var playerLeftPacket = new S2CPlayerLeftPacket { PlayerId = player.Id };
                _networkServer.Multicast(otherPlayerIds, playerLeftPacket, DeliveryMethod.ReliableOrdered);
            }
        }

        private void OnIsInGameUpdate(C2SIsInGameUpdatePacket packet, PacketSender sender)
        {
            var playerId = sender.Id;
            if (Players.TryGetValue(playerId, out var player))
            {
                Players[playerId] = player.WithIsInGame(packet.IsInGame);
                var isInGameUpdatePacket = new S2CIsInGameUpdatePacket
                {
                    PlayerId = playerId,
                    IsInGame = packet.IsInGame
                };
                var otherPlayerIds = Players.Keys.Where(id => id != playerId);
                _networkServer.Multicast(otherPlayerIds, isInGameUpdatePacket, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
