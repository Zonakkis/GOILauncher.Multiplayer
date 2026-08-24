using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    /// <summary>
    /// "我现在用的是这张皮肤"。进游戏读到本地皮肤后发一次，握手完成后补发一次。
    /// 只带清单不带字节，服务端手上没有这个哈希时客户端紧接着上传。
    /// </summary>
    public struct C2SSkinManifestPacket : INetSerializable
    {
        public SkinState State { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(State);
        }

        public void Deserialize(NetDataReader reader)
        {
            State = reader.Get<SkinState>();
        }
    }
}
