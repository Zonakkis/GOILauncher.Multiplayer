using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Server.Interfaces
{
    public interface IGameServer : IServerEventListener
    {
        Dictionary<int, ServerPlayer> Players { get; }
        bool IsRunning { get; }
        void Start(int port);
        void Stop();
    }
}