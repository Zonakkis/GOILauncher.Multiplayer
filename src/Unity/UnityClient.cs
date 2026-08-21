using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Unity.Config;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Extensions;
using GOILauncher.Multiplayer.Unity.Player;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity
{

    public class UnityClient : MonoBehaviour, IUnityClient
    {
        private bool _initialized;

        public bool IsConnected => IsMultiplayerEnabled && ClientService.IsConnected;
        public ReadOnlyCollection<Message> ChatMessages => ChatService.Messages;
        public IClientService ClientService { get; set; }
        public IPlayerService PlayerService { get; set; }
        public IChatService ChatService { get; set; }
        public IEventBus EventBus { get; set; }
        public IGameManager GameManager { get; set; }
        public IPlayerManager PlayerManager { get; set; }
        public IMultiplayerState MultiplayerState { get; set; }

        public event EventHandler PlayerListUpdated;

        /// <summary>
        /// 每次枚举都现场造 PlayerView，不留副本：名单的唯一所有者是 PlayerService。
        /// </summary>
        public IEnumerable<PlayerView> Players
        {
            get
            {
                foreach (PlayerInfo info in PlayerService.Players)
                    yield return new PlayerView(info.Id, PlayerService, PlayerManager);
            }
        }

        public bool TryGetPlayer(int playerId, out PlayerView player)
        {
            PlayerInfo info;
            if (!PlayerService.TryGetPlayer(playerId, out info))
            {
                player = default(PlayerView);
                return false;
            }

            player = new PlayerView(info.Id, PlayerService, PlayerManager);
            return true;
        }

        public void Init()
        {
            if (_initialized)
                return;

            _initialized = true;
            EventBus.Subscribe<GameStartedEvent>(OnGameStartedEvent);
            EventBus.Subscribe<GameQuitEvent>(OnGameQuitEvent);
            EventBus.Subscribe<PlayerListUpdatedEvent>(OnPlayerListUpdatedEvent);
        }

        private void Update()
        {
            ClientService?.Poll();
        }

        public void Connect(string host, int port, string playerName)
        {
            if (!IsMultiplayerEnabled)
                return;

            var playerInfo = new PlayerInfo(0, playerName, Application.platform.ToPlatform(), GameManager.IsInGame);

            PlayerService.SetLocalPlayerInfo(playerInfo);
            ClientService.Connect(host, port);
        }

        public void Disconnect()
        {
            ClientService.Disconnect();
        }

        public void SendMessage(MessageType type, string message)
        {
            if (!IsMultiplayerEnabled)
                return;

            ChatService.SendMessage(type, message);
        }

        private bool IsMultiplayerEnabled
        {
            get { return MultiplayerState == null || MultiplayerState.Enabled; }
        }

        private void OnGameStartedEvent(GameStartedEvent @event)
        {
            PlayerService.SetIsInGame(true);
        }

        private void OnGameQuitEvent(GameQuitEvent @event)
        {
            PlayerService.SetIsInGame(false);
        }

        private void OnPlayerListUpdatedEvent(PlayerListUpdatedEvent @event)
        {
            EventHandler handler = PlayerListUpdated;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
