using GOILauncher.Multiplayer.Server.Services;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity
{
    public class UnityServer : MonoBehaviour, IUnityServer
    {
        public IServerService ServerService { get; set; }
        public IPlayerService PlayerService { get; set; }
        public ChatService ChatService { get; set; }
        public IMultiplayerState MultiplayerState { get; set; }

        public bool IsRunning => IsMultiplayerEnabled && ServerService.IsRunning;

        public void Start()
        {
            
        }
        public void Start(int port)
        {
            if (!IsMultiplayerEnabled)
                return;

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

        private bool IsMultiplayerEnabled
        {
            get { return MultiplayerState == null || MultiplayerState.Enabled; }
        }
    }
}
