using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public class ServerConnectedPacket : IPacket
    {
        public PacketType PacketType => PacketType.ServerConnected;
        public int PlayerId { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
        }
    }
}
