using GOILauncher.Multiplayer.Network.Enums;
using System;

namespace GOILauncher.Multiplayer.Network
{
    public interface INetworkServer
    {
        event Action<int> Connected;
        event Action<int> Disconnected;
        event Action<byte[]> DataReceived;

        void Start(int port);
        void Stop();
        void Poll();
        void Send(int clientId, byte[] data, SendMode mode);
    }
}
