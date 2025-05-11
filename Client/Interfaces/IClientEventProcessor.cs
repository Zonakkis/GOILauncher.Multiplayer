using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Interfaces
{
    internal interface IClientEventProcessor
    {
        void OnNetworkReceived(NetPeer server, NetPacketReader reader, DeliveryMethod deliveryMethod);

        void OnServerConnected(NetPeer server, ServerConnectedPacket packet);

        void OnChatMessageReceived(NetPeer server, ChatMessagePacket packet);
    }
}