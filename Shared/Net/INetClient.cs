using GOILauncher.Multiplayer.Shared.Events;
using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;
using System;

namespace GOILauncher.Multiplayer.Shared.Net
{
    public interface INetClient : INet
    {
        event EventHandler<ServerDisconnectedEventArgs> ServerDisconnected;
        EventBasedNetListener Listener { get; }
        bool IsRunning { get; }

        int Ping { get; }
        void Start(string host, int port);
        void Stop();
        void Poll();
        void Send(IPacket packet, DeliveryMethod deliveryMethod);
    }
}
