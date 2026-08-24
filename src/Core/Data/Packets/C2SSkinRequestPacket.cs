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
        public int PlayerId { get; set; }
        public SkinHash Hash { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Hash);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            Hash = reader.Get<SkinHash>();
        }
    }
}
