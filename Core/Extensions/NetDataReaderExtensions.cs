using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Extensions
{
    public static class NetDataReaderExtensions
    {
        public static UnityVector3 GetVector3(this NetDataReader reader)
        {
            return new UnityVector3
            {
                X = reader.GetFloat(),
                Y = reader.GetFloat(),
                Z = reader.GetFloat()
            };
        }

        public static UnityQuaternion GetQuaternion(this NetDataReader reader)
        {
            return new UnityQuaternion
            {
                X = reader.GetFloat(),
                Y = reader.GetFloat(),
                Z = reader.GetFloat(),
                W = reader.GetFloat()
            };
        }

        public static Platform GetPlatform(this NetDataReader reader)
        {
            return (Platform)reader.GetByte();
        }
    }
}
