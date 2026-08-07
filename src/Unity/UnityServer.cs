using GOILauncher.Multiplayer.Server.Services;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity
{
    public class UnityServer : MonoBehaviour, IUnityServer
    {
        public bool IsRunning => ServerService.IsRunning;
        public IServerService ServerService { get; set; }
        public IPlayerService PlayerService { get; set; }
        public ChatService ChatService { get; set; }

        public void Start()
        {
            
        }
        public void Start(int port)
        {
            ServerService.Start(port);
        }

        public void Stop()
        {
            ServerService.Stop();
        }

        public void Update()
        {
            ServerService?.Poll();
        }
    }
}
