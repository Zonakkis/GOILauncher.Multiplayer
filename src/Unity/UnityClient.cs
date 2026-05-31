using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Unity.Extensions;
using UnityEngine;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Client.Events;
using System;

namespace GOILauncher.Multiplayer.Client
{

    public class UnityClient : MonoBehaviour, IUnityClient
    {
        public bool IsConnected => ClientService.IsConnected;
        public IList<ClientPlayer> Players { get; private set; } = new List<ClientPlayer>();
        public List<Message> ChatMessages => ChatService.Messages;
        public IClientService ClientService { get; set; }
        public IPlayerService PlayerService { get; set; }
        public IChatService ChatService { get; set; }
        public IEventBus EventBus { get; set; }
        public event EventHandler<PlayerListUpdatedEventArgs> PlayerListUpdated;

        public void Init()
        {
            EventBus.Subscribe<PlayerListUpdatedEvent>(OnPlayerListUpdatedEvent);
        }

        private void Update()
        {
            ClientService?.Poll();
        }

        public void Connect(string host, int port, string playerName)
        {
            var playerMetadata = new PlayerMetadata
            {
                Name = playerName,
                Platform = Application.platform.ToPlatform()
            };

            PlayerService.UpdateLocalPlayerMetadata(playerMetadata);
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
    }
}
