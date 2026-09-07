using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct C2SPlayerStatePacket : INetSerializable
    {
        public ulong MembershipId { get; set; }
        public uint Sequence { get; set; }
        public PlayerState State { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MembershipId);
            writer.Put(Sequence);
            writer.Put(State);
        }

        public void Deserialize(NetDataReader reader)
        {
            MembershipId = reader.GetULong();
            Sequence = reader.GetUInt();
            State = reader.Get<PlayerState>();
        }
    }
}
