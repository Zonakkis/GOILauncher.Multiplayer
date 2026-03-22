using GOILauncher.Multiplayer.Client.Services;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client
{
    public class UnityClient : MonoBehaviour
    {
        public IClientService ClientService { get; set; }

        public void Connect(string host, int port)
        {
            ClientService.Connect(host, port);
        }

        public void Disconnect()
        {
            ClientService.Disconnect();
        }
    }
}
