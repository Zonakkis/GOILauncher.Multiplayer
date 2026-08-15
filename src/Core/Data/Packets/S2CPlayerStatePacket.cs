using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CPlayerStatePacket : INetSerializable
    {
        public int PlayerId { get; set; }
        public uint Sequence { get; set; }
        public PlayerState State { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Sequence);
            writer.Put(State);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            Sequence = reader.GetUInt();
            State = reader.Get<PlayerState>();
        }
    }
}
