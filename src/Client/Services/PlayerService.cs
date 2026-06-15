using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using System.Collections.Generic;
using System.Linq;

namespace GOILauncher.Multiplayer.Client.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly INetworkClient _networkClient;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PlayerService> _logger;
        public ClientPlayer LocalPlayer { get; } = new ClientPlayer();
        public Dictionary<int, ClientPlayer> Players { get; } = new Dictionary<int, ClientPlayer>();

        public PlayerService(INetworkClient networkClient,
            IPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<PlayerService> logger)
        {
            _networkClient = networkClient;
            _eventBus = eventBus;
            _logger = logger;
            dispatcher.RegisterClass<S2CPlayerListPacket>(OnPlayerList);
            dispatcher.RegisterStruct<S2CPlayerJoinedPacket>(OnPlayerJoined);
            dispatcher.RegisterStruct<S2CPlayerLeftPacket>(OnPlayerLeft);
            dispatcher.RegisterStruct<S2CIsInGameUpdatePacket>(OnIsInGameUpdate);
            // Set local player when handshake is successful
            eventBus.Subscribe<ServerHandshakeEvent>(OnServerHandshake);
            eventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnected);
        }

        public void SetLocalPlayerInfo(PlayerInfo info)
        {
            LocalPlayer.Info = info;
        }

        public void SetIsInGame(bool isInGame)
        {
            LocalPlayer.Info.IsInGame = isInGame;
            _networkClient.Send(new C2SIsInGameUpdatePacket(isInGame), DeliveryMethod.ReliableOrdered);
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
        }

        private IList<IClientPlayer> GetPlayers()
        {
            return Players.Values.OfType<IClientPlayer>().ToList();
        }

        private void OnServerHandshake(ServerHandshakeEvent e)
        {
            LocalPlayer.Info.Id = e.PlayerId;
            Players[e.PlayerId] = LocalPlayer;
            var packet = new C2SClientHandShakePacket
            { PlayerName = LocalPlayer.Info.Name, Platform = LocalPlayer.Info.Platform, IsInGame = LocalPlayer.Info.IsInGame };
            _networkClient.Send(packet, DeliveryMethod.ReliableOrdered);
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
            _logger.Info("Connected to server with PlayerId: {PlayerId}", e.PlayerId);
        }

        private void OnServerDisconnected(ServerDisconnectedEvent e)
        {
            Players.Clear();
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
        }

        private void OnPlayerList(S2CPlayerListPacket packet, NetPeer _)
        {
            Players.Clear();
            Players.Add(LocalPlayer.Info.Id, LocalPlayer);
            foreach (var player in packet.Players)
            {
                Players.Add(player.Id, new ClientPlayer
                {
                    Info = new PlayerInfo
                    {
                        Id = player.Id,
                        Name = player.Name,
                        Platform = player.Platform,
                        IsInGame = player.IsInGame
                    }
                });
            }
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
        }

        private void OnPlayerJoined(S2CPlayerJoinedPacket packet, NetPeer _)
        {
            var playerId = packet.PlayerId;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            var isInGame = packet.IsInGame;
            Players[playerId] = new ClientPlayer
            {
                Info = new PlayerInfo
                {
                    Id = playerId,
                    Name = playerName,
                    Platform = platform,
                    IsInGame = isInGame
                }
            };
            _eventBus.Publish(
                new PlayerJoinedEvent(playerId, playerName, platform, isInGame));
            _logger.Info("[{}][{}]{} joined.", playerName, playerId, platform);
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
        }

        private void OnPlayerLeft(S2CPlayerLeftPacket packet, NetPeer _)
        {
            var playerId = packet.PlayerId;
            if (Players.TryGetValue(playerId, out var player))
            {
                Players.Remove(playerId);
                _eventBus.Publish(new PlayerLeftEvent(playerId, player.Info.Name, player.Info.Platform));
                _eventBus.Publish(new PlayerListUpdatedEvent(Players.Values.OfType<IClientPlayer>().ToList()));
                _logger.Info($"{player.Format()} left.");
            }
            else
            {
                _logger.Warn("Received PlayerLeftPacket for unknown playerId: {PlayerId}", playerId);
            }
        }

        private void OnIsInGameUpdate(S2CIsInGameUpdatePacket packet, NetPeer _)
        {
            var playerId = packet.PlayerId;
            if (Players.TryGetValue(playerId, out var player))
            {
                player.Info.IsInGame = packet.IsInGame;
                _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
            }
            else
            {
                _logger.Warn("Received IsInGameUpdatePacket for unknown playerId: {PlayerId}", playerId);
            }
        }
    }
}
