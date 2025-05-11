using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public class PlayerLeftPacket : IPacket
    {
        public PacketType PacketType => PacketType.PlayerLeft;
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
