using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace GOILauncher.Multiplayer.Network
{
    public interface INetworkClient : IDisposable
    {
        bool IsConnected { get; }
        void Connect(string host, int port);
        void Disconnect();
        void Poll();
        void Send(INetSerializable packet, DeliveryMethod method);
    }
}
