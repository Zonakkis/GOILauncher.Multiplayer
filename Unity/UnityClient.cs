using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Unity.Extensions;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client
{
    public class UnityClient : MonoBehaviour
    {
        public IClientService ClientService { get; set; }
        public IPlayerService PlayerService { get; set; }

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
            ClientService?.Disconnect();
        }
    }
}
