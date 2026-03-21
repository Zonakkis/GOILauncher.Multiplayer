using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets.C2S
{
    public struct ClientHandShakePacket : INetSerializable
    {
        public string PlayerName { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerName);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerName = reader.GetString();
        }
    }
}