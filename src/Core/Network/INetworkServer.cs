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
        void Send<T>(int clientId, T packet, DeliveryMethod method) where T : class, new();
        void Send(int clientId, INetSerializable packet, DeliveryMethod method);
        void Multicast<T>(IEnumerable<int> clientIds, T packet, DeliveryMethod method) where T : class, new();
        void Multicast(IEnumerable<int> clientIds, INetSerializable packet, DeliveryMethod method);
        void Broadcast<T>(T packet, DeliveryMethod method) where T : class, new();
        void Broadcast(INetSerializable packet, DeliveryMethod method);
    }
}
