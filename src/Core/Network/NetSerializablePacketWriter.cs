using System;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Network
{
    internal static class NetSerializablePacketWriter
    {
        public static byte[] Write(INetSerializable packet)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");

            var writer = new NetDataWriter();
            writer.Put(GetHash(packet.GetType()));
            packet.Serialize(writer);
            return writer.CopyData();
        }

        private static ulong GetHash(Type type)
        {
            ulong hash = 14695981039346656037UL;
            string typeName = type.ToString();
            for (int i = 0; i < typeName.Length; i++)
            {
                hash ^= typeName[i];
                hash *= 1099511628211UL;
            }

            return hash;
        }
    }
}
