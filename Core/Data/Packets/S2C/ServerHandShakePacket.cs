using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets.S2C
{
    public struct ServerHandShakePacket : IPacket, INetSerializable
    {
        public PacketType Type => PacketType.ServerHandShake;
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