using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Network
{
    public interface INetworkServer : IDisposable
    {
        bool IsRunning { get; }
        void Start(int port);
        void Stop();
        void Poll();
        void Send(int clientId, INetSerializable packet, DeliveryMethod method);
        void Multicast(IEnumerable<int> clientIds, INetSerializable packet, DeliveryMethod method);
        void Broadcast(INetSerializable packet, DeliveryMethod method);
    }
}
