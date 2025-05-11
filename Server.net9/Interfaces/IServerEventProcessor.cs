using GOILauncher.Multiplayer.Shared.Events;
using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Interfaces
{
    internal interface IServerEventProcessor
    {
        void OnClientConnected(object sender, ClientConnectedEventArgs e);
        void OnNetworkReceived(NetPeer player, NetPacketReader reader, DeliveryMethod deliveryMethod);

        void OnPlayerConnected(NetPeer peer, PlayerConnectedPacket packet);
        void OnChatMessageReceived(NetPeer peer, ChatMessagePacket packet);
    }
}