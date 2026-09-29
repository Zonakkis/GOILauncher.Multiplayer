using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2COpeningStatePacket : INetSerializable
    {
        public RoomPacketScope Scope { get; set; }
        public int PlayerId { get; set; }
        public OpeningState State { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Scope);
            writer.Put(PlayerId);
            writer.Put(State);
        }

        public void Deserialize(NetDataReader reader)
        {
            Scope = reader.Get<RoomPacketScope>();
            PlayerId = reader.GetInt();
            State = reader.Get<OpeningState>();
        }
    }
}
