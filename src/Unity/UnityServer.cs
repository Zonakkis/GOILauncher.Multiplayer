using GOILauncher.Multiplayer.Server.Services;

namespace GOILauncher.Multiplayer.Unity
{
    public class UnityServer
    {
        public bool IsRunning => ServerService.IsRunning;
        public IServerService ServerService { get; set; }

        public void Start(int port)
        {
            ServerService.Start(port);
        }

        public void Stop()
        {
            ServerService.Stop();
        }
    }
}
