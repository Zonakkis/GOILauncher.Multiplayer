using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct C2SPlayerStatePacket : INetSerializable
    {
        public uint Sequence { get; set; }
        public PlayerState State { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Sequence);
            writer.Put(State);
        }

        public void Deserialize(NetDataReader reader)
        {
            Sequence = reader.GetUInt();
            State = reader.Get<PlayerState>();
        }
    }
}
