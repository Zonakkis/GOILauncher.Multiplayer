using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CChatMessagePacket : INetSerializable
    {
        public int PlayerId { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Unix timestamp in seconds
        /// </summary>
        public long Timestamp { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Content);
            writer.Put(Timestamp);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            Content = reader.GetString();
            Timestamp = reader.GetLong();
        }
    }
}
