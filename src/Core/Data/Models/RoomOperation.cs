using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    public enum RoomOperation : byte { Refresh, Create, Join, Leave, Update }
    public enum RoomPasswordChange : byte { Keep, Set, Remove }
    public enum RoomError : byte
    {
        None, NotReady, Busy, RoomNotFound, IncorrectPassword, RoomFull,
        NotOwner, InvalidName, InvalidCapacity, InvalidPassword, Forbidden, StaleMembership
    }
    public struct RoomOperationResult : INetSerializable
    {
        public uint RequestId { get; set; }
        public RoomOperation Operation { get; set; }
        public RoomError Error { get; set; }
        public bool IsSuccess => Error == RoomError.None;
        public void Serialize(NetDataWriter writer)
        { writer.Put(RequestId); writer.Put((byte)Operation); writer.Put((byte)Error); }
        public void Deserialize(NetDataReader reader)
        { RequestId = reader.GetUInt(); Operation = (RoomOperation)reader.GetByte(); Error = (RoomError)reader.GetByte(); }
    }
}
