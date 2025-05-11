using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Client.Interfaces
{
    public interface IGameClient : IClientEventListener
    {
        Dictionary<int, ClientPlayer> Players { get; }
        int Ping { get; }
        void Start(string host, int port, string playerName);
        void Reset();
        void Stop();
        void SendChatMessage(string message);
    }
}
