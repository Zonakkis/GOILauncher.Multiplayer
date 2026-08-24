using System;
using System.Security.Cryptography;
using GOILauncher.Multiplayer.Core.Data.Constants;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// 一张皮肤贴图的内容寻址标识：PNG 字节的 SHA-256。
    /// 全零（包括 <c>default(SkinHash)</c>）表示"没有自定义贴图，按原版渲染"。
    /// </summary>
    /// <remarks>
    /// 用内容哈希而不是版本号，是因为玩家每次重开都可能换皮肤，而"换回上一张"和"换成新的"
    /// 对协议来说必须是两件不同的事：哈希一样就说明字节一样，收端手上那份还能直接用。
    /// 它不用来跨玩家去重——两个人用同一张皮肤的概率接近 0，为此加引用计数不值得。
    /// </remarks>
    public struct SkinHash : INetSerializable, IEquatable<SkinHash>
    {
        private static readonly byte[] EmptyBytes = new byte[SkinConstants.HashLength];

        private byte[] _bytes;

        /// <summary>
        /// 32 字节内容。只读用途（序列化、比较、日志），调用方不得改写返回的数组。
        /// </summary>
        public byte[] Bytes
        {
            get { return _bytes ?? EmptyBytes; }
        }

        public bool IsEmpty
        {
            get
            {
                if (_bytes == null) return true;
                for (var i = 0; i < _bytes.Length; i++)
                {
                    if (_bytes[i] != 0) return false;
                }
                return true;
            }
        }

        public static SkinHash Compute(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return default(SkinHash);

            using (var sha = new SHA256Managed())
            {
                return new SkinHash(sha.ComputeHash(payload));
            }
        }

        private SkinHash(byte[] bytes)
        {
            _bytes = bytes;
        }

        /// <summary>这串字节是不是正好散列成本哈希。</summary>
        public bool Matches(byte[] payload)
        {
            return Equals(Compute(payload));
        }

        public void Serialize(NetDataWriter writer)
        {
            // 定长裸写，不带长度前缀：长度是协议常量，写成变长只会多一个能被伪造的字段。
            writer.Put(Bytes, 0, SkinConstants.HashLength);
        }

        public void Deserialize(NetDataReader reader)
        {
            if (reader.AvailableBytes < SkinConstants.HashLength)
                throw new ParseException("SkinHash is truncated.");

            var bytes = new byte[SkinConstants.HashLength];
            reader.GetBytes(bytes, SkinConstants.HashLength);
            _bytes = bytes;
        }

        public bool Equals(SkinHash other)
        {
            var left = Bytes;
            var right = other.Bytes;
            for (var i = 0; i < SkinConstants.HashLength; i++)
            {
                if (left[i] != right[i]) return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is SkinHash && Equals((SkinHash)obj);
        }

        public override int GetHashCode()
        {
            // 前四个字节就够散列了：SHA-256 的每一位都是均匀的。
            var bytes = Bytes;
            return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
        }

        public override string ToString()
        {
            // 日志里只要能区分两张贴图，不需要全长。
            if (IsEmpty) return "vanilla";
            return BitConverter.ToString(Bytes, 0, 4).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
