using System.Collections.Concurrent;

namespace GOILauncher.Multiplayer.Server.Interfaces
{
    public interface IGameServer : IServerEventListener
    {
        ConcurrentDictionary<int, ServerPlayer> Players { get; }
        bool IsRunning { get; }
        void Start(int port);
        void Stop();
    }
}