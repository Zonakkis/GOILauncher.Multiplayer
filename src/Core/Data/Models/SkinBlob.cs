using GOILauncher.Multiplayer.Core.Data.Constants;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// 一张皮肤贴图的线上表示：内容寻址的 <see cref="SkinHash"/> 加 PNG 字节。
    /// 上传（C2S）和下发（S2C）传的都是这一对，所以字节的线上格式只写在这里。
    /// </summary>
    public struct SkinBlob : INetSerializable
    {
        private static readonly byte[] EmptyData = new byte[0];

        public SkinHash Hash { get; set; }
        public byte[] Data { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Hash);
            writer.PutBytesWithLength(Data ?? EmptyData);
        }

        public void Deserialize(NetDataReader reader)
        {
            Hash = reader.Get<SkinHash>();

            // 长度先看一眼再读：超限的包不该先分配出来再判掉。
            if (reader.AvailableBytes >= sizeof(int) && reader.PeekInt() > SkinConstants.MaxPayloadBytes)
                throw new ParseException("Skin payload exceeds the size limit.");

            // 必须走 TryGetBytesWithLength：GetBytesWithLength 会拿包里写的长度直接
            // BlockCopy，伪造一个长度就能抛出 ArgumentException——那不是 ParseException，
            // PacketDispatcher 不接，会一路冒到 PollEvents 外面去。
            byte[] data;
            if (!reader.TryGetBytesWithLength(out data))
                throw new ParseException("Skin payload is truncated.");

            Data = data;
        }
    }
}
