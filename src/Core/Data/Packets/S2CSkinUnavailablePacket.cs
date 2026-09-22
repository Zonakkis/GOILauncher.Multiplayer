using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    /// <summary>
    /// "这张皮肤我这儿没有"。请求必须有个回音，否则客户端会一直等着一份永远不来的字节，
    /// 那名玩家的罐子就卡在原版上，也不会重试。
    /// </summary>
    public struct S2CSkinUnavailablePacket : INetSerializable
    {
        public RoomPacketScope Scope { get; set; }
        public int PlayerId { get; set; }

        /// <summary>是哪个槽位拿不到，收端据此只把那个部件回退到原版。</summary>
        public byte Slot { get; set; }

        public SkinHash Hash { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Scope);
            writer.Put(PlayerId);
            writer.Put(Slot);
            writer.Put(Hash);
        }

        public void Deserialize(NetDataReader reader)
        {
            Scope = reader.Get<RoomPacketScope>();
            PlayerId = reader.GetInt();
            Slot = reader.GetByte();
            Hash = reader.Get<SkinHash>();
        }
    }
}
