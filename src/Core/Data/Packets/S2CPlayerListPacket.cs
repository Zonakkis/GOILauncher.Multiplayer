using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    /// <summary>Authoritative room-entry snapshot, including the local member.</summary>
    public sealed class S2CPlayerListPacket : INetSerializable
    {
        public RoomInfo Room { get; set; }
        public List<RoomMemberInfo> Members { get; set; } = new List<RoomMemberInfo>();
        public void Serialize(NetDataWriter writer)
        { writer.Put(Room); writer.Put(Members.Count); foreach (var member in Members) writer.Put(member); }
        public void Deserialize(NetDataReader reader)
        {
            Room = reader.Get<RoomInfo>(); Members.Clear();
            int count = reader.GetInt();
            if (count < 0 || count > reader.AvailableBytes) throw new ParseException("Invalid member count.");
            for (int i = 0; i < count; i++) Members.Add(reader.Get<RoomMemberInfo>());
        }
    }
}
