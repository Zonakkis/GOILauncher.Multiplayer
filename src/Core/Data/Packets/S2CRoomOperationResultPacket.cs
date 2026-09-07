using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CRoomOperationResultPacket : INetSerializable
    {
        public RoomOperationResult Result { get; set; }
        public void Serialize(NetDataWriter writer) { writer.Put(Result); }
        public void Deserialize(NetDataReader reader) { Result = reader.Get<RoomOperationResult>(); }
    }
}
