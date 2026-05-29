using System;

namespace GOILauncher.Multiplayer.Server.Services
{
    public interface IServerService : IDisposable
    {
        bool IsRunning { get; }
        void Start(int port);
        void Stop();
        void Poll();
    }
}