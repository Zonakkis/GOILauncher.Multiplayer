using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Extensions;
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
    public class PlayerService : IPlayerService, IStartable
    {
        private readonly INetworkClient _networkClient;
        private readonly IClientPacketDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PlayerService> _logger;
        public PlayerInfo LocalPlayer { get; private set; } = new PlayerInfo(0, null, Platform.Unknown, false);
        public Dictionary<int, PlayerInfo> Players { get; } = new Dictionary<int, PlayerInfo>();

        public PlayerService(INetworkClient networkClient,
            IClientPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<PlayerService> logger)
        {
            _networkClient = networkClient;
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _logger = logger;
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterClass<S2CPlayerListPacket>(OnPlayerList);
            _dispatcher.RegisterStruct<S2CPlayerJoinedPacket>(OnPlayerJoined);
            _dispatcher.RegisterStruct<S2CPlayerLeftPacket>(OnPlayerLeft);
            _dispatcher.RegisterStruct<S2CIsInGameUpdatePacket>(OnIsInGameUpdate);
            // Set local player when handshake is successful
            _eventBus.Subscribe<ServerHandshakeEvent>(OnServerHandshake);
            _eventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnected);
        }

        public void SetLocalPlayerInfo(PlayerInfo info)
        {
            LocalPlayer = info;
        }

        public void SetIsInGame(bool isInGame)
        {
            LocalPlayer = LocalPlayer.WithIsInGame(isInGame);
            // LocalPlayer 是不可变快照，替换属性后必须同步更新字典，否则 GetPlayers()/UI 读到的仍是旧状态
            Players[LocalPlayer.Id] = LocalPlayer;
            _networkClient.Send(new C2SIsInGameUpdatePacket(isInGame), DeliveryMethod.ReliableOrdered);
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
            _logger.Info("Local player IsInGame set to {IsInGame}", isInGame);
        }

        private IList<PlayerInfo> GetPlayers()
        {
            return Players.Values.ToList();
        }

        private void OnServerHandshake(ServerHandshakeEvent e)
        {
            LocalPlayer = new PlayerInfo(e.PlayerId, LocalPlayer.Name, LocalPlayer.Platform, LocalPlayer.IsInGame);
            Players[e.PlayerId] = LocalPlayer;
            var packet = new C2SClientHandShakePacket
            { PlayerName = LocalPlayer.Name, Platform = LocalPlayer.Platform, IsInGame = LocalPlayer.IsInGame };
            _networkClient.Send(packet, DeliveryMethod.ReliableOrdered);
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
            _logger.Info("Connected to server with PlayerId: {PlayerId}", e.PlayerId);
        }

        private void OnServerDisconnected(ServerDisconnectedEvent e)
        {
            Players.Clear();
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
        }

        private void OnPlayerList(S2CPlayerListPacket packet, PacketSender _)
        {
            Players.Clear();
            Players.Add(LocalPlayer.Id, LocalPlayer);
            foreach (var player in packet.Players)
            {
                Players.Add(player.Id, player);
            }
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
        }

        private void OnPlayerJoined(S2CPlayerJoinedPacket packet, PacketSender _)
        {
            var playerId = packet.PlayerId;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            var isInGame = packet.IsInGame;
            Players[playerId] = new PlayerInfo(playerId, playerName, platform, isInGame);
            _eventBus.Publish(
                new PlayerJoinedEvent(playerId, playerName, platform, isInGame));
            _logger.Info("[{}][{}]{} joined.", playerName, playerId, platform);
            _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
        }

        private void OnPlayerLeft(S2CPlayerLeftPacket packet, PacketSender _)
        {
            var playerId = packet.PlayerId;
            if (Players.TryGetValue(playerId, out var player))
            {
                Players.Remove(playerId);
                _eventBus.Publish(new PlayerLeftEvent(playerId, player.Name, player.Platform));
                _eventBus.Publish(new PlayerListUpdatedEvent(Players.Values.ToList()));
                _logger.Info($"{player.Format()} left.");
            }
            else
            {
                _logger.Warn("Received PlayerLeftPacket for unknown playerId: {PlayerId}", playerId);
            }
        }

        private void OnIsInGameUpdate(S2CIsInGameUpdatePacket packet, PacketSender _)
        {
            var playerId = packet.PlayerId;
            if (Players.TryGetValue(playerId, out var player))
            {
                var updated = player.WithIsInGame(packet.IsInGame);
                Players[playerId] = updated;

                if (player.IsInGame != updated.IsInGame)
                {
                    if (updated.IsInGame)
                    {
                        _eventBus.Publish(new PlayerEnteredGameEvent(updated));
                        _logger.Info("Player {PlayerName} ({PlayerId}) entered the game.", updated.Name, updated.Id);
                    }
                    else
                    {
                        _eventBus.Publish(new PlayerQuitGameEvent(updated));
                        _logger.Info("Player {PlayerName} ({PlayerId}) quit the game.", updated.Name, updated.Id);
                    }
                }

                _eventBus.Publish(new PlayerListUpdatedEvent(GetPlayers()));
            }
            else
            {
                _logger.Warn("Received IsInGameUpdatePacket for unknown playerId: {PlayerId}", playerId);
            }
        }
    }
}
