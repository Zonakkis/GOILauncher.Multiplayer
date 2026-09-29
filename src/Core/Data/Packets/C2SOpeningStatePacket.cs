using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct C2SOpeningStatePacket : INetSerializable
    {
        public ulong MembershipId { get; set; }
        public OpeningState State { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MembershipId);
            writer.Put(State);
        }

        public void Deserialize(NetDataReader reader)
        {
            MembershipId = reader.GetULong();
            State = reader.Get<OpeningState>();
        }
    }
}
