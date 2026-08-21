using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Unity.Config;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Extensions;
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
        public IMultiplayerState MultiplayerState { get; set; }

        public void Init()
        {
            if (_initialized)
                return;

            _initialized = true;
            EventBus.Subscribe<GameStartedEvent>(OnGameStartedEvent);
            EventBus.Subscribe<GameQuitEvent>(OnGameQuitEvent);
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
    }
}
