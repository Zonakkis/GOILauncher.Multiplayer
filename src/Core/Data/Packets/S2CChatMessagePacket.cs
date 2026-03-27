using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CChatMessagePacket : INetSerializable
    {
        public int PlayerId { get; set; }
        public string Message { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Message);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            {
                PlayerId = reader.GetInt();
                Message = reader.GetString();
            }
        }
    }
}
