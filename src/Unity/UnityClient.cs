using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Extensions;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity
{

    public class UnityClient : MonoBehaviour, IUnityClient
    {
        private bool _initialized;

        public bool IsConnected => ClientService.IsConnected;
        public IList<PlayerInfo> Players { get; private set; } = new List<PlayerInfo>();
        public List<Message> ChatMessages => ChatService.Messages;
        public IClientService ClientService { get; set; }
        public IPlayerService PlayerService { get; set; }
        public IChatService ChatService { get; set; }
        public IEventBus EventBus { get; set; }
        public IGameManager GameManager { get; set; }

        public event EventHandler<PlayerListUpdatedEventArgs> PlayerListUpdated;

        public void Init()
        {
            if (_initialized)
                return;

            _initialized = true;
            EventBus.Subscribe<PlayerListUpdatedEvent>(OnPlayerListUpdatedEvent);
            EventBus.Subscribe<GameStartedEvent>(OnGameStartedEvent);
            EventBus.Subscribe<GameQuitEvent>(OnGameQuitEvent);
        }

        private void Update()
        {
            ClientService?.Poll();
        }

        public void Connect(string host, int port, string playerName)
        {
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
            ChatService.SendMessage(type, message);
        }

        private void OnPlayerListUpdatedEvent(PlayerListUpdatedEvent @event)
        {
            Players = @event.Players;
            PlayerListUpdated?.Invoke(this, new PlayerListUpdatedEventArgs(Players));
        }

        private void OnGameStartedEvent(GameStartedEvent @event)
        {
            PlayerService.SetIsInGame(true);
        }

        private void OnGameQuitEvent(GameQuitEvent @event)
        {
            PlayerService.SetIsInGame(false);
        }
    }
}
