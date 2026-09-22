using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    /// <summary>
    /// 向服务端索取某名玩家皮肤的 PNG 字节。带上哈希是为了让服务端能判出"你要的是旧那张"：
    /// 请求在路上时对方可能已经换了皮肤。
    /// </summary>
    public struct C2SSkinRequestPacket : INetSerializable
    {
        public RoomPacketScope Scope { get; set; }
        public int PlayerId { get; set; }

        /// <summary>要哪个部件的皮肤。服务端按 (玩家, 槽位) 存字节，同一玩家两个槽位可能同哈希。</summary>
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
