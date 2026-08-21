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

namespace GOILauncher.Multiplayer.Client.Services
{
    public class PlayerService : IPlayerService, IStartable
    {
        private readonly INetworkClient _networkClient;
        private readonly IClientPacketDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PlayerService> _logger;

        private readonly Dictionary<int, PlayerInfo> _players = new Dictionary<int, PlayerInfo>();

        public PlayerInfo LocalPlayer { get; private set; } = new PlayerInfo(0, null, Platform.Unknown, false);

        public IEnumerable<PlayerInfo> Players
        {
            get { return _players.Values; }
        }

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

        public bool TryGetPlayer(int playerId, out PlayerInfo player)
        {
            return _players.TryGetValue(playerId, out player);
        }

        public void SetLocalPlayerInfo(PlayerInfo info)
        {
            LocalPlayer = info;
        }

        public void SetIsInGame(bool isInGame)
        {
            LocalPlayer = LocalPlayer.WithIsInGame(isInGame);
            // LocalPlayer 是不可变快照，替换属性后必须同步更新字典，否则名单里读到的仍是旧状态
            _players[LocalPlayer.Id] = LocalPlayer;
            _networkClient.Send(new C2SIsInGameUpdatePacket(isInGame), DeliveryMethod.ReliableOrdered);
            _eventBus.Publish(new PlayerListUpdatedEvent());
            _logger.Info("Local player IsInGame set to {IsInGame}", isInGame);
        }

        private void OnServerHandshake(ServerHandshakeEvent e)
        {
            // 每条连接都从空名单开始：上一条连接遗留的条目在这里被丢弃。
            _players.Clear();
            LocalPlayer = new PlayerInfo(e.PlayerId, LocalPlayer.Name, LocalPlayer.Platform, LocalPlayer.IsInGame);
            _players[e.PlayerId] = LocalPlayer;
            var packet = new C2SClientHandShakePacket
            { PlayerName = LocalPlayer.Name, Platform = LocalPlayer.Platform, IsInGame = LocalPlayer.IsInGame };
            _networkClient.Send(packet, DeliveryMethod.ReliableOrdered);
            // 自身状态写完之后再发布，订阅者读到的一定是新身份。
            _eventBus.Publish(new LocalPlayerReadyEvent(LocalPlayer));
            _eventBus.Publish(new PlayerListUpdatedEvent());
            _logger.Info("Connected to server with PlayerId: {PlayerId}", e.PlayerId);
        }

        private void OnServerDisconnected(ServerDisconnectedEvent e)
        {
            _players.Clear();
            // 服务端分配的 Id 随连接失效，只保留下次连接会复用的名字和平台。
            LocalPlayer = new PlayerInfo(0, LocalPlayer.Name, LocalPlayer.Platform, false);
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }

        private void OnPlayerList(S2CPlayerListPacket packet, PacketSender _)
        {
            _players.Clear();
            _players[LocalPlayer.Id] = LocalPlayer;
            foreach (var player in packet.Players)
            {
                _players[player.Id] = player;
            }
            _eventBus.Publish(new PlayerRosterReceivedEvent());
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }

        private void OnPlayerJoined(S2CPlayerJoinedPacket packet, PacketSender _)
        {
            var playerId = packet.PlayerId;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            var isInGame = packet.IsInGame;
            _players[playerId] = new PlayerInfo(playerId, playerName, platform, isInGame);
            _eventBus.Publish(
                new PlayerJoinedEvent(playerId, playerName, platform, isInGame));
            _logger.Info("[{}][{}]{} joined.", playerName, playerId, platform);
            _eventBus.Publish(new PlayerListUpdatedEvent());
        }

        private void OnPlayerLeft(S2CPlayerLeftPacket packet, PacketSender _)
        {
            var playerId = packet.PlayerId;
            if (_players.TryGetValue(playerId, out var player))
            {
                _players.Remove(playerId);
                _eventBus.Publish(new PlayerLeftEvent(playerId, player.Name, player.Platform));
                _eventBus.Publish(new PlayerListUpdatedEvent());
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
            if (_players.TryGetValue(playerId, out var player))
            {
                var updated = player.WithIsInGame(packet.IsInGame);
                _players[playerId] = updated;

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

                _eventBus.Publish(new PlayerListUpdatedEvent());
            }
            else
            {
                _logger.Warn("Received IsInGameUpdatePacket for unknown playerId: {PlayerId}", playerId);
            }
        }
    }
}
