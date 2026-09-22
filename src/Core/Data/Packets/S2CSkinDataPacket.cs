using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    /// <summary>某名玩家皮肤的 PNG 字节。</summary>
    public struct S2CSkinDataPacket : INetSerializable
    {
        public RoomPacketScope Scope { get; set; }
        public int PlayerId { get; set; }

        /// <summary>这份字节属于哪个部件的槽位，收端据此贴到对应的 mesh。</summary>
        public byte Slot { get; set; }

        public SkinBlob Blob { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Scope);
            writer.Put(PlayerId);
            writer.Put(Slot);
            writer.Put(Blob);
        }

        public void Deserialize(NetDataReader reader)
        {
            Scope = reader.Get<RoomPacketScope>();
            PlayerId = reader.GetInt();
            Slot = reader.GetByte();
            Blob = reader.Get<SkinBlob>();
        }
    }
}
