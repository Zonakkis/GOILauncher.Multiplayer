using System;
using GOILauncher.Multiplayer.Core.Data.Packets;

namespace GOILauncher.Multiplayer.Core.Data.Serialization
{
    public interface IPacketSerializer<T> where T : IPacket
    {
        byte[] Serialize(T packet);
        T Deserialize(ArraySegment<byte> data);
    }
}