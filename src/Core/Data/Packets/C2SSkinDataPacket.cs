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
        public SkinBlob Blob { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Blob);
        }

        public void Deserialize(NetDataReader reader)
        {
            Blob = reader.Get<SkinBlob>();
        }
    }
}
