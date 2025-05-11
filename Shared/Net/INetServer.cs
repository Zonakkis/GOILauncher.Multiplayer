using System;
using GOILauncher.Multiplayer.Shared.Events;
using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Shared.Net
{
    public interface INetServer : INet
    {

        event EventHandler<ClientConnectedEventArgs> ClientConnected;
        event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        EventBasedNetListener Listener { get; }
        bool IsRunning { get; }

        void Start(int port);

        void Stop();

        void Poll();

        void SendTo(int clientId, IPacket packet, DeliveryMethod deliveryMethod);

        void SendToAll(IPacket packet, DeliveryMethod deliveryMethod);

        void SendToAllExcept(int exceptClientId, IPacket packet, DeliveryMethod deliveryMethod);
    }
}
