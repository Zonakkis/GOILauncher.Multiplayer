using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Client.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly INetworkClient _networkClient;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PlayerService> _logger;

        public ClientPlayer LocalPlayer { get; } = new ClientPlayer();
        public Dictionary<int, ClientPlayer> Players { get; }
            = new Dictionary<int, ClientPlayer>();

        public PlayerService(INetworkClient networkClient,
            IPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<PlayerService> logger)
        {
            _networkClient = networkClient;
            _eventBus = eventBus;
            _logger = logger;
            dispatcher.RegisterStruct<S2CPlayerJoinedPacket>(OnPlayerJoined);
            dispatcher.RegisterStruct<S2CPlayerLeftPacket>(OnPlayerLeft);
            // Set local player when handshake is successful
            eventBus.Subscribe<ServerHandshakeEvent>(OnServerHandshake);
        }

        public void UpdateLocalPlayerMetadata(PlayerMetadata metadata)
        {
            LocalPlayer.Name = metadata.Name;
            LocalPlayer.Platform = metadata.Platform;
        }

        private void OnServerHandshake(ServerHandshakeEvent e)
        {
            LocalPlayer.Id = e.PlayerId;
            Players[e.PlayerId] = LocalPlayer;
            var packet = new C2SClientHandShakePacket
            { PlayerName = LocalPlayer.Name, Platform = LocalPlayer.Platform };
            _networkClient.Send(packet, DeliveryMethod.ReliableOrdered);
            _logger.Info("Connected to server with PlayerId: {PlayerId}", e.PlayerId);
        }

        private void OnPlayerJoined(S2CPlayerJoinedPacket packet, NetPeer _)
        {
            var playerId = packet.PlayerId;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            Players[playerId] = new ClientPlayer
            { Id = playerId, Name = playerName, Platform = platform };
            _eventBus.Publish(
                new PlayerJoinedEvent(playerId, playerName, platform));
            _logger.Info("[{}][{}]{} joined.", playerName, playerId, platform);
        }

        private void OnPlayerLeft(S2CPlayerLeftPacket packet, NetPeer _)
        {
            var playerId = packet.PlayerId;
            if (Players.TryGetValue(playerId, out var player))
            {
                Players.Remove(playerId);
                _eventBus.Publish(new PlayerLeftEvent(playerId, player.Name, player.Platform));
                _logger.Info($"{player.Format()} left.");
            }
            else
            {
                _logger.Warn("Received PlayerLeftPacket for unknown playerId: {PlayerId}", playerId);
            }

        }

    }
}
