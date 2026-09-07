using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public sealed class S2CRoomListPacket : INetSerializable
    {
        public List<RoomInfo> Rooms { get; set; } = new List<RoomInfo>();
        public void Serialize(NetDataWriter writer)
        { writer.Put(Rooms.Count); foreach (var room in Rooms) writer.Put(room); }
        public void Deserialize(NetDataReader reader)
        {
            Rooms.Clear();
            int count = reader.GetInt();
            if (count < 0 || count > reader.AvailableBytes) throw new ParseException("Invalid room count.");
            for (int i = 0; i < count; i++) Rooms.Add(reader.Get<RoomInfo>());
        }
    }
}
