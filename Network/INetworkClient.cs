using GOILauncher.Multiplayer.Network.Enums;
using System;

namespace GOILauncher.Multiplayer.Network
{
    public interface INetworkClient
    {
        bool IsConnected { get; }

        event Action Connected;
        event Action Disconnected;
        event Action<byte[]> DataReceived;

        void Connect(string host, int port);
        void Disconnect();
        void Poll();
        void Send(byte[] data, SendMode mode);
    }
}
