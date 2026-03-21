using GOILauncher.Multiplayer.Core.Data.Packets;
using System;
using System.IO;

namespace GOILauncher.Multiplayer.Core.Data.Serialization
{
    public abstract class BasePacketSerializer<T> : IPacketSerializer<T> where T : IPacket
    {
        public byte[] Serialize(T packet)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    Write(binaryWriter, packet);
                    return memoryStream.ToArray();
                }
            }
        }

        protected abstract void Write(BinaryWriter binaryWriter, T packet);

        public abstract T Deserialize(ArraySegment<byte> data);
    }
}
