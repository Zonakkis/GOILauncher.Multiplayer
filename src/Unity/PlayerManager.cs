using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Models;

namespace GOILauncher.Multiplayer.Unity
{
    public class PlayerManager : IPlayerManager, IDisposable
    {
        private const int DefaultAvatarWarmUpCount = 4;

        private readonly IEventBus _eventBus;
        private readonly IGameManager _gameManager;
        private readonly IPlayerAvatarPool _avatarPool;
        private readonly ILogger<PlayerManager> _logger;

        private readonly Dictionary<int, PlayerBase> _players = new Dictionary<int, PlayerBase>();
        private readonly Dictionary<int, IClientPlayer> _knownPlayers = new Dictionary<int, IClientPlayer>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        private int _localPlayerId;
        private LocalPlayer _localPlayer;

        public PlayerManager(IEventBus eventBus,
            IGameManager gameManager,
            IPlayerAvatarPool avatarPool,
            ILogger<PlayerManager> logger)
        {
            _eventBus = eventBus;
            _gameManager = gameManager;
            _avatarPool = avatarPool;
            _logger = logger;
        }

        public PlayerBase GetPlayer(int playerId)
        {
            PlayerBase player;
            return _players.TryGetValue(playerId, out player) ? player : null;
        }

        public IEnumerable<PlayerBase> Players
        {
            get { return _players.Values; }
        }

        public void Init()
        {
            _subscriptions.Add(_eventBus.Subscribe<ServerHandshakeEvent>(OnServerHandshakeEvent));
            _subscriptions.Add(_eventBus.Subscribe<GameStartedEvent>(OnGameStartedEvent));
            _subscriptions.Add(_eventBus.Subscribe<GameRestartedEvent>(OnGameRestartedEvent));
            _subscriptions.Add(_eventBus.Subscribe<GameQuitEvent>(OnGameQuitEvent));
            _subscriptions.Add(_eventBus.Subscribe<PlayerListUpdatedEvent>(OnPlayerListUpdatedEvent));
            _subscriptions.Add(_eventBus.Subscribe<PlayerLeftEvent>(OnPlayerLeftEvent));
            _subscriptions.Add(_eventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnectedEvent));
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        private void OnServerHandshakeEvent(ServerHandshakeEvent e)
        {
            _localPlayerId = e.PlayerId;
            _logger.Info("Handshake completed, local player id: {PlayerId}", e.PlayerId);
        }

        private void OnGameStartedEvent(GameStartedEvent e)
        {
            EnsureLocalPlayer();
            _avatarPool.WarmUp(DefaultAvatarWarmUpCount);
            SyncRemotePlayers();
        }

        private void OnGameRestartedEvent(GameRestartedEvent e)
        {
            RemoveAllRemotePlayers();
            EnsureLocalPlayer();
            _avatarPool.WarmUp(DefaultAvatarWarmUpCount);
            SyncRemotePlayers();
        }

        private void OnGameQuitEvent(GameQuitEvent e)
        {
            RemoveAllRemotePlayers();
            ReleaseLocalPlayer();
            _avatarPool.Clear();
        }

        private void OnPlayerListUpdatedEvent(PlayerListUpdatedEvent e)
        {
            _knownPlayers.Clear();
            foreach (var player in e.Players)
            {
                if (player.Info != null)
                {
                    _knownPlayers[player.Info.Id] = player;
                }
            }

            SyncRemotePlayers();
        }

        private void OnPlayerLeftEvent(PlayerLeftEvent e)
        {
            _knownPlayers.Remove(e.PlayerId);

            PlayerBase player;
            if (!_players.TryGetValue(e.PlayerId, out player))
            {
                return;
            }

            _players.Remove(e.PlayerId);
            var remote = player as RemotePlayer;
            if (remote != null)
            {
                _avatarPool.Return(remote);
            }
            _logger.Info("Player {PlayerName} ({PlayerId}) removed.", e.PlayerName, e.PlayerId);
        }

        private void OnServerDisconnectedEvent(ServerDisconnectedEvent e)
        {
            RemoveAllRemotePlayers();
            ReleaseLocalPlayer();
            _avatarPool.Clear();
            _knownPlayers.Clear();
            _localPlayerId = 0;
        }

        private void SyncRemotePlayers()
        {
            if (!_gameManager.IsInGame)
            {
                return;
            }

            foreach (var pair in _knownPlayers)
            {
                SyncPlayer(pair.Value);
            }
        }

        private void SyncPlayer(IClientPlayer clientPlayer)
        {
            var info = clientPlayer.Info as PlayerInfo;
            if (info == null || info.Id == _localPlayerId)
            {
                return;
            }

            PlayerBase existing;
            if (_players.TryGetValue(info.Id, out existing))
            {
                existing.Name = info.Name;
                return;
            }

            var avatar = _avatarPool.Rent(info);
            if (avatar != null)
            {
                _players[info.Id] = avatar;
                _logger.Info("Avatar created for remote player {PlayerName} ({PlayerId}).", info.Name, info.Id);
            }
        }

        private void EnsureLocalPlayer()
        {
            if (_localPlayer != null)
            {
                return;
            }

            var player = _gameManager.Player;
            if (player == null)
            {
                _logger.Warn("Local player GameObject not found, cannot attach LocalPlayer.");
                return;
            }

            var localPlayer = player.GetComponent<LocalPlayer>();
            if (localPlayer == null)
            {
                localPlayer = player.AddComponent<LocalPlayer>();
            }

            IClientPlayer self;
            string name = _knownPlayers.TryGetValue(_localPlayerId, out self) ? self.Info.Name : null;
            localPlayer.Init(new PlayerInfo { Id = _localPlayerId, Name = name });
            _localPlayer = localPlayer;
            _players[_localPlayerId] = localPlayer;
        }

        private void ReleaseLocalPlayer()
        {
            if (_localPlayer == null)
            {
                return;
            }

            _players.Remove(_localPlayerId);
            _localPlayer = null;
            // LocalPlayer 组件挂在真实玩家对象上，随场景卸载自动销毁，无需手动处理。
        }

        private void RemoveAllRemotePlayers()
        {
            foreach (var pair in _players)
            {
                if (pair.Value == _localPlayer)
                {
                    continue;
                }

                var remote = pair.Value as RemotePlayer;
                if (remote != null)
                {
                    _avatarPool.Return(remote);
                }
            }
            _players.Clear();
        }
    }
}
