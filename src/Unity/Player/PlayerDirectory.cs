using System;
using System.Collections.Generic;
using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Unity.Models;

namespace GOILauncher.Multiplayer.Unity.Player
{
    public class PlayerDirectory : IPlayerDirectory, IStartable, IDisposable
    {
        private readonly IPlayerService _playerService;
        private readonly IPlayerManager _playerManager;
        private readonly IEventBus _eventBus;

        private IDisposable _subscription;

        public event EventHandler RosterChanged;

        public PlayerDirectory(IPlayerService playerService,
            IPlayerManager playerManager,
            IEventBus eventBus)
        {
            _playerService = playerService;
            _playerManager = playerManager;
            _eventBus = eventBus;
        }

        void IStartable.Start()
        {
            _subscription = _eventBus.Subscribe<PlayerListUpdatedEvent>(OnPlayerListUpdated);
        }

        public void Dispose()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }

        public int LocalPlayerId
        {
            get
            {
                var local = _playerService.LocalPlayer;
                return local == null ? 0 : local.Id;
            }
        }

        public IEnumerable<PlayerInfo> Players
        {
            get { return _playerService.Players; }
        }

        public bool TryGetPlayer(int playerId, out PlayerInfo player)
        {
            return _playerService.TryGetPlayer(playerId, out player);
        }

        public bool TryGetDistance(int playerId, out float meters)
        {
            // 本地玩家拿到的是 LocalPlayer 而不是 RemotePlayer，这里同样返回 false。
            var remote = _playerManager.GetPlayer(playerId) as RemotePlayer;
            if (remote == null)
            {
                meters = 0f;
                return false;
            }

            meters = remote.DistanceToLocalPlayer;
            return true;
        }

        private void OnPlayerListUpdated(PlayerListUpdatedEvent e)
        {
            var handler = RosterChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
