using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib.Utils;
using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Extensions
{
    public static class NetDataReaderExtensions
    {
        public static PacketType GetPacketType(this NetDataReader reader)
        {
            return (PacketType)reader.GetByte();
        }

        public static Vector3 GetVector3(this NetDataReader reader)
        {
            return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }

        public static Quaternion GetQuaternion(this NetDataReader reader)
        {
            return new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
    }
}
