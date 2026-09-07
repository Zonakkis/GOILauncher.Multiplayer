using System;
using GOILauncher.Multiplayer.Core.Utils;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct C2SChatMessagePacket : INetSerializable
    {
        public ulong MembershipId { get; set; }

        public string Content { get; set; }
        /// <summary>
        /// Unix timestamp in seconds
        /// </summary>
        public long Timestamp { get; set; }
        public C2SChatMessagePacket(string content)
        {
            MembershipId = 0;
            Content = content;
            Timestamp = DateTimeUtils.ToUnixTimeSeconds(DateTime.Now);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MembershipId);
            writer.Put(Content);
            writer.Put(Timestamp);
        }

        public void Deserialize(NetDataReader reader)
        {
            MembershipId = reader.GetULong();
            Content = reader.GetString();
            Timestamp = reader.GetLong();
        }
    }
}
