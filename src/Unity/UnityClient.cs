using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Unity.Extensions;
using UnityEngine;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Core.Data;

namespace GOILauncher.Multiplayer.Client
{

    public class UnityClient : MonoBehaviour, IUnityClient
    {
        public bool IsConnected => ClientService.IsConnected;
        public ReadOnlyDictionary<int, ClientPlayer> Players => PlayerService.Players.AsReadOnly();
        public List<Message> ChatMessages => ChatService.Messages;
        public IClientService ClientService { get; set; }
        public IPlayerService PlayerService { get; set; }
        public IChatService ChatService { get; set; }

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
    }
}
