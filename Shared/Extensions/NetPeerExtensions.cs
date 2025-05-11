using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Extensions
{
    public static class NetPeerExtensions
    {
        public static void Send(this NetPeer peer, IPacket packet, DeliveryMethod deliveryMethod)
        {
            NetDataWriter writer = new NetPacketWriter(packet);
            peer.Send(writer, deliveryMethod);
        }
    }
}
