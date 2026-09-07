using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CChatMessagePacket : INetSerializable
    {
        public RoomPacketScope Scope { get; set; }
        public int PlayerId { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Unix timestamp in seconds
        /// </summary>
        public long Timestamp { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Scope);
            writer.Put(PlayerId);
            writer.Put(Content);
            writer.Put(Timestamp);
        }

        public void Deserialize(NetDataReader reader)
        {
            Scope = reader.Get<RoomPacketScope>();
            PlayerId = reader.GetInt();
            Content = reader.GetString();
            Timestamp = reader.GetLong();
        }
    }
}
