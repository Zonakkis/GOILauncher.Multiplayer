using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public class NetPacketWriter : NetDataWriter
    {
        public NetPacketWriter(IPacket packet)
        {
            Put((byte)packet.PacketType);
            Put(packet);
        }
    }
}
