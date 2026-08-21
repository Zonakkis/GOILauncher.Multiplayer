using System;
using System.Collections.Generic;
using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Models;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// 只负责远端玩家的 Unity 实例生命周期。名单本身不在这里维护——身份、名字和
    /// IsInGame 一律从 IPlayerService 读取，本类只保存"哪个玩家当前有实例"。
    /// </summary>
    public class PlayerManager : IPlayerManager, IStartable, IDisposable
    {
        private const int DefaultInstanceWarmUpCount = 4;

        private readonly IEventBus _eventBus;
        private readonly IGameManager _gameManager;
        private readonly IPlayerService _playerService;
        private readonly IPlayerInstancePool _instancePool;
        private readonly ILogger<PlayerManager> _logger;

        private readonly Dictionary<int, PlayerBase> _players = new Dictionary<int, PlayerBase>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        private LocalPlayer _localPlayer;

        public LocalPlayer LocalPlayer
        {
            get { return _localPlayer; }
        }

        public PlayerManager(IEventBus eventBus,
            IGameManager gameManager,
            IPlayerService playerService,
            IPlayerInstancePool instancePool,
            ILogger<PlayerManager> logger)
        {
            _eventBus = eventBus;
            _gameManager = gameManager;
            _playerService = playerService;
            _instancePool = instancePool;
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

        void IStartable.Start()
        {
            // 订阅的都是"发生了什么"的事件。PlayerListUpdatedEvent 只是 UI 刷新信号，
            // 拿它驱动实例增删会让同一次变化走两条路径。
            _subscriptions.Add(_eventBus.Subscribe<LocalPlayerReadyEvent>(OnLocalPlayerReadyEvent));
            _subscriptions.Add(_eventBus.Subscribe<PlayerRosterReceivedEvent>(OnPlayerRosterReceivedEvent));
            _subscriptions.Add(_eventBus.Subscribe<GameStartedEvent>(OnGameStartedEvent));
            _subscriptions.Add(_eventBus.Subscribe<GameRestartedEvent>(OnGameRestartedEvent));
            _subscriptions.Add(_eventBus.Subscribe<GameQuitEvent>(OnGameQuitEvent));
            _subscriptions.Add(_eventBus.Subscribe<PlayerJoinedEvent>(OnPlayerJoinedEvent));
            _subscriptions.Add(_eventBus.Subscribe<PlayerEnteredGameEvent>(OnPlayerEnteredGameEvent));
            _subscriptions.Add(_eventBus.Subscribe<PlayerQuitGameEvent>(OnPlayerQuitGameEvent));
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

        private int LocalPlayerId
        {
            get
            {
                var local = _playerService.LocalPlayer;
                return local == null ? 0 : local.Id;
            }
        }

        private void OnLocalPlayerReadyEvent(LocalPlayerReadyEvent e)
        {
            _logger.Info("Handshake completed, local player id: {PlayerId}", LocalPlayerId);

            if (_gameManager.IsInGame)
            {
                InitializeGamePlayers();
            }
        }

        /// <summary>
        /// 加入一个已经有人的服务器时，这些玩家不会再产生 PlayerJoinedEvent，
        /// 只能靠这一次名单快照补齐实例。
        /// </summary>
        private void OnPlayerRosterReceivedEvent(PlayerRosterReceivedEvent e)
        {
            SyncRemotePlayers();
        }

        private void OnGameStartedEvent(GameStartedEvent e)
        {
            InitializeGamePlayers();
        }

        private void OnGameRestartedEvent(GameRestartedEvent e)
        {
            RemoveAllRemotePlayers();
            ReleaseLocalPlayer();
            _instancePool.Clear();
            InitializeGamePlayers();
        }

        private void OnGameQuitEvent(GameQuitEvent e)
        {
            RemoveAllRemotePlayers();
            ReleaseLocalPlayer();
            _instancePool.Clear();
        }

        private void OnPlayerJoinedEvent(PlayerJoinedEvent e)
        {
            // PlayerService 先写名单再发事件，这里一定读得到。
            PlayerInfo info;
            if (_playerService.TryGetPlayer(e.PlayerId, out info))
            {
                EnsureRemoteInstance(info);
            }
        }

        private void OnPlayerEnteredGameEvent(PlayerEnteredGameEvent e)
        {
            EnsureRemoteInstance(e.Player);
        }

        private void OnPlayerQuitGameEvent(PlayerQuitGameEvent e)
        {
            var info = e.Player;
            if (info == null)
            {
                return;
            }

            if (ReleaseRemoteInstance(info.Id))
            {
                _logger.Info("Player {PlayerName} ({PlayerId}) quit the game.", info.Name, info.Id);
            }
        }

        private void OnPlayerLeftEvent(PlayerLeftEvent e)
        {
            if (ReleaseRemoteInstance(e.PlayerId))
            {
                _logger.Info("Player {PlayerName} ({PlayerId}) removed.", e.PlayerName, e.PlayerId);
            }
        }

        private void OnServerDisconnectedEvent(ServerDisconnectedEvent e)
        {
            RemoveAllRemotePlayers();
            ReleaseLocalPlayer();
            _instancePool.Clear();
        }

        private void SyncRemotePlayers()
        {
            if (!_gameManager.IsInGame)
            {
                return;
            }

            foreach (var info in _playerService.Players)
            {
                EnsureRemoteInstance(info);
            }
        }

        /// <summary>
        /// 远端玩家出现的唯一入口：名单快照、中途加入、中途进入游戏都走这里，
        /// 避免同一段创建逻辑散在多个事件处理器里。
        /// </summary>
        private void EnsureRemoteInstance(PlayerInfo info)
        {
            if (info == null || info.Id == LocalPlayerId || !info.IsInGame || !_gameManager.IsInGame)
            {
                return;
            }

            PlayerBase existing;
            if (_players.TryGetValue(info.Id, out existing))
            {
                existing.Name = info.Name;
                return;
            }

            var instance = _instancePool.Rent(info);
            if (instance != null)
            {
                _players[info.Id] = instance;
                _logger.Info("Instance created for remote player {PlayerName} ({PlayerId}).", info.Name, info.Id);
            }
        }

        private bool ReleaseRemoteInstance(int playerId)
        {
            PlayerBase player;
            if (!_players.TryGetValue(playerId, out player))
            {
                return false;
            }

            _players.Remove(playerId);
            var remote = player as RemotePlayer;
            if (remote != null)
            {
                _instancePool.Return(remote);
            }
            return true;
        }

        private void InitializeGamePlayers()
        {
            EnsureLocalPlayer();
            _instancePool.WarmUp(DefaultInstanceWarmUpCount);
            SyncRemotePlayers();
        }

        private void EnsureLocalPlayer()
        {
            if (_localPlayer != null)
            {
                BindLocalPlayer(_localPlayer);
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

            BindLocalPlayer(localPlayer);
        }

        private void BindLocalPlayer(LocalPlayer localPlayer)
        {
            _players.Remove(localPlayer.Id);

            var localId = LocalPlayerId;
            PlayerInfo self;
            var name = _playerService.TryGetPlayer(localId, out self) && self != null
                ? self.Name
                : localPlayer.Name;
            localPlayer.Init(new PlayerInfo(localId, name, Platform.PC, false));
            _localPlayer = localPlayer;
            _players[localId] = localPlayer;
        }

        private void ReleaseLocalPlayer()
        {
            // 用实例上记录的 Id 而不是当前 LocalPlayerId：断线时 PlayerService 会先把
            // 本地身份重置为 0，那时再去查 Id 就删不掉这条记录了。
            if (_localPlayer != null)
            {
                _players.Remove(_localPlayer.Id);
            }
            _localPlayer = null;
            // 场景重载后 Unity 对象可能已销毁，但仍需清除托管引用。
        }

        private void RemoveAllRemotePlayers()
        {
            foreach (var pair in _players)
            {
                if (ReferenceEquals(pair.Value, _localPlayer))
                {
                    continue;
                }

                var remote = pair.Value as RemotePlayer;
                if (remote != null)
                {
                    _instancePool.Return(remote);
                }
            }
            _players.Clear();
        }
    }
}
