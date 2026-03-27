using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Extensions;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CPlayerLeftPacket : INetSerializable
    {
        public int PlayerId { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
        }
    }
}
