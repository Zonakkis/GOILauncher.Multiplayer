using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    /// <summary>
    /// 把自己皮肤的 PNG 字节上传给服务端。客户端在宣告清单之后主动发，不等服务端来要——
    /// 服务端总是没有新哈希的字节，多一次往返只是白等。
    /// </summary>
    public struct C2SSkinDataPacket : INetSerializable
    {
        /// <summary>这份字节属于哪个部件的槽位。服务端按 (发送方, 槽位) 存，别的槽位不受影响。</summary>
        public byte Slot { get; set; }

        public SkinBlob Blob { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Slot);
            writer.Put(Blob);
        }

        public void Deserialize(NetDataReader reader)
        {
            Slot = reader.GetByte();
            Blob = reader.Get<SkinBlob>();
        }
    }
}
