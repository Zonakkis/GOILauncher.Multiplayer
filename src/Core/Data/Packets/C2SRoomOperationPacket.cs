using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct C2SRoomOperationPacket : INetSerializable
    {
        public uint RequestId { get; set; }
        public ulong MembershipId { get; set; }
        public RoomOperation Operation { get; set; }
        public int RoomId { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public RoomPasswordChange PasswordChange { get; set; }
        public string Password { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(RequestId); writer.Put(MembershipId); writer.Put((byte)Operation);
            writer.Put(RoomId); writer.Put(Name ?? ""); writer.Put(MaxPlayers);
            writer.Put((byte)PasswordChange); writer.Put(Password ?? "");
        }
        public void Deserialize(NetDataReader reader)
        {
            RequestId = reader.GetUInt(); MembershipId = reader.GetULong(); Operation = (RoomOperation)reader.GetByte();
            RoomId = reader.GetInt(); Name = reader.GetString(); MaxPlayers = reader.GetInt();
            PasswordChange = (RoomPasswordChange)reader.GetByte(); Password = reader.GetString();
        }
    }
}
