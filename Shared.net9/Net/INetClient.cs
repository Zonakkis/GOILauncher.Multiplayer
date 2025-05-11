using GOILauncher.Multiplayer.Shared.Events;
using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;
using System;

namespace GOILauncher.Multiplayer.Shared.Net
{
    public interface INetClient
    {
        event EventHandler<ServerDisconnectedEventArgs> ServerDisconnected;
        EventBasedNetListener Listener { get; }
        bool IsRunning { get; }
        long BytesSent { get; }
        long BytesReceived { get; }
        long PacketLossPercentage { get; }

        int Ping { get; }
        void Start(string host, int port);
        void Stop();
        void Poll();
        void Send(IPacket packet, DeliveryMethod deliveryMethod);
    }
}
